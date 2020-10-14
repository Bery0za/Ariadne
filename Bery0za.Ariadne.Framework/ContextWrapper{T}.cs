using System;
using System.Collections.Generic;
using System.Linq;

namespace Bery0za.Ariadne.Framework
{
    public delegate void ContextChanged();

    public class ContextWrapper<T> : PropertyWrapper<T>, IContextWrapper<T>
    {
        public event ContextChanged ContextChanged;

        public ContextWrapper() { }

        public ContextWrapper(T context)
            : base(context) { }

        public Func<T, IEnumerable<IContext>> Children { get; set; }

        public override void Set(T value, bool raiseEvent = true)
        {
            if (_value == null || !EqualityComparer<T>.Default.Equals(_value, value))
            {
                T prevValue = _value;
                _value = value;

                if (raiseEvent)
                {
                    RisePropertyChanged(prevValue);
                }

                ContextChanged?.Invoke();

                if (!EqualityComparer<T>.Default.Equals(prevValue, default))
                {
                    foreach (IContext context in DefineChildren(prevValue, Children))
                    {
                        context.DestroyWithChildren();
                    }
                }
            }
        }

        public IEnumerable<IContext> DefineChildren()
        {
            return DefineChildren(Value, Children);
        }

        private static IEnumerable<IContext> DefineChildren(T value, Func<T, IEnumerable<IContext>> children)
        {
            if (!EqualityComparer<T>.Default.Equals(value, default))
            {
                switch (value)
                {
                    case IContext cValue:
                        return new List<IContext> { cValue };

                    case IEnumerable<IContext> cValues:
                        return cValues;
                }

                if (children != null)
                {
                    return children.Invoke(value);
                }
            }

            return Enumerable.Empty<IContext>();
        }

        public void Destroy() { }
    }
}