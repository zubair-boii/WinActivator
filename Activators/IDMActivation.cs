using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

using WinActivator.AppUtils;
using MessageBox = System.Windows.MessageBox;

namespace WinActivator.Activators
{
    public class IDMActivation
    {
        // thanks to oop7 for the code. link: https://codeberg.org/oop7/IDM-Activator

        private const string DefaultIdmPathX86 = "C:\\Program Files (x86)\\Internet Download Manager";
        private const string IdmExecutable = "IDMan.exe";
        private const string IdmProcessName = "IDMan";
        private const string BackupDirName = "backup";
        private const string IdmRegistryKey = @"HKEY_CURRENT_USER\Software\DownloadManager";
        private const string IdmVersionValueName = "idmvers";
        private static readonly List<string> SupportedVersions = new List<string> { "v6.42b64 Full", "v6.42b64 Trial", "v6.42b64" };

        // Embedded Resources for idm activation
        private const string ResDataBin = "WinActivator.Resources.IDMActivationResources.data.bin";
        private const string ResRegistryBin = "WinActivator.Resources.IDMActivationResources.Registry.bin";
        //private const string ResExtensionsBin = "WinActivator.Resources.IDMActivationResources.bin";

        // Store paths to extracted temp files
        private List<string> _tempFiles = new List<string>();

        private static string BackupPath => Path.Combine(Utils.GetIDMPath(), BackupDirName);

        public static void ActivateIDM()
        {
            string idmPath = Utils.GetIDMPath();

            if (!ValidateIDMPath(idmPath))
            {
                MessageBox.Show(
                    "IDM installation not found. Select correct folder.",
                    "Not Found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                idmPath = Utils.SelectFolder("Select a folder");
                if (!ValidateIDMPath(idmPath))
                    return;
            }

            string tempData = null;
            string tempReg = null;

            try
            {
                tempData = Utils.ExtractEmbeddedResource(ResDataBin);
                Utils.TempCleanupManager.Register(tempData);
                tempReg = Utils.ExtractEmbeddedResource(ResRegistryBin);
                Utils.TempCleanupManager.Register(tempReg);

                if (tempData == null || tempReg == null)
                    return;

                if (!Utils.BackupFiles(idmPath, BackupPath, IdmExecutable, IdmRegistryKey))
                {
                    MessageBox.Show("Backup failed.");
                    return;
                }

                if (!CheckIDMVersion())
                {
                    MessageBox.Show("Unsupported version.");
                    return;
                }

                if (IsIDMRunning() == true)
                {
                    TerminateIdmProcess();
                }

                if (!CopyActivationFile(idmPath, tempData))
                {
                    MessageBox.Show("File copy failed.");
                    return;
                }

                if (!Utils.ImportRegistryFile(tempReg))
                {
                    MessageBox.Show("Registry import failed.");
                    return;
                }

                MessageBox.Show("IDM Activated successfully", "Activation Succesfull.");
            }
            finally
            {
                Utils.TempCleanupManager.CleanupAll();
            }
        }

        public static bool ValidateIDMPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path) || !File.Exists(Path.Combine(path, IdmExecutable)))
            {
                MessageBox.Show($"Error: IDM installation not found or {IdmExecutable} is missing in the specified path:\n{path}", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public static bool CheckIDMVersion()
        {
            try
            {
                string version = (string)Registry.GetValue(IdmRegistryKey, IdmVersionValueName, null);

                if (string.IsNullOrEmpty(version))
                {
                    MessageBox.Show($"Could not read IDM version from registry key:\n{IdmRegistryKey}\\{IdmVersionValueName}\n\nActivation might fail if version is incompatible.", "Version Check Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // Allow continuing but warn user
                    return true;
                }

                if (!SupportedVersions.Contains(version))
                {
                    MessageBox.Show($"Unsupported IDM version detected: {version}\nSupported versions: {string.Join(", ", SupportedVersions)}\n\nPlease install the correct IDM version or update the activator.", "Unsupported Version", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Allow continuing but warn user
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error occurred while checking IDM version:\n{e.Message}", "Version Check Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private static bool TerminateIdmProcess()
        {
            try
            {
                bool terminated = false;
                foreach (var process in Process.GetProcessesByName(IdmProcessName))
                {
                    try
                    {
                        process.Kill();
                        process.WaitForExit(5000); // Wait up to 5 seconds
                        terminated = true;
                    }
                    catch (Exception ex)
                    {
                        // Handle cases where process might have already exited or access denied
                        MessageBox.Show($"Warning: Failed to terminate IDM process (PID: {process.Id}): {ex.Message}", "Process Termination Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
                if (!terminated)
                {
                   
                    return false;
                }
                // Short pause to ensure process is fully terminated
                Thread.Sleep(500);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error terminating IDM process: {ex.Message}", "Process Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private static bool CopyActivationFile(string idmPath, string tempSourcePath)
        {
            try
            {
                string destPath = Path.Combine(idmPath, IdmExecutable);
                File.Copy(tempSourcePath, destPath, true);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying activation file ({Path.GetFileName(tempSourcePath)}) to {idmPath}: {ex.Message}\n\nEnsure IDM is closed and you have permissions.", "File Copy Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool IsIDMRunning()
        {
            return Process.GetProcessesByName("IDMan").Any();
        }


    }

}




