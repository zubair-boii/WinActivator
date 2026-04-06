

using System.Windows;
using WinActivator.AppUtils;

namespace WinActivator.Activators
{
    public class HWIDActivation
    {
        public static void ActivateWindows()
        {
            Utils.RunEmbeddedCmd("WinActivator.Scripts.HWID_Activation.cmd", "HWID_Activation.cmd", "/HWID");
        }
    }
}
