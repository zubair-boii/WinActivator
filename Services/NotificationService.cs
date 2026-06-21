using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinActivator.Services
{
    public static class NotificationService
    {
        public static void Success(string message)
        {
            Growl.Success(message);
        }

        public static void Error(string message)
        {
            Growl.Error(message);
        }

        public static void Warning(string message)
        {
            Growl.Warning(message);
        }

        public static void Info(string message)
        {
            Growl.Info(message);
        }
    }
}
