using System.Collections;
using System.ComponentModel;
using ReactiveUI;
using ReactiveValidation;

namespace HomeLabManager.Manager
{
    public class ValidatableViewModel: ReactiveObject, IValidatableObject
    {
        /// <inheritdoc />
        public IObjectValidator Validator
        {
            get => _objectValidator;
            set
            {
                if (value != _objectValidator)
                {
                    _objectValidator?.Dispose();
                    _objectValidator = value;
                    _objectValidator?.Revalidate();
                }
            }
        }

        /// <inheritdoc />
        public virtual void OnPropertyMessagesChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            HasErrors = !_objectValidator?.IsValid ?? false;
        }


        #region INotifyDataErrorInfo

        /// <inheritdoc />
        public bool HasErrors
        {
            get => _hasErrors;
            private set => this.RaiseAndSetIfChanged(ref _hasErrors, value);
        }

        /// <inheritdoc />
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <inheritdoc />
        public IEnumerable GetErrors(string propertyName)
        {
            if (Validator is null)
                return Array.Empty<ValidationMessage>();

            return string.IsNullOrEmpty(propertyName)
                ? Validator.ValidationMessages
                : Validator.GetMessages(propertyName!);
        }

        #endregion

        private IObjectValidator _objectValidator;
        private bool _hasErrors;
    }
}
