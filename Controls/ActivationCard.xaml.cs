using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WinActivator.Controls
{
    public partial class ActivationCard : UserControl
    {
        public ActivationCard()
        {
            InitializeComponent();
        }

        // --- Dependency Properties ---

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(ActivationCard));

        public static readonly DependencyProperty CardTitleProperty =
            DependencyProperty.Register("CardTitle", typeof(string), typeof(ActivationCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CardDescriptionProperty =
            DependencyProperty.Register("CardDescription", typeof(string), typeof(ActivationCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty Option1TextProperty =
            DependencyProperty.Register("Option1Text", typeof(string), typeof(ActivationCard), new PropertyMetadata("Option 1"));

        public static readonly DependencyProperty Option2TextProperty =
            DependencyProperty.Register("Option2Text", typeof(string), typeof(ActivationCard), new PropertyMetadata("Option 2"));

        public static readonly DependencyProperty HasTwoOptionsProperty =
            DependencyProperty.Register("HasTwoOptions", typeof(bool), typeof(ActivationCard), new PropertyMetadata(false));

        public static readonly DependencyProperty IsOption1CheckedProperty =
            DependencyProperty.Register("IsOption1Checked", typeof(bool), typeof(ActivationCard), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IsOption2CheckedProperty =
            DependencyProperty.Register("IsOption2Checked", typeof(bool), typeof(ActivationCard), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // --- Property Accessors ---

        public ImageSource Icon { get => (ImageSource)GetValue(IconProperty); set => SetValue(IconProperty, value); }
        public string CardTitle { get => (string)GetValue(CardTitleProperty); set => SetValue(CardTitleProperty, value); }
        public string CardDescription { get => (string)GetValue(CardDescriptionProperty); set => SetValue(CardDescriptionProperty, value); }
        public string Option1Text { get => (string)GetValue(Option1TextProperty); set => SetValue(Option1TextProperty, value); }
        public string Option2Text { get => (string)GetValue(Option2TextProperty); set => SetValue(Option2TextProperty, value); }
        public bool HasTwoOptions { get => (bool)GetValue(HasTwoOptionsProperty); set => SetValue(HasTwoOptionsProperty, value); }
        public bool IsOption1Checked { get => (bool)GetValue(IsOption1CheckedProperty); set => SetValue(IsOption1CheckedProperty, value); }
        public bool IsOption2Checked { get => (bool)GetValue(IsOption2CheckedProperty); set => SetValue(IsOption2CheckedProperty, value); }

        // --- Custom Routed Event ---

        public static readonly RoutedEvent CardClickEvent =
            EventManager.RegisterRoutedEvent("CardClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ActivationCard));

        public event RoutedEventHandler CardClick
        {
            add => AddHandler(CardClickEvent, value);
            remove => RemoveHandler(CardClickEvent, value);
        }

        // --- Event Handler ---

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // If the card only has ONE option total, clicking anywhere on the row should check it.
            if (!HasTwoOptions)
            {
                IsOption1Checked = true;
            }
            // If it has two options, let the user click the specific radio buttons instead
            // so we don't accidentally check the wrong option.

            // Fire the global click notification to MainWindow
            RaiseEvent(new RoutedEventArgs(CardClickEvent));
        }

        public static readonly RoutedEvent OptionCheckedEvent = EventManager.RegisterRoutedEvent("OptionChecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ActivationCard));

        public event RoutedEventHandler OptionChecked
        {
            add => AddHandler(OptionCheckedEvent, value);
            remove => RemoveHandler(OptionCheckedEvent, value);
        }

        //private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (!HasTwoOptions)
        //    {
        //        IsOption1Checked = true;
        //    }
        //}

        //private void RadioButton_Checked(object sender, RoutedEventArgs e)
        //{
        //    // Raise the bubble event up to the main Grid view window context
        //    RaiseEvent(new RoutedEventArgs(Views.DashboardView.OptionCheckedEvent, this));
        //}
    }
}