using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Bery0za.Ariadne
{
    public enum BindingFlow
    {
        OneWay,
        TwoWay,
        Reverse,
        Once
    }

    public enum BindingSide
    {
        A,
        B
    }

    public static class Binder
    {
        public static BindingSide<T> Side<T>(Expression<Func<T>> propertyExpression)
        {
            return new BindingSide<T>(propertyExpression);
        }

        public static BindingSide<T> Side<T>(PropertyWrapper<T> propertyWrapper)
        {
            return new BindingSide<T>(propertyWrapper);
        }

        public static (BindingSide<T> SideA, BindingSide<U> SideB) To<T, U>(this BindingSide<T> sideA,
                                                                            Expression<Func<U>> propertyExpression)
        {
            return (sideA, new BindingSide<U>(propertyExpression));
        }

        public static (BindingSide<T> SideA, BindingSide<U> SideB) To<T, U>(this BindingSide<T> sideA,
                                                                            PropertyWrapper<U> propertyWrapper)
        {
            return (sideA, new BindingSide<U>(propertyWrapper));
        }

        public static Binding<T, T> Using<T>(this (BindingSide<T> SideA, BindingSide<T> SideB) unconfiguredPair,
                                             BindingFlow flow = BindingFlow.TwoWay)
        {
            return unconfiguredPair.SideA.To(unconfiguredPair.SideB, flow);
        }

        public static Binding<T, U> Using<T, U>(this (BindingSide<T> SideA, BindingSide<U> SideB) unconfiguredPair,
                                                Func<U, T> adapterBA,
                                                Func<T, U> adapterAB = null,
                                                BindingFlow flow = BindingFlow.TwoWay)
        {
            return unconfiguredPair.SideA.To(unconfiguredPair.SideB, adapterBA, adapterAB, flow);
        }

        public static Binding<T, T> To<T>(this BindingSide<T> sideA,
                                          BindingSide<T> sideB,
                                          BindingFlow flow = BindingFlow.TwoWay)
        {
            Binding<T, T> b = new Binding<T, T>
            {
                _sideA = sideA,
                _sideB = sideB,
                _flow = flow,
                _assignBA = sideA.Assigner(sideB)
            };

            if (flow == BindingFlow.TwoWay || flow == BindingFlow.Reverse)
            {
                b._assignAB = sideB.Assigner(sideA);
            }

            return Bind(b);
        }

        public static Binding<T, U> To<T, U>(this BindingSide<T> sideA,
                                             BindingSide<U> sideB,
                                             Func<U, T> adapterBA,
                                             Func<T, U> adapterAB = null,
                                             BindingFlow flow = BindingFlow.TwoWay)
        {
            Binding<T, U> b = new Binding<T, U>
            {
                _sideA = sideA,
                _sideB = sideB,
                _flow = flow,
                _assignBA = sideA.Assigner(sideB, adapterBA)
            };

            if (flow == BindingFlow.TwoWay || flow == BindingFlow.Reverse)
            {
                b._assignAB = adapterAB != null
                    ? sideB.Assigner(sideA, adapterAB)
                    : throw new BindingException("Adapter from A to B must be defined for the chosen binding flow.");

                ;
            }

            return Bind(b);
        }

        internal static Binding<T, U> Bind<T, U>(Binding<T, U> binding)
        {
            try
            {
                switch (binding._flow)
                {
                    case BindingFlow.OneWay:
                        binding._handlerB = binding._sideB.MakeHandler(OneWay(binding));
                        break;

                    case BindingFlow.TwoWay:
                        binding._handlerA =
                            binding._sideA.MakeHandler(TwoWay(binding, BindingSide.B, binding._assignAB));

                        binding._handlerB =
                            binding._sideB.MakeHandler(TwoWay(binding, BindingSide.A, binding._assignBA));

                        break;

                    case BindingFlow.Reverse:
                        binding._handlerA =
                            binding._sideA.MakeHandler(TwoWay(binding, BindingSide.A, binding._assignAB));

                        binding._handlerB =
                            binding._sideB.MakeHandler(TwoWay(binding, BindingSide.A, binding._assignBA));

                        break;

                    case BindingFlow.Once:
                        binding._handlerB = binding._sideB.MakeHandler(Once(binding));
                        break;
                }

                return binding;
            }
            catch (Exception e)
            {
                throw new BindingException("There was an error during bind establishing.", e);
            }
        }

        internal static void RunAssign(Delegate assigner)
        {
            try
            {
                assigner.DynamicInvoke();
            }
            catch (TargetInvocationException e)
            {
                throw new BindingException("There was an error during value assignment.", e.InnerException);
            }
        }

        private static Action OneWay<T, U>(Binding<T, U> binding)
        {
            return () => { RunAssign(binding._assignBA); };
        }

        private static Action TwoWay<T, U>(Binding<T, U> binding, BindingSide side, Delegate assign)
        {
            return () =>
            {
                binding.Unsubscribe(side);
                RunAssign(assign);
                binding.Subscribe(side);
            };
        }

        private static Action Once<T, U>(Binding<T, U> binding)
        {
            return () =>
            {
                binding.Unsubscribe(BindingSide.B);
                RunAssign(binding._assignBA);
            };
        }
    }
}