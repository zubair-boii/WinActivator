using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.Text;
using System.Windows;

namespace WinActivator.AppUtils
{
    public class Utils
    {
        // determine the operating system e.g, windows 7 ultimate, windows 10 pro
        // It was slow so i didn't use it 
        //public static string GetOSInfo(string info)
        //{
        //    string registryKeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        //    // Open the HKLM key for reading
        //    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKeyPath))
        //    {
        //        if (key != null)
        //        {
        //            switch (info)
        //            {
        //                case "buildID":
        //                    string currentBuild = key.GetValue("CurrentBuild").ToString();
        //                    return currentBuild;

        //                case "productName":
        //                    string productName = key.GetValue("ProductName").ToString();
        //                    return productName;

        //                case "releaseID":
        //                    string releaseId = key.GetValue("ReleaseId").ToString();   // e.g., "2004" or "1709"
        //                    return releaseId;

        //                default:
        //                    throw new Exception("Usage: buildID | producName | releaseID");
        //            }
        //        }
        //        else
        //        {
        //            return "Could not open the registry key.";
        //        }
        //    }
        //}

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

        // fix : can't detect "pro"
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


        // check if windows is activated or not
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

        // execute script files
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

        // find idm installation path from registry
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

        //extract embedded resource to a temp file
        public static string ExtractEmbeddedResource(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    MessageBox.Show($"Error: Embedded resource '{resourceName}' not found. The application might be corrupt.", "Resource Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        // helper function for selecting a folder
        public static string SelectFolder(string description)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = description;
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    return dialog.SelectedPath;
                }
            }
            return null;
        }

        private static bool RunCommand(string fileName, string arguments)
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

        public static bool BackupFiles(string idmPath, string BackupPath, string IdmEXE, string IdmRegistryKey)
        {
            try
            {
                if (Directory.Exists(BackupPath))
                {
                    Directory.Delete(BackupPath, true); // Remove old backup
                }
                Directory.CreateDirectory(BackupPath);

                string idmExePath = Path.Combine(idmPath, IdmEXE);
                string backupExePath = Path.Combine(BackupPath, IdmEXE + ".bak");
                string backupRegPath = Path.Combine(BackupPath, "Registry_Backup.reg");

                if (File.Exists(idmExePath))
                {
                    File.Copy(idmExePath, backupExePath, true);
                }
                else
                {

                }

                // Use reg export command for simplicity and reliability
                if (!RunCommand("reg", $"export \"{IdmRegistryKey}\" \"{backupRegPath}\" /y"))
                {
                    MessageBox.Show("Failed to back up registry keys.", "Backup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create backup: {ex.Message}", "Backup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

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

        // Cleanup manager for temporary files (optional, can be used if you prefer temp extraction)
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
