using HandyControl.Controls;
using HandyControl.Data;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WinActivator.Activators;

namespace WinActivator
{
    public partial class MainWindow : System.Windows.Window
    {

        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            Loaded += DashboardView_Loaded;
        }

        private void DashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateText();
        }

        private async void UpdateText()
        {
            OSIdentifierTxt.Text = "Loading system variables...";
            MainWindow.Instance?.UpdateSystemStatusState("Polling background OS architecture parameters...", Brushes.Orange);

            try
            {
                var result = await Task.Run(() =>
                {
                    string osInfo = AppUtils.Utils.GetOSInfo("productName");
                    string status = AppUtils.Utils.GetWindowsActivationStatus();
                    return (osInfo, status);
                });

                OSIdentifierTxt.Text = result.osInfo ?? "Target System Profile Corrupted";
                ApplyActivationStatus(result.status);
            }
            catch (Exception ex)
            {
                ActivationStatusTxt.Text = "Exception Encountered";
                MainWindow.Instance?.UpdateSystemStatusState("Fatal environmental diagnostic exception occurred.", Brushes.Red);
                System.Windows.MessageBox.Show(ex.ToString(), "Diagnostic Exception Context");
            }
        }

        private void ApplyActivationStatus(string osActivationStatus)
        {
            if (osActivationStatus == "Licensed")
            {
                ActivationStatusTxt.Text = "ENVIRONMENT HEALTHY / ACTIVE";
                ActivationStatusTxt.Foreground = Brushes.LimeGreen;
                MainWindow.Instance?.UpdateSystemStatusState("All environment licenses validated.", Brushes.LimeGreen);
            }
            else if (osActivationStatus == "Unlicensed" || osActivationStatus == "Notification")
            {
                ActivationStatusTxt.Text = "ATTENTION REQUIRED: LICENSE INACTIVE";
                ActivationStatusTxt.Foreground = Brushes.Crimson;
                MainWindow.Instance?.UpdateSystemStatusState("Licensing framework requires administrative resolution.", Brushes.Crimson);
            }
            else
            {
                ActivationStatusTxt.Text = (osActivationStatus ?? "UNKNOWN SUITE CONTEXT").ToUpper();
                ActivationStatusTxt.Foreground = Brushes.DarkOrange;
                MainWindow.Instance?.UpdateSystemStatusState("Ready for script runtime assignment.", Brushes.DarkOrange);
            }
        }

        //private async void ActivateButton_Click(object sender, RoutedEventArgs e)
        //{
        //    string selectedMethod = null;

        //    if (Hwid.IsOption1Checked) selectedMethod = "hwid";
        //    else if (OhookCard.IsOption1Checked) selectedMethod = "Ohook";
        //    else if (TsForgeCard.IsOption1Checked) selectedMethod = "TSforgeWindows";
        //    else if (TsForgeCard.IsOption2Checked) selectedMethod = "TSforgeOffice";
        //    else if (IdmCard.IsOption1Checked) selectedMethod = "idmActivation";

        //    if (selectedMethod == null)
        //    {
        //        System.Windows.MessageBox.Show("Please isolate an operational target package sequence before calling automation triggers.", "Workspace Information Context");
        //        return;
        //    }

        //    ActivateButton.IsEnabled = false;
        //    MainWindow.Instance?.UpdateSystemStatusState($"Initializing background script task sequence: [{selectedMethod}]...", Brushes.Gold);

        //    await Task.Run(() =>
        //    {
        //        switch (selectedMethod)
        //        {
        //            case "hwid": HWIDActivation.ActivateWindows(); break;
        //            case "Ohook": OhookActivation.ActivateOffice(); break;
        //            case "TSforgeWindows": TSForgeActivation.ActivateWindows(); break;
        //            case "TSforgeOffice": TSForgeActivation.ActivateOffice(); break;
        //            case "idmActivation": IDMActivation.ActivateIDM(); break;
        //        }
        //    });

        //    UpdateText();
        //    ActivateButton.IsEnabled = true;
        //}

        private void SideMenu_SelectionChanged(object sender, FunctionEventArgs<object> e)
        {
            if (MainTabContainer == null || SideNavigationMenu == null) return;

            if (e.Info is SideMenuItem selectedItem)
            {
                int index = SideNavigationMenu.Items.IndexOf(selectedItem);
                if (index >= 0) MainTabContainer.SelectedIndex = index;
            }
        }

        public void UpdateSystemStatusState(string statusMessage, Brush indicatorTone)
        {
            if (StatusBarMessageTxt == null || StatusExecutionDot == null) return;
            StatusBarMessageTxt.Text = statusMessage;
            StatusExecutionDot.Fill = indicatorTone;
        }

        //private void MasterCards_OptionChecked(object sender, RoutedEventArgs e)
        //{
        //    if (e.OriginalSource is Controls.ActivationCard activeCard)
        //    {
        //        var allCards = new[] {
        //            HwidCard,
        //            OhookCard,
        //            TsForgeCard,
        //            IdmCard
        //        };

        //        foreach (var card in allCards)
        //        {
        //            if (card != null && card != activeCard)
        //            {
        //                card.IsOption1Checked = false;
        //                card.IsOption2Checked = false;
        //            }
        //        }
        //    }
        //}
    }
}