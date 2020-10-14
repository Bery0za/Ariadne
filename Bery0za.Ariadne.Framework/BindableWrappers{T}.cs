using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Bery0za.Ariadne;

namespace Bery0za.Ariadne.Framework
{
    public delegate void OnContextAttaching<T>(T context, IBinder<IContextWrapper<T>> binder);

    public delegate void OnContextDestroying();

    public class BindableWrapper<T> : PropertyWrapper<T>, IBindable<IContextWrapper<T>>
    {
        public event OnContextAttaching<T> Attaching;
        public event OnContextDestroying Destroying;

        public void OnContextAttach(IContextWrapper<T> context,
                                    IList<IBinding> bindings,
                                    IBinder<IContextWrapper<T>> binder)
        {
            bindings.Add(Binder.Side(() => Value).To(Binder.Side(() => context.Value)));
            Attaching?.Invoke(context.Value, binder);
        }

        public void OnContextDestroy()
        {
            Destroying?.Invoke();
        }
    }
}