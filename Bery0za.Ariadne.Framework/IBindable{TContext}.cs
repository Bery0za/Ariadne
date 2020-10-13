using System.Collections.Generic;

namespace Bery0za.Ariadne.Framework
{
    public interface IBindable<in TContext>
        where TContext : IContext
    {
        void OnContextAttach(TContext context, IList<IBinding> bindings, IBinder<TContext> binder);
        void OnContextDestroy();
    }
}