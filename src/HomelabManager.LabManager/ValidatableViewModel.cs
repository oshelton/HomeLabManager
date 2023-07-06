using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public virtual void OnPropertyMessagesChanged(string propertyName) => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));


        #region INotifyDataErrorInfo

        /// <inheritdoc />
        public bool HasErrors => Validator?.IsValid == false || Validator?.HasWarnings == true;

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
