using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WinActivator.Activators;
using WinActivator.AppUtils;

namespace WinActivator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string osInfo = AppUtils.Utils.GetOSInfo("productName");
            OSIdentifierTxt.Text = osInfo;

            string osActivationStatus = AppUtils.Utils.GetWindowsActivationStatus();
            if (osActivationStatus == "Licensed")
            {
                ActivationStatusTxt.Text = osActivationStatus + " Healthy";
                ActivationStatusTxt.Foreground = new SolidColorBrush(Colors.LimeGreen);
            }
            else if (osActivationStatus == "Unlicensed")
            {
                ActivationStatusTxt.Text = "Windows is not activated";
                ActivationStatusTxt.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (osActivationStatus == "Pre-Activation Trial")
            {
                ActivationStatusTxt.Text = "OOBGrace";
                ActivationStatusTxt.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else if (osActivationStatus == "OOTGrace")
            {
                ActivationStatusTxt.Text = "OOTGrace";
                ActivationStatusTxt.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else if (osActivationStatus == "NonGenuineGrace")
            {
                ActivationStatusTxt.Text = "NonGenuineGrace";
                ActivationStatusTxt.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else if (osActivationStatus == "Notification")
            {
                ActivationStatusTxt.Text = "Windows Not Activated";
                ActivationStatusTxt.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (osActivationStatus == "ExtendedGrace")
            {
                ActivationStatusTxt.Text = "ExtendedGrace";
                ActivationStatusTxt.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                ActivationStatusTxt.Text = "Unknown Status";
                ActivationStatusTxt.Foreground = new SolidColorBrush(Colors.Orange);
            }

        }



        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (tsforgeRadioBtn.IsChecked != true)
                tsforgeRadioBtn.IsChecked = true;
            else
                tsforgeRadioBtn.IsChecked = false;
        }

        private void Ohook_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (OhookRadioBtn.IsChecked != true)
                OhookRadioBtn.IsChecked = true;
            else
                OhookRadioBtn.IsChecked = false;
        }

        private void hwid_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            if (hwidRadioBtn.IsChecked != true)
                hwidRadioBtn.IsChecked = true;
            else
                hwidRadioBtn.IsChecked = false;
        }

        private void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedMethod = null;

            if (tsforgeRadioBtn.IsChecked == true)
                selectedMethod = tsforgeRadioBtn.Name;
            else if (hwidRadioBtn.IsChecked == true)
                selectedMethod = hwidRadioBtn.Name;
            else if (OhookRadioBtn.IsChecked == true)
                selectedMethod = OhookRadioBtn.Name;

            if (selectedMethod == null)
                MessageBox.Show("Please select an activation method.", "Information");


            switch (selectedMethod)
            {
                case "tsforgeRadioBtn":
                    TSForgeActivation.ActivateOffice();
                    break;

                case "OhookRadioBtn":
                    OhookActivation.ActivateWindows();
                    break;

                case "hwidRadioBtn":
                    HWIDActivation.ActivateWindows();
                    break;
            }

        }
    }
}
