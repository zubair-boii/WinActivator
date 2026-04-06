using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using WinActivator.AppUtils;

namespace WinActivator.Activators
{
    public class TSForgeActivation
    {
        public static void ActivateWindows()
        {
            
            Utils.RunEmbeddedCmd("WinActivator.Scripts.TSforge_Activation.cmd", "TSforge_Activation.cmd", "/Z-Windows");
        }

        public static void ActivateOffice()
        {

            Utils.RunEmbeddedCmd("WinActivator.Scripts.TSforge_Activation.cmd", "TSforge_Activation.cmd", "/Z-Office");
        }
    }
}
