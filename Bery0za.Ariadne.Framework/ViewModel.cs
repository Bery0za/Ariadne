using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Bery0za.Ariadne.Framework
{
    public abstract class ViewModel : INotifyPropertyChanged, IContext
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected List<IBinding> _bindings = new List<IBinding>();
        
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void InitBindings()
        {
            _bindings.ForEach(b => b.Bind());
            _bindings.ForEach(b => b.Invalidate(BindingSide.B));
            _bindings.ForEach(b => b.Subscribe());
        }

        public virtual IEnumerable<IContext> DefineChildren()
        {
            return Enumerable.Empty<IContext>();
        }

        public virtual void Destroy()
        {
            _bindings.ForEach(b => b.Unbind());
        }
    }
}
