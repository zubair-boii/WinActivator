using System;
using System.ComponentModel;
using System.Threading.Tasks;
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
            Loaded += MainWindow_Loaded;
        }

        private async void UpdateText()
        {
            OSIdentifierTxt.Text = "Loading...";
            ActivationStatusTxt.Text = "Checking...";

            try
            {
                var result = await Task.Run(() =>
                {
                    string osInfo = AppUtils.Utils.GetOSInfo("productName");
                    string status = AppUtils.Utils.GetWindowsActivationStatus();

                    return (osInfo, status);
                });

                OSIdentifierTxt.Text = result.osInfo ?? "NULL";
                ApplyActivationStatus(result.status);
            }
            catch (Exception ex)
            {
                ActivationStatusTxt.Text = "Error";
                MessageBox.Show(ex.ToString()); // IMPORTANT
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateText();
        }

        private void ApplyActivationStatus(string osActivationStatus)
        {
            if (osActivationStatus == "Licensed")
            {
                ActivationStatusTxt.Text = osActivationStatus + " Healthy";
                ActivationStatusTxt.Foreground = Brushes.LimeGreen;
            }
            else if (osActivationStatus == "Unlicensed")
            {
                ActivationStatusTxt.Text = "Windows is not activated";
                ActivationStatusTxt.Foreground = Brushes.Red;
            }
            else if (osActivationStatus == "Notification")
            {
                ActivationStatusTxt.Text = "Windows Not Activated";
                ActivationStatusTxt.Foreground = Brushes.Red;
            }
            else
            {
                ActivationStatusTxt.Text = osActivationStatus ?? "Unknown Status";
                ActivationStatusTxt.Foreground = Brushes.Orange;
            }
        }



        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (TSforgeWindowsRadioBtn.IsChecked != true)
                TSforgeWindowsRadioBtn.IsChecked = true;
            else
                TSforgeWindowsRadioBtn.IsChecked = false;
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

        private async void ActivateButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedMethod = null;

            if (TSforgeWindowsRadioBtn.IsChecked == true)
                selectedMethod = TSforgeWindowsRadioBtn.Name;
            else if (TSforgeOfficeRadioBtn.IsChecked == true)
                selectedMethod = TSforgeOfficeRadioBtn.Name;
            else if (hwidRadioBtn.IsChecked == true)
                selectedMethod = hwidRadioBtn.Name;
            else if (OhookRadioBtn.IsChecked == true)
                selectedMethod = OhookRadioBtn.Name;
            else if (idmActivationRadioBtn.IsChecked == true)
                selectedMethod = idmActivationRadioBtn.Name;

            if (selectedMethod == null)
            {
                MessageBox.Show("Please select an activation method.", "Information");
                return;
            }

            ActivateButton.IsEnabled = false;

            await Task.Run(() =>
            {
                switch (selectedMethod)
                {
                    case "TSforgeWindowsRadioBtn":
                        TSForgeActivation.ActivateWindows();
                        
                        break;

                    case "TSforgeOfficeRadioBtn":
                        TSForgeActivation.ActivateOffice();

                        break;

                    case "OhookRadioBtn":
                        OhookActivation.ActivateOffice();

                        break;

                    case "hwidRadioBtn":
                        HWIDActivation.ActivateWindows();

                        break;

                    case "idmActivationRadioBtn":
                        IDMActivation.ActivateIDM();
                        break;
                }
            });

            // 2. Now that we are back on the UI thread (thanks to await), 
            // it is safe to update the text.
            UpdateText();

            ActivateButton.IsEnabled = true;
        }

        private void idmActivation_CardLeftClick(object sender, MouseButtonEventArgs e)
        {
            if (idmActivationRadioBtn.IsChecked != true)
                idmActivationRadioBtn.IsChecked = true;
            else
                idmActivationRadioBtn.IsChecked = false;
        }
    }
}
