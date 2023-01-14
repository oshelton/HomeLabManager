using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Material.Icons;

namespace HomeLabManager.Manager.Controls
{
    /// <summary>
    /// Reusable Page Nav Button control.
    /// </summary>
    public partial class PageNavButton : UserControl
    {
        /// <summary>
        /// Defines the <see cref="Icon"/> property.
        /// </summary>
        public static readonly StyledProperty<MaterialIconKind> IconProperty =
            AvaloniaProperty.Register<PageNavButton, MaterialIconKind>(nameof(Icon), MaterialIconKind.SimpleIcons);

        /// <summary>
        /// Gets or sets a value for the Button's Icon.
        /// </summary>
        public MaterialIconKind Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Text"/> property.
        /// </summary>
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<PageNavButton, string>(nameof(Text), "Nav Button");

        /// <summary>
        /// Gets or sets a value for the Button's Text.
        /// </summary>
        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Click"/> event.
        /// </summary>
        public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
            RoutedEvent.Register<Button, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

        /// <summary>
        /// Raised when the user clicks the button.
        /// </summary>
        public event EventHandler<RoutedEventArgs>? Click
        {
            add => button.Click += value;
            remove => button.Click -= value;
        }

        public PageNavButton()
        {
            InitializeComponent();

            button.Classes = Classes;
        }
    }
}
