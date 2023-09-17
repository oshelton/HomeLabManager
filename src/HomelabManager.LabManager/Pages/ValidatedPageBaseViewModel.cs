using System.Collections;
using System.ComponentModel;
using ReactiveUI;
using ReactiveValidation;

namespace HomeLabManager.Manager.Pages
{
    /// <summary>
    /// Base class for primary navigation pages that are validatable.
    /// </summary>
    public abstract class ValidatedPageBaseViewModel<T> : PageBaseViewModel<T>, IValidatableObject where T : class
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
            this.RaisePropertyChanged(nameof(HasErrors));
        }


        #region INotifyDataErrorInfo

        /// <inheritdoc />
        public bool HasErrors => Validator?.IsValid == false;

        /// <inheritdoc />
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <inheritdoc />
        public IEnumerable GetErrors(string propertyName)
        {
            if (Validator == null)
                return Array.Empty<ValidationMessage>();

            return string.IsNullOrEmpty(propertyName)
                ? Validator.ValidationMessages
                : Validator.GetMessages(propertyName!);
        }

        #endregion

        private IObjectValidator _objectValidator;
    }
}
