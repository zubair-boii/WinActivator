using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WinActivator.Services
{
    public static class DialogService
    {
        public static bool Confirm(string message)
        {
            return HandyControl.Controls.MessageBox.Show(
                message,
                "Confirm",
                MessageBoxButton.YesNo)
                == MessageBoxResult.Yes;
        }
    }
}
