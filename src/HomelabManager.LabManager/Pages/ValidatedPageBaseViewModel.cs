﻿using System.Collections;
using System.ComponentModel;
using ReactiveValidation;

namespace HomeLabManager.Manager.Pages
{
    /// <summary>
    /// Base class for primary navigation pages that are validatable.
    /// </summary>
    public abstract class ValidatedPageBaseViewModel: PageBaseViewModel, IValidatableObject
    {
        /// <inheritdoc />
        public IObjectValidator? Validator
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
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        /// <inheritdoc />
        public IEnumerable GetErrors(string? propertyName)
        {
            if (Validator == null)
                return Array.Empty<ValidationMessage>();

            return string.IsNullOrEmpty(propertyName)
                ? Validator.ValidationMessages
                : Validator.GetMessages(propertyName!);
        }

        #endregion

        private IObjectValidator? _objectValidator;
    }
}