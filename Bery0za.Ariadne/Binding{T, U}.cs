using System;

namespace Bery0za.Ariadne
{
    public class Binding<T, U> : IBinding
    {
        internal BindingSide<T> _sideA;
        internal BindingSide<U> _sideB;

        internal BindingFlow _flow;

        internal Delegate _assignAB;
        internal Delegate _assignBA;

        internal Delegate _handlerA;
        internal Delegate _handlerB;

        internal Binding()
        {

        }

        public void Bind()
        {
            Binder.Bind(this);
        }

        public void Invalidate(BindingSide<T> side)
        {
            if (_sideA ==  side)
            {
                Invalidate(BindingSide.A);
            }
        }

        public void Invalidate(BindingSide<U> side)
        {
            if (_sideB == side)
            {
                Invalidate(BindingSide.B);
            }
        }

        public void Invalidate(BindingSide side)
        {
            switch (_flow)
            {
                case BindingFlow.TwoWay:
                    if (side == BindingSide.A)
                    {
                        Binder.RunAssign(_assignAB);
                    }
                    else
                    {
                        Binder.RunAssign(_assignBA);
                    }
                    break;
                case BindingFlow.OneWay:
                    if (side == BindingSide.B)
                    {
                        Binder.RunAssign(_assignBA);
                    }
                    break;
                case BindingFlow.Reverse:
                    if (side == BindingSide.A)
                    {
                        Binder.RunAssign(_assignAB);
                    }

                    Binder.RunAssign(_assignBA);
                    break;
            }
        }

        public void Subscribe()
        {
            Subscribe(BindingSide.A);
            Subscribe(BindingSide.B);
        }

        public void Subscribe(BindingSide side)
        {
            if (side == BindingSide.A && _handlerA != null) _sideA.ChangeEvent.AddEventHandler(_sideA.Target, _handlerA);
            else if (side == BindingSide.B && _handlerB != null) _sideB.ChangeEvent.AddEventHandler(_sideB.Target, _handlerB);
        }

        public void Unsubscribe()
        {
            Unsubscribe(BindingSide.A);
            Unsubscribe(BindingSide.B);
        }

        public void Unsubscribe(BindingSide side)
        {
            if (side == BindingSide.A && _handlerA != null) _sideA.ChangeEvent.RemoveEventHandler(_sideA.Target, _handlerA);
            else if (side == BindingSide.B && _handlerB != null) _sideB.ChangeEvent.RemoveEventHandler(_sideB.Target, _handlerB);
        }

        public void Unbind()
        {
            Unsubscribe();
            _handlerA = null;
            _handlerB = null;
        }
    }
}