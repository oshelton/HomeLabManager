using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Material.Icons;

namespace HomeLabManager.Manager.Controls
{
    /// <summary>
    /// Reusable Page Nav Button control.
    /// </summary>
    [TemplatePart(PartButtonName, typeof(Button))]
    public partial class PageNavButton : TemplatedControl
    {
        public const string PartButtonName = "PART_Button";

        /// <summary>
        /// Defines the <see cref="Icon"/> property.
        /// </summary>
        public static readonly StyledProperty<MaterialIconKind> IconProperty =
            AvaloniaProperty.Register<PageNavButton, MaterialIconKind>(nameof(Icon), MaterialIconKind.SimpleIcons);

        /// <summary>
        /// Defines the <see cref="Text"/> property.
        /// </summary>
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<PageNavButton, string>(nameof(Text), "Nav Button");

        /// <summary>
        /// Defines the <see cref="IsOutline"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsOutlineProperty =
            AvaloniaProperty.Register<PageNavButton, bool>(nameof(IsOutline), false);

        /// <summary>
        /// Defines the <see cref="Command"/> property.
        /// </summary>
        public static readonly DirectProperty<PageNavButton, ICommand> CommandProperty =
            AvaloniaProperty.RegisterDirect<PageNavButton, ICommand>(
                nameof(Command),
                o => o.Command,
                (o, v) => o.Command = v);

        /// <summary>
        /// Defines the <see cref="CommandParameter"/> property.
        /// </summary>
        public static readonly DirectProperty<PageNavButton, object> CommandParameterProperty =
            AvaloniaProperty.RegisterDirect<PageNavButton, object>(
                nameof(CommandParameter),
                o => o.CommandParameter,
                (o, v) => o.CommandParameter = v);

        /// <summary>
        /// Defines the <see cref="Click"/> event.
        /// </summary>
        public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
            RoutedEvent.Register<PageNavButton, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

        public PageNavButton() => InitializeComponent();

        /// <summary>
        /// Gets or sets a value for the Button's Icon.
        /// </summary>
        public MaterialIconKind Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>
        /// Gets or sets a value for the Button's Text.
        /// </summary>
        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// Gets or sets a value for if the button should be an outline button or not.
        /// </summary>
        public bool IsOutline
        {
            get => GetValue(IsOutlineProperty);
            set => SetValue(IsOutlineProperty, value);
        }

        /// <summary>
        /// Gets or sets a value for the command associated with the button.
        /// </summary>
        public ICommand Command
        {
            get => _command;
            set
            {
                if (SetAndRaise(CommandProperty, ref _command, value) && _button is not null)
                    _button.Command = value;
            }
        }

        /// <summary>
        /// Gets or sets a value for the command parameter associated with the button.
        /// </summary>
        public object CommandParameter
        {
            get => _commandParameter;
            set
            {
                if (SetAndRaise(CommandParameterProperty, ref _commandParameter, value) && _button is not null)
                    _button.CommandParameter = value;
            }
        }

        /// <summary>
        /// Raised when the user clicks the button.
        /// </summary>
        public event EventHandler<RoutedEventArgs> Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        internal void OnButtonClick(object sender, RoutedEventArgs args)
        {
            var eventArgs = new RoutedEventArgs { RoutedEvent = ClickEvent };
            RaiseEvent(eventArgs);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            if (e is null)
                throw new ArgumentNullException(nameof(e));

            base.OnApplyTemplate(e);

            _button = e.NameScope.Get<Button>(PartButtonName);

            _button.Command = Command;
            _button.CommandParameter = CommandParameter;

            if (_button is null)
                throw new InvalidOperationException($"PageNavButton templates must define a {PartButtonName} Button element.");
        }

        private ICommand _command;
        private object _commandParameter;

        private Button _button;
    }
}
