using System;
using System.Linq.Expressions;
using Caliburn.Micro;

namespace Asv.Avialab.Core
{
    public class ValidatingScreen : Screen, ISupportValidation
    {
        private readonly Validator _validator;

        public ValidatingScreen()
        {
            _validator = new Validator();
        }

        void NotifyErrorChanged()
        {
            Deferred.Execute(() =>
            {
                NotifyOfPropertyChange(() => Error);
                NotifyOfPropertyChange(() => HasError);
            }, 100);
        }

        public string Validate()
        {
            NotifyErrorChanged();

            return _validator.Validate();
        }

        public string Error => _validator.Error;

        public bool HasError => _validator.HasError;

        public string this[string columnName]
        {
            get
            {
                NotifyErrorChanged();

                return _validator[columnName];
            }
        }

        public ValidationRule AddValidationRule<TProperty>(Expression<Func<TProperty>> expression)
        {
            return _validator.AddValidationRule(expression);
        }

        public void RemoveValidationRule<TProperty>(Expression<Func<TProperty>> expression)
        {
            _validator.RemoveValidationRule(expression);
        }
    }
}
