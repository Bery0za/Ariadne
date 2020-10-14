using System.Collections.Generic;
using System.Linq;

namespace Bery0za.Ariadne.Framework
{
    public class Binder<TContext> : IBinder<TContext>
        where TContext : IContext
    {
        public TContext Context { get; private set; }
        IContext IBinder.Context => Context;
        public IBinder Parent { get; private set; }
        public IEnumerable<IBinder> Children => _children;

        private readonly List<IBinder> _children = new List<IBinder>();

        private readonly Dictionary<IBindable<TContext>, List<IBinding>> _bindings =
            new Dictionary<IBindable<TContext>, List<IBinding>>();

        public Binder(TContext context, IBinder parent = null)
        {
            Context = context;
            Parent = parent;

            if (Context is IVariableContext vContext)
            {
                vContext.ContextChanged += OnContextChanged;
            }
        }

        public void Attach(IBindable<TContext> bindable)
        {
            if (!_bindings.ContainsKey(bindable))
            {
                List<IBinding> bindings = new List<IBinding>();
                bindable.OnContextAttach(Context, bindings, this);
                _bindings.Add(bindable, bindings);
            }
        }

        public void AttachChild<UContext>(UContext context, IBindable<UContext> bindable)
            where UContext : IContext
        {
            Binder<UContext> binder = new Binder<UContext>(context, this);
            binder.Attach(bindable);
            _children.Add(binder);
        }

        public void AddChild<UContext>(UContext context)
            where UContext : IContext
        {
            Binder<UContext> binder = new Binder<UContext>(context, this);
            _children.Add(binder);
        }

        private void OnContextChanged()
        {
            List<IBindable<TContext>> bindables = _bindings.Keys.ToList();

            DestroyBindings();
            DestroyChildren();

            bindables.ForEach(Attach);
            Run();
        }

        public bool IsAttached(IBindable<TContext> bindable)
        {
            return _bindings.ContainsKey(bindable);
        }

        public void Invalidate(bool affectChilds = true)
        {
            foreach (IBindable<TContext> key in _bindings.Keys)
            {
                Invalidate(key);
            }

            if (affectChilds)
            {
                _children.ForEach(c => c.Invalidate(true));
            }
        }

        public void Invalidate(IBindable<TContext> consumer)
        {
            if (_bindings.TryGetValue(consumer, out List<IBinding> bindings))
            {
                bindings.ForEach(b => b.Invalidate(BindingSide.B));
            }
        }

        public void Bind(bool affectChilds = true)
        {
            foreach (IBindable<TContext> key in _bindings.Keys)
            {
                Bind(key);
            }

            if (affectChilds)
            {
                _children.ForEach(b => b.Bind(true));
            }
        }

        public void Bind(IBindable<TContext> consumer)
        {
            if (_bindings.TryGetValue(consumer, out List<IBinding> bindings))
            {
                bindings.ForEach(b => b.Bind());
            }
        }

        public void Unbind(bool affectChilds = true)
        {
            foreach (IBindable<TContext> key in _bindings.Keys)
            {
                Unbind(key);
            }

            if (affectChilds)
            {
                _children.ForEach(b => b.Unbind(true));
            }
        }

        public void Unbind(IBindable<TContext> consumer)
        {
            if (_bindings.TryGetValue(consumer, out List<IBinding> bindings))
            {
                bindings.ForEach(b => b.Unbind());
            }
        }

        public void Subscribe(bool affectChilds = true)
        {
            foreach (IBindable<TContext> key in _bindings.Keys)
            {
                Subscribe(key);
            }

            if (affectChilds)
            {
                _children.ForEach(b => b.Subscribe(true));
            }
        }

        public void Subscribe(IBindable<TContext> consumer)
        {
            if (_bindings.TryGetValue(consumer, out List<IBinding> bindings))
            {
                bindings.ForEach(b => b.Subscribe());
            }
        }

        public void Unsubscribe(bool affectChilds = true)
        {
            foreach (IBindable<TContext> key in _bindings.Keys)
            {
                Unsubscribe(key);
            }

            if (affectChilds)
            {
                _children.ForEach(b => b.Unsubscribe(true));
            }
        }

        public void Unsubscribe(IBindable<TContext> consumer)
        {
            if (_bindings.TryGetValue(consumer, out List<IBinding> bindings))
            {
                bindings.ForEach(b => b.Unsubscribe());
            }
        }

        public void Run(bool affectChilds = true)
        {
            Bind(affectChilds);
            Invalidate(affectChilds);
            Subscribe(affectChilds);
        }

        public void Destroy()
        {
            if (Context is IVariableContext vContext)
            {
                vContext.ContextChanged -= OnContextChanged;
            }

            DestroyBindings();
            DestroyChildren();
        }

        public void DestroyBindings()
        {
            foreach (IBindable<TContext> key in _bindings.Keys)
            {
                Destroy(key);
            }

            _bindings.Clear();
        }

        public void DestroyChildren()
        {
            _children.ForEach(b => b.Destroy());
            _children.Clear();
        }

        public void Destroy(IBindable<TContext> consumer)
        {
            if (_bindings.TryGetValue(consumer, out List<IBinding> bindings))
            {
                bindings.ForEach(b => b.Unbind());
                consumer.OnContextDestroy();
            }
        }
    }
}