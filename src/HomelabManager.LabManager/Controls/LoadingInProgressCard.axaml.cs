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
    /// Reusable Loading In Progress Card.
    /// </summary>
    public partial class LoadingInProgressCard : TemplatedControl
    {

        /// <summary>
        /// Defines the <see cref="Label"/> property.
        /// </summary>
        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<LoadingInProgressCard, string>(nameof(Label), "Loading...");

        public LoadingInProgressCard() => InitializeComponent();

        /// <summary>
        /// Gets or sets a value for the Button's Text.
        /// </summary>
        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }
    }
}
