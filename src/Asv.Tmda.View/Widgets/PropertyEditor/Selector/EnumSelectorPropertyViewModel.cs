using System;
using System.Linq;

namespace Asv.Avialab.Core
{
    public class EnumSelectorPropertyViewModel<TValue> : SelectorPropertyViewModel<TValue>
    {
        public EnumSelectorPropertyViewModel(string displayName, Func<TValue,string> displayNameResolver = null, Action<SelectorItem<TValue>> onChanged = null):base(displayName,onChanged)
        {
            if (!typeof(TValue).IsEnum) throw new Exception($"type of {nameof(TValue)} mast be ENUM type");
            foreach (var item in Enum.GetValues(typeof(TValue)).Cast<TValue>())
            {
                var itemName = displayNameResolver == null ? Enum.GetName(typeof(TValue),item): displayNameResolver((TValue) item);
                ActivateItem(new SelectorItem<TValue>(itemName,item));
            }
        }

    }
}