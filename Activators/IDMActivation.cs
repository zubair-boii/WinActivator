using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using HandyControl.Controls;

using WinActivator.AppUtils;
using MessageBox = System.Windows.MessageBox;
using WinActivator.Services;

namespace WinActivator.Activators
{
    public class IDMActivation
    {
        // thanks to oop7 for the code. link: https://codeberg.org/oop7/IDM-Activator

        // default installation path for IDM (recommended)
        private const string DefaultIdmPathX86 = "C:\\Program Files (x86)\\Internet Download Manager";

        // IDM execuatabe name
        private const string IdmExecutable = "IDMan.exe";

        // IDMGrHlp executable name
        private const string IdmGrHlpExecutable = "IDMGrHlp.exe";

        // IDM process name (without .exe extension)
        private const string IdmProcessName = "IDMan";

        // Backup directory name for original files and registry keys to store.
        private const string BackupDirName = "backup";

        // Registry key and value name for IDM version, used for backup and validation purposes during activation.
        private const string IdmRegistryKey = @"HKEY_CURRENT_USER\Software\DownloadManager";

        //Store paths to extracted temp files
        private List<string> _tempFiles = new List<string>();

        // path to backup directory for original files and registry keys
        private static string BackupPath => Path.Combine(Utils.GetIDMPath(), BackupDirName);

        // idm installation path
        private static string idmPath = null;

        // temp variables to hold paths of extracted resources, will be cleaned up in finally block
        private static string tempData = null;
        private static string tempReg = null;
        private static string tempExtensions = null;
        private static string tempDataHlp = null;

        // Embedded Resources for idm activation
        private const string ResDataBin = "WinActivator.Resources.IDMActivationResources.ata.bin";
        private const string ResRegistryBin = "WinActivator.Resources.IDMActivationResources.egistry.bin";
        private const string ResExtensionsBin = "WinActivator.Resources.IDMActivationResources.extensions.bin";
        private const string ResDataHlp = "WinActivator.Resources.IDMActivationResources.dataHlp.bin";

        /// <summary>
        /// This is the method that actually activates the IDM.
        /// </summary>
        public static void ActivateIDM()
        {
            // get idm installation path from registry or default path
            idmPath = Utils.GetIDMPath() ?? DefaultIdmPathX86;

            // validate the idm path
            if (!ValidateIDMPath(idmPath))
            {
                // Safely fetch the folder path from the UI Thread
                idmPath = System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    Utils.SelectFolder("Please Select your IDM installation folder.")
                );

                // Strict validation check after user interaction
                if (string.IsNullOrEmpty(idmPath) || !ValidateIDMPath(idmPath))
                {
                    return;
                }
            }

            try
            {
                // extract resources to temp files 
                tempData = Utils.ExtractEmbeddedResource(ResDataBin);
                tempReg = Utils.ExtractEmbeddedResource(ResRegistryBin);
                tempExtensions = Utils.ExtractEmbeddedResource(ResExtensionsBin);
                tempDataHlp = Utils.ExtractEmbeddedResource(ResDataHlp);

                // validate that the resources were extracted successfully
                if (tempData == null || tempReg == null || tempExtensions == null)
                {
                    //MessageBox.Show("Failed to extract necessary resources for activation.", "Resource Extraction Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    NotificationService.Error("Failed to extract necessary resources.");
                    return;
                }

                // register files for cleanup
                Utils.TempCleanupManager.Register(tempData);
                Utils.TempCleanupManager.Register(tempReg);
                Utils.TempCleanupManager.Register(tempExtensions);
                Utils.TempCleanupManager.Register(tempDataHlp);

                // checks if idm is running and terminates it to prevent file access issues during activation
                if (IsIDMRunning() == true)
                {
                    // kill idm process
                    TerminateIdmProcess();
                }


                // backup original files and registry key before making any changes, if backup fails, abort activation to prevent potential data loss
                if (!BackupFiles(idmPath, BackupPath, IdmExecutable, IdmRegistryKey, IdmGrHlpExecutable))
                {
                    NotificationService.Error("Backup failed. Activation aborted.");
                    return;
                }

                // add the extensions to registry
                if (!Utils.ImportRegistryFile(tempExtensions))
                {
                    NotificationService.Error("Failed to add file extensions. Activation aborted.");
                    return;
                }

                // copy the activation file to the idm directory, if copy fails, abort activation to prevent partial activation state
                if(!CopyActivationFiles())
                {
                    NotificationService.Error("Failed to copy activation file to IDM directory. Activation aborted.");
                    return;
                }


                // import the registry settings from the extracted registry file, if import fails, abort activation to prevent inconsistent registry state
                if (!Utils.ImportRegistryFile(tempReg))
                {
                    NotificationService.Error("Registry import failed. Activation aborted.");
                    return;
                }

                NotificationService.Success("IDM Activated successfully. Enjoy!");
            }
            finally
            {
                Utils.TempCleanupManager.CleanupAll();
            }
        }

        /// <summary>
        /// Validates the provided IDM installation path by checking if it exists and contains the required executable.
        /// </summary>
        /// <param name="path">this is the string path to validate</param>
        /// <returns></returns>
        public static bool ValidateIDMPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path) || !File.Exists(Path.Combine(path, IdmExecutable)))
            {
                NotificationService.Error("IDM installation path is invalid. Please select correct path to idm folder.");
                return false;
            }
            return true;
        }


        /// <summary>
        /// Terminates all running instances of the IDM process (IDMan.exe) to ensure that files can be modified during activation. It handles exceptions and provides user feedback if termination fails.
        /// </summary>
        /// <returns>Returns true if all instances were terminated successfully, false otherwise.</returns>
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


        /// <summary>
        /// Copies the activation file from the temporary source path to the IDM installation directory. It handles exceptions and provides user feedback if the copy operation fails.
        /// </summary>
        /// <param name="idmPath">The path to the IDM installation directory.</param>
        /// <param name="tempSourcePath">The path to the temporary activation file.</param>
        /// <returns>Returns true if the file was copied successfully, false otherwise.</returns>
        private static bool CopyActivationFiles()
        {
            try
            {
                if (string.IsNullOrEmpty(idmPath))
                {
                    MessageBox.Show("IDM path is not set.");
                    return false;
                }

                if (string.IsNullOrEmpty(tempData) || !File.Exists(tempData))
                {
                    MessageBox.Show("Activation resource file not found.");
                    return false;
                }


                // Copy and overwrite existing file
                string idmanDest = Path.Combine(idmPath, "IDMan.exe");
                string grhlpDest = Path.Combine(idmPath, "IDMGrHlp.exe");

                File.Copy(tempData, idmanDest, true);
                File.Copy(tempDataHlp, grhlpDest, true);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                $"Copy failed:\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                return false;
            }
        }


        /// <summary>
        /// Checks if any instances of the IDM process (IDMan.exe) are currently running. This is used to determine if the process needs to be terminated before activation.
        /// </summary>
        /// <returns>Returns true if any instances are running, false otherwise.</returns>
        public static bool IsIDMRunning()
        {
            return Process.GetProcessesByName("IDMan").Any();
        }


        /// <summary>
        /// Backs up the IDM executable and relevant registry keys to a specified backup directory.
        /// </summary>
        /// <param name="idmPath">The path to the IDM installation directory.</param>
        /// <param name="BackupPath">The path to the backup directory.</param>
        /// <param name="IdmEXE">The name of the IDM executable file.</param>
        /// <param name="IdmRegistryKey">The registry key to back up.</param>
        /// <returns>Returns true if the backup is successful, false otherwise.</returns>
        public static bool BackupFiles(string idmPath, string BackupPath, string IdmEXE, string IdmRegistryKey, string IdmGrHlpExecutable)
        {
            try
            {
                if (Directory.Exists(BackupPath))
                {
                    Directory.Delete(BackupPath, true); // Remove old backup
                }
                Directory.CreateDirectory(BackupPath);

                string idmExePath = Path.Combine(idmPath, IdmEXE);
                string idmGrHlpExePath = Path.Combine(idmPath, IdmGrHlpExecutable);

                string backupExePath = Path.Combine(BackupPath, IdmEXE + ".bak");
                string backupRegPath = Path.Combine(BackupPath, "Registry_Backup.reg");
                string backupIdmGrHlpPath = Path.Combine(BackupPath, IdmGrHlpExecutable + ".bak");

                if (File.Exists(idmExePath) && File.Exists(idmGrHlpExePath))
                {
                    File.Copy(idmExePath, backupExePath, true);
                    File.Copy(idmGrHlpExePath, backupIdmGrHlpPath, true);
                }
                else
                {

                }

                // Use reg export command for simplicity and reliability
                if (!Utils.RunCommand("reg", $"export \"{IdmRegistryKey}\" \"{backupRegPath}\" /y"))
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
    }

}