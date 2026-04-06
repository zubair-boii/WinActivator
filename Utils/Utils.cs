using Microsoft.Win32;
using System;
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
        public static string GetOSInfo(string info)
        {
            string registryKeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            // Open the HKLM key for reading
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKeyPath))
            {
                if (key != null)
                {
                    switch (info)
                    {
                        case "buildID":
                            string currentBuild = key.GetValue("CurrentBuild").ToString();
                            return currentBuild;

                        case "productName":
                            string productName = key.GetValue("ProductName").ToString();
                            return productName;

                        case "releaseID":
                            string releaseId = key.GetValue("ReleaseId").ToString();   // e.g., "2004" or "1709"
                            return releaseId;

                        default:
                            throw new Exception("Usage: buildID | producName | releaseID");
                    }
                }
                else
                {
                    return "Could not open the registry key.";
                }
            }
        }

        // check if windows is activated or not
        public static string GetWindowsActivationStatus()
        {
            try
            {
                // Query the SoftwareLicensingProduct class for the current OS instance
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                    "SELECT * FROM SoftwareLicensingProduct WHERE PartialProductKey <> null AND ApplicationId='55c92734-d682-4d71-983e-d6ec3f16059f' AND LicenseIsAddon=False");

                foreach (ManagementObject wmiObject in searcher.Get())
                {
                    // The LicenseStatus property is a UInt32
                    uint licenseStatus = (uint)wmiObject["LicenseStatus"];

                    switch (licenseStatus)
                    {
                        case 0:
                            return "Unlicensed"; // Windows is not activated
                        case 1:
                            return "Licensed"; // Windows is activated
                        case 2:
                            return "OOBGrace"; 
                        case 3:
                            return "OOTGrace"; 
                        case 4:
                            return "NonGenuineGrace";
                        case 5:
                            return "Notification";
                        case 6:
                            return "ExtendedGrace";
                        default:
                            return "Unknown";
                    }
                }
                return "Status not found";
            }
            catch (ManagementException e)
            {
                MessageBox.Show($"An error occurred while querying WMI: {e.Message}");
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
    }
}
