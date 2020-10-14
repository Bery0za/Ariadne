using System.Collections.Generic;

namespace Bery0za.Ariadne.Framework
{
    public interface IBinder
    {
        IContext Context { get; }
        IBinder Parent { get; }
        IEnumerable<IBinder> Children { get; }

        void AttachChild<UContext>(UContext context, IBindable<UContext> bindable)
            where UContext : IContext;

        void Invalidate(bool affectChilds);
        void Bind(bool affectChilds);
        void Unbind(bool affectChilds);
        void Subscribe(bool affectChilds);
        void Unsubscribe(bool affectChilds);
        void Run(bool affectChilds);
        void Destroy();
        void DestroyBindings();
        void DestroyChildren();
    }
}