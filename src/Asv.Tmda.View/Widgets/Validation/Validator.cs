using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Asv.Avialab.Core
{
  public class Validator
  {
    readonly Dictionary<string, List<ValidationRule>> _validationRules = new Dictionary<string, List<ValidationRule>>();
    readonly Dictionary<string, string> _errors = new Dictionary<string, string>();

    public ValidationRule AddValidationRule<TPropertyT>(Expression<Func<TPropertyT>> expression)
    {
      var propertyName = expression.GetPropertyFullName();
      if (!_validationRules.ContainsKey(propertyName))
      {
        _validationRules.Add(propertyName, new List<ValidationRule>());

      }
      var rule = new ValidationRule();
      _validationRules[propertyName].Add(rule);

      return rule;
    }
    public void RemoveValidationRule<TPropertyT>(Expression<Func<TPropertyT>> expression)
    {
      var propertyName = expression.GetPropertyFullName();
      if (_validationRules.ContainsKey(propertyName))
        _validationRules.Remove(propertyName);
    }

    public virtual string Error
    {
      get 
      {
        Validate();
        return Strings.Agregate(Environment.NewLine, _errors.Select(x => x.Value).Distinct().ToArray());
      }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(Error);

      public virtual string this[string columnName] => Validate(columnName);

      public virtual string Validate()
    {
      _errors.Clear();
      return Validate(GetType().GetProperties().Select(x => x.Name).Union(_validationRules.Keys));
    }

    string Validate(string propertyName)
    {
      return Validate(new List<string> { propertyName });
    }

    string Validate(IEnumerable<string> propertyNames)
    {
      List<string> results = new List<string>();
      foreach (var propertyName in propertyNames)
      {
        if (!_validationRules.ContainsKey(propertyName)) continue;

        if (_errors.ContainsKey(propertyName))
        {
          _errors.Remove(propertyName);
        }

        foreach (var validationRule in _validationRules[propertyName])
        {
          var result = validationRule.Validate();
          if (!string.IsNullOrEmpty(result))
          {
            results.Add(result);

            if (_errors.ContainsKey(propertyName))
            {
              _errors[propertyName] = result;
            }
            else
            {
              _errors.Add(propertyName, result);
            }
          }
        }
      }

      return Strings.Agregate(Environment.NewLine, results.Distinct().ToArray());
    }
  }

  public class ValidationRule
  {
    Expression<Func<bool>> _condition;
    string _message;

    public ValidationRule Condition(Expression<Func<bool>> condition)
    {
      _condition = condition;
      return this;
    }

    public ValidationRule Message(string message)
    {
      _message = message;
      return this;
    }

    public string Validate()
    {
      if (_condition == null)
        return string.Empty;
      else
        return _condition.Compile()() ? _message : string.Empty;
    }
  }

  public interface ISupportValidation : IDataErrorInfo
  {
    ValidationRule AddValidationRule<TPropertyT>(Expression<Func<TPropertyT>> expression);
    void RemoveValidationRule<TPropertyT>(Expression<Func<TPropertyT>> expression);
    string Validate();
  }
}
