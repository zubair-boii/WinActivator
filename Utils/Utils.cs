using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace WinActivator.AppUtils
{
    public class Utils
    {

        /// <summary>
        /// Method to get information about the operating system based on the provided parameter. It reads from the Windows registry to retrieve details such as build ID, product name, edition ID, registered owner, and release ID. If the requested information is not available or if an invalid parameter is provided, it returns "Unknown" or "Invalid parameter" accordingly.
        /// </summary>
        /// <param name="info">The information to retrieve (buildID, productName, editionId, registeredOwner, releaseID).</param>
        /// <returns>The requested operating system information or an error message.</returns>
        public static string GetOSInfo(string info)
        {
            const string path = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

            using (var key = Registry.LocalMachine.OpenSubKey(path))
            {
                if (key == null)
                    return "Unknown";

                if (info == "buildID")
                    return key.GetValue("CurrentBuild") != null ? key.GetValue("CurrentBuild").ToString() : "Unknown";

                else if (info == "productName")
                    return GetWindowsEdition(); // 🔥 FIXED PART

                else if (info == "editionId")
                    return key.GetValue("EditionId") != null ? key.GetValue("EditionId").ToString() : "Unknown";

                else if (info == "registeredOwner")
                    return key.GetValue("RegisteredOwner") != null ? key.GetValue("RegisteredOwner").ToString() : "Unknown";

                else if (info == "releaseID")
                    return key.GetValue("ReleaseId") != null ? key.GetValue("ReleaseId").ToString() : "Unknown";

                else
                    return "Invalid parameter";
            }
        }

        /// <summary>
        /// Gets the Windows edition.
        /// </summary>
        /// <returns>The Windows edition.</returns>
        public static string GetWindowsEdition()
        {
            try
            {
                using (var searcher =
                    new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject os in searcher.Get())
                    {
                        return os["Caption"]?.ToString() ?? "Unknown";
                    }
                }
            }
            catch
            {
                return "Unknown";
            }

            return "Unknown";
        }


       /// <summary>
       /// Checks wheter windows is activated or not 
       /// </summary>
       /// <returns>different strings based on the activation status</returns>
        public static string GetWindowsActivationStatus()
        {
            try
            {
                // FAST PATH (registry)
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SoftwareProtectionPlatform"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("BackupProductKeyDefault");

                        // If key exists, system is usually activated
                        if (val != null && !string.IsNullOrEmpty(val.ToString()))
                        {
                            return "Licensed";
                        }
                    }
                }

                // FALLBACK (optimized WMI — minimal query)
                using (var searcher = new System.Management.ManagementObjectSearcher(
                    "SELECT LicenseStatus FROM SoftwareLicensingProduct WHERE ApplicationID='55c92734-d682-4d71-983e-d6ec3f16059f' AND PartialProductKey IS NOT NULL"))
                {
                    foreach (System.Management.ManagementObject obj in searcher.Get())
                    {
                        int status = Convert.ToInt32(obj["LicenseStatus"]);

                        switch (status)
                        {
                            case 0: return "Unlicensed";
                            case 1: return "Licensed";
                            case 2: return "OOBGrace";
                            case 3: return "OOTGrace";
                            case 4: return "NonGenuineGrace";
                            case 5: return "Notification";
                            case 6: return "ExtendedGrace";
                            default: return "Unknown";
                        }
                    }
                }

                return "Unknown";
            }
            catch
            {
                return "Error";
            }
        }

        /// <summary>
        /// Extracts an embedded command file from the assembly, saves it to a safe location, and executes it with optional arguments and admin privileges.
        /// </summary>
        /// <param name="resourceName">The name of the embedded resource to extract.</param>
        /// <param name="cmdFileName">The name of the command file to create.</param>
        /// <param name="extraArgs">Additional arguments for the command.</param>
        /// <param name="runAsAdmin">Indicates whether to run the command as an administrator.</param>
        public static void RunEmbeddedCmd(string resourceName, string cmdFileName, string extraArgs = "", bool runAsAdmin = false)

        {
            // 1. Prepare safe directory (NOT temp)
            string baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WinActivator"
            );

            string cmdPath = Path.Combine(baseDir, cmdFileName);

            try
            {
                Directory.CreateDirectory(baseDir);

                // 2. Extract resource
                using (var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new Exception("Resource not found: " + resourceName);

                    using (var reader = new StreamReader(stream))
                    {
                        string content = reader.ReadToEnd();

                        // Write with safe encoding
                        File.WriteAllText(cmdPath, content, Encoding.ASCII);
                    }
                }

                // 3. Remove "blocked" flag (just in case)
                try { File.Delete(cmdPath + ":Zone.Identifier"); } catch { }

                // 4. Execute
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c \"" + cmdPath + "\" " + extraArgs,
                    WorkingDirectory = Path.GetDirectoryName(cmdPath)
                };

                if (runAsAdmin)
                {
                    psi.UseShellExecute = true;
                    psi.Verb = "runas"; // UAC prompt
                }
                else
                {
                    psi.UseShellExecute = false;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.CreateNoWindow = true;
                }

                var process = Process.Start(psi);

                // 5. Optional: capture output (only if NOT admin mode)
                if (!runAsAdmin)
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();


                }
            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error:\n" + ex.Message);
            }
            finally
            {
                try
                {
                    if (File.Exists(cmdPath))
                    {
                        File.Delete(cmdPath);
                    }

                    if (Directory.Exists(baseDir) &&
                        Directory.GetFiles(baseDir).Length == 0)
                    {
                        Directory.Delete(baseDir);
                    }
                }
                catch
                {

                }


                // FORCE GC (optional, not usually required)
                GC.Collect();
                GC.WaitForPendingFinalizers();

            }
        }

        /// <summary>
        /// Attempts to locate the installation directory of Internet Download Manager (IDM) by checking registry locations.
        /// </summary>
        /// <returns>The path to the IDM installation directory or null if not found.</returns>
        public static string GetIDMPath()
        {
            // 1. HKCU (MOST RELIABLE in your system)
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\DownloadManager"))
            {
                string exePath = key?.GetValue("ExePath")?.ToString();

                if (!string.IsNullOrWhiteSpace(exePath))
                {
                    string dir = Path.GetDirectoryName(exePath);

                    if (Directory.Exists(dir))
                        return dir;

                    // fallback: even if file missing, still trust registry
                    return dir;
                }
            }

            // 2. HKLM 32-bit view
            using (RegistryKey baseKey =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
            using (RegistryKey key = baseKey.OpenSubKey(@"SOFTWARE\Internet Download Manager"))
            {
                string installPath = key?.GetValue("InstallPath")?.ToString();

                if (!string.IsNullOrWhiteSpace(installPath))
                    return installPath;
            }

            // 3. HKLM Wow6432Node fallback
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Wow6432Node\Internet Download Manager"))
            {
                string installPath = key?.GetValue("InstallPath")?.ToString();

                if (!string.IsNullOrWhiteSpace(installPath))
                    return installPath;
            }

            return null;
        }

        /// <summary>
        /// Extracts an embedded resource from the assembly and saves it to a temporary file. Returns the path to the temporary file or null if extraction fails. The method handles exceptions and provides user feedback if the resource cannot be found or extracted.
        /// </summary>
        /// <param name="resourceName">The name of the embedded resource to extract.</param>
        /// <returns>The path to the temporary file or null if extraction fails.</returns>
        public static string ExtractEmbeddedResource(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                
                if (stream == null)
                {
                    return null;
                }

                string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(resourceName));
                try
                {
                    using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fileStream);
                    }
                    return tempFilePath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error extracting resource '{resourceName}' to temporary file: {ex.Message}", "Extraction Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Attempt to clean up partially created file
                    if (File.Exists(tempFilePath)) try { File.Delete(tempFilePath); } catch { }
                    return null;
                }
            }
        }

        /// <summary>
        /// Helper method to select a folder using FolderBrowserDialog. Returns the selected path or null if cancelled.
        /// </summary>
        /// <param name="description">The description for the folder browser dialog.</param>
        /// <returns>The selected folder path or null if cancelled.</returns>
        public static string SelectFolder(string description)
        {
            // Using declaration ensures the dialog is automatically and promptly disposed
            var dialog = new FolderBrowserDialog
            {
                Description = description,
                ShowNewFolderButton = false,
                RootFolder = Environment.SpecialFolder.MyComputer,
            };

            return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
        }

        /// <summary>
        /// Runs a command-line process with the specified file name and arguments. waits for exit.
        /// </summary>
        /// <param name="fileName">The name of the file to execute.</param>
        /// <param name="arguments">The arguments for the command-line process.</param>
        /// <returns>Returns true if the process exits successfully, false otherwise.</returns>
        public static bool RunCommand(string fileName, string arguments)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true, // Optional: capture output if needed
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit(); // Wait for the command to complete
                    // string output = process.StandardOutput.ReadToEnd(); // Optional
                    // string errors = process.StandardError.ReadToEnd(); // Optional
                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running command: {fileName} {arguments}\n{ex.Message}", "Command Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        /// <summary>
        /// Imports a registry file using regedit.exe with silent mode. It handles exceptions and provides user feedback if the import process fails. The method ensures that the registry file is correctly quoted to handle spaces in the path.
        /// </summary>
        /// <param name="tempRegFilePath">The path to the temporary registry file to import.</param>
        /// <returns>Returns true if the import is successful, false otherwise.</returns>
        public static bool ImportRegistryFile(string tempRegFilePath)
        {
            string regFileName = Path.GetFileName(tempRegFilePath);
            //UpdateStatus($"Importing registry file ({regFileName})...");
            string arguments = $"/s \"{tempRegFilePath}\""; // Correct way to quote path for regedit

            if (!RunCommand("regedit.exe", arguments))
            {
                MessageBox.Show($"Failed to import registry file: {regFileName}", "Registry Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// A helper class to manage temporary files created during the application's execution. It allows registering temporary file paths and provides a method to clean up all registered files at once. The SafeDelete method is used to attempt deletion of files while ignoring any exceptions that may occur due to locked files or access issues.
        /// </summary>
        public static class TempCleanupManager
        {
            private static readonly List<string> _tempFiles = new List<string>();

            public static void Register(string path)
            {
                if (!string.IsNullOrWhiteSpace(path))
                    _tempFiles.Add(path);
            }

            public static void CleanupAll()
            {
                for (int i = _tempFiles.Count - 1; i >= 0; i--)
                {
                    SafeDelete(_tempFiles[i]);
                }

                _tempFiles.Clear();
            }

            public static void SafeDelete(string path)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(path))
                        return;

                    if (File.Exists(path))
                        File.Delete(path);
                }
                catch
                {
                    // ignore locked files / access issues
                }
            }
        }
    }
}
