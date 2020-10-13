namespace Bery0za.Ariadne.Framework
{
    public interface IBinder<out TContext> : IBinder
        where TContext : IContext
    {
        new TContext Context { get; }
        void Attach(IBindable<TContext> bindable);
    }
}