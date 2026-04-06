using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WinActivator.AppUtils;

namespace WinActivator.Activators
{
    public class OhookActivation
    {
        public static void ActivateWindows()
        {
            Utils.RunEmbeddedCmd("WinActivator.Scripts.Ohook_Activation.cmd", "Ohook_Activation.cmd", "/Ohook");
        }
    }
}
