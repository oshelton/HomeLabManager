using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Material.Icons;

namespace HomeLabManager.Manager.Controls
{
    /// <summary>
    /// Reusable Page Nav Button control.
    /// </summary>
    [TemplatePart(PartButtonName, typeof(Button))]
    public partial class AddItemInlineSeperator : TemplatedControl
    {
        /// <summary>
        /// Name of the main Button Template part.
        /// </summary>
        public const string PartButtonName = "PART_Button";

        /// <summary>
        /// Defines the <see cref="Icon"/> property.
        /// </summary>
        public static readonly StyledProperty<MaterialIconKind> IconProperty =
            AvaloniaProperty.Register<AddItemInlineSeperator, MaterialIconKind>(nameof(Icon), MaterialIconKind.CirclesAdd);

        /// <summary>
        /// Defines the <see cref="Text"/> property.
        /// </summary>
        public static readonly StyledProperty<string> TextProperty =
            AvaloniaProperty.Register<AddItemInlineSeperator, string>(nameof(Text), "");

        /// <summary>
        /// Defines the <see cref="Command"/> property.
        /// </summary>
        public static readonly DirectProperty<AddItemInlineSeperator, ICommand> CommandProperty =
            AvaloniaProperty.RegisterDirect<AddItemInlineSeperator, ICommand>(
                nameof(Command),
                o => o.Command,
                (o, v) => o.Command = v);

        /// <summary>
        /// Defines the <see cref="CommandParameter"/> property.
        /// </summary>
        public static readonly DirectProperty<AddItemInlineSeperator, object> CommandParameterProperty =
            AvaloniaProperty.RegisterDirect<AddItemInlineSeperator, object>(
                nameof(CommandParameter),
                o => o.CommandParameter,
                (o, v) => o.CommandParameter = v);

        /// <summary>
        /// Defines the <see cref="Click"/> event.
        /// </summary>
        public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
            RoutedEvent.Register<AddItemInlineSeperator, RoutedEventArgs>(nameof(Click), RoutingStrategies.Bubble);

        public AddItemInlineSeperator() => InitializeComponent();

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

            _button = e.NameScope.Get<Button>(PartButtonName) ?? throw new InvalidOperationException($"PageNavButton templates must define a {PartButtonName} Button element.");

            _button.Command = Command;
            _button.CommandParameter = CommandParameter;
        }

        private ICommand _command;
        private object _commandParameter;

        private Button _button;
    }
}
