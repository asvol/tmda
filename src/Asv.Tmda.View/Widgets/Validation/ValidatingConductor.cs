using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using Caliburn.Micro;

namespace Asv.Avialab.Core
{
  public class ValidatingConductor<T> : Conductor<T>, ISupportValidation where T : class
  {
    private class ConductorValidator : Validator
    {
        readonly IConductor _conductor;
      public ConductorValidator(IConductor conductor)
      {
        if (conductor == null) throw new ArgumentNullException(nameof(conductor));
        _conductor = conductor;
      }

      public override string Validate()
      {
        var results = new List<string>();
        foreach (var i in _conductor.GetChildren())
        {
          var validator = i as ISupportValidation;
          if (validator == null) continue;

          results.Add(validator.Validate());
        }

        results.Add(base.Validate());

        return Strings.Agregate(Environment.NewLine, results.ToArray());
      }

      public override string Error
      {
        get
        {
          var results = new List<string>();
          foreach (var i in _conductor.GetChildren())
          {
            var validator = i as ISupportValidation;
            if (validator == null) continue;

            results.Add(validator.Error);
          }

          results.Add(base.Error);

          return Strings.Agregate(Environment.NewLine, results.ToArray());
        }
      }

      public bool HasError => !string.IsNullOrWhiteSpace(Error);

        public override string this[string columnName]
      {
        get
        {
          var results = new List<string>();
          foreach (var i in _conductor.GetChildren())
          {
            var validator = i as ISupportValidation;
            if (validator == null) continue;

            results.Add(validator[columnName]);
          }

          results.Add(base[columnName]);

          return Strings.Agregate(Environment.NewLine, results.ToArray());
        }
      }
    }

    public new class Collection
    {
      public class OneActive : Conductor<T>.Collection.OneActive, ISupportValidation
      {
        void SubscribeActiveItemEvents(T item)
        {
          if (item == null) throw new ArgumentNullException(nameof(item));

          var np = item as INotifyPropertyChanged;
          if (np != null)
          {
            np.PropertyChanged += np_PropertyChanged;
          }
        }

        void UnsubscribeActiveItemEvents(T item)
        {
          if (item == null) throw new ArgumentNullException(nameof(item));

          var np = item as INotifyPropertyChanged;
          if (np != null)
          {
            np.PropertyChanged -= np_PropertyChanged;
          }
        }

        void np_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
          NotifyOfPropertyChange("ActiveItem." + e.PropertyName);
          NotifyOfPropertyChange(e.PropertyName);
        }

        public override void ActivateItem(T item)
        {
          base.ActivateItem(item);
          SubscribeActiveItemEvents(item);
        }

        public override void DeactivateItem(T item, bool close)
        {
          base.DeactivateItem(item, close);
          if (close)
          {
            UnsubscribeActiveItemEvents(item);
          }
        }

        #region Validator
        private readonly Validator _validator;

        public OneActive()
        {
          _validator = new ConductorValidator(this);
        }

        public string Validate()
        {
          Deferred.Execute(() =>
          {
            NotifyOfPropertyChange(() => Error);
            NotifyOfPropertyChange(() => HasError);
          }, 100);

          return _validator.Validate();
        }

        public string Error => _validator.Error;

          public bool HasError => _validator.HasError;

          public string this[string columnName] => _validator[columnName];

          public ValidationRule AddValidationRule<TPropertyT>(Expression<Func<TPropertyT>> expression)
        {
          return _validator.AddValidationRule<TPropertyT>(expression);
        }

        public void RemoveValidationRule<TPropertyT>(Expression<Func<TPropertyT>> expression)
        {
          _validator.RemoveValidationRule<TPropertyT>(expression);
        }
        #endregion
      }

      public class AllActive : Conductor<T>.Collection.AllActive, ISupportValidation
      {
        void SubscribeActiveItemEvents(T item)
        {
          if (item == null) throw new ArgumentNullException(nameof(item));

          var np = item as INotifyPropertyChanged;
          if (np != null)
          {
            np.PropertyChanged += np_PropertyChanged;
          }
        }

        void UnsubscribeActiveItemEvents(T item)
        {
          if (item == null) throw new ArgumentNullException(nameof(item));

          var np = item as INotifyPropertyChanged;
          if (np != null)
          {
            np.PropertyChanged -= np_PropertyChanged;
          }
        }

        void np_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
          NotifyOfPropertyChange("ActiveItem." + e.PropertyName);
          NotifyOfPropertyChange(e.PropertyName);
        }

        public override void ActivateItem(T item)
        {
          base.ActivateItem(item);
          SubscribeActiveItemEvents(item);
        }

        public override void DeactivateItem(T item, bool close)
        {
          base.DeactivateItem(item, close);
          if (close)
          {
            UnsubscribeActiveItemEvents(item);
          }
        }

        #region Validator
        private readonly Validator _validator;

        public AllActive()
        {
          _validator = new ConductorValidator(this);
        }

        public string Validate()
        {
          Deferred.Execute(() =>
          {
            NotifyOfPropertyChange(() => Error);
            NotifyOfPropertyChange(() => HasError);
          }, 100);

          return _validator.Validate();
        }

        public string Error => _validator.Error;

          public bool HasError => _validator.HasError;

          public string this[string columnName] => _validator[columnName];

          public ValidationRule AddValidationRule<TPropertyT>(Expression<Func<TPropertyT>> expression)
        {
          return _validator.AddValidationRule<TPropertyT>(expression);
        }

        public void RemoveValidationRule<TPropertyT>(Expression<Func<TPropertyT>> expression)
        {
          _validator.RemoveValidationRule<TPropertyT>(expression);
        }
        #endregion
      }
    }

    #region Validator
    private readonly Validator _validator;

    public ValidatingConductor()
    {
      _validator = new Validator();
    }

    public string Validate()
    {
      Deferred.Execute(() =>
      {
        NotifyOfPropertyChange(() => Error);
        NotifyOfPropertyChange(() => HasError);
      }, 100);

      return _validator.Validate();
    }

    public string Error => _validator.Error;

      public bool HasError => _validator.HasError;

      public string this[string columnName] => _validator[columnName];

      public ValidationRule AddValidationRule<TPropertyT>(Expression<Func<TPropertyT>> expression)
    {
      return _validator.AddValidationRule<TPropertyT>(expression);
    }

    public void RemoveValidationRule<TPropertyT>(Expression<Func<TPropertyT>> expression)
    {
      _validator.RemoveValidationRule<TPropertyT>(expression);
    }
    #endregion
  }
}