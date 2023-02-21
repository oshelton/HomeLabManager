using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace HomeLabManager.Manager.SharedDialogs
{
    public sealed class SimpleSavingDataDialogViewModel : ReactiveObject
    {
        public string? SaveMessage
        {
            get => _saveMessage;
            set => this.RaiseAndSetIfChanged(ref _saveMessage, value);
        }

        private string? _saveMessage;
    }
}
