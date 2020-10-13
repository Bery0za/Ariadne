using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Bery0za.Ariadne
{
    public delegate void PropertyChanged<T>(T value, T previousValue);

    public class PropertyWrapper<T> : IPropertyWrapper<T>
    {
        public event PropertyChanged<T> ValueChanged;

        protected T _value;

        public T Value
        {
            get => _value;
            set => Set(value);
        }

        public PropertyWrapper()
        {
            
        }

        public PropertyWrapper(T initialValue)
        {
            _value = initialValue;
        }

        public virtual void Set(T value, bool raiseEvent = true)
        {
            if (_value == null || !EqualityComparer<T>.Default.Equals(_value, value))
            {
                T prevValue = _value;
                _value = value;

                if (raiseEvent)
                {
                    RisePropertyChanged(prevValue);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RisePropertyChanged()
        {
            ValueChanged?.Invoke(_value, _value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RisePropertyChanged(T oldValue)
        {
            ValueChanged?.Invoke(_value, oldValue);
        }

        public static explicit operator T(PropertyWrapper<T> wrapper)
        {
            return wrapper._value;
        }
    }
}
