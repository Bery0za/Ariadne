using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Bery0za.Ariadne
{
    public class BindingSide<T>
    {
        internal bool CanRead => _propertyInfo.CanRead;
        internal bool CanWrite => _propertyInfo.CanWrite;
        internal EventInfo ChangeEvent { get; private set; }
        internal bool HasEvent { get; private set; }
        internal object Target { get; private set; }

        private Expression _expressionBody;
        private Expression<Func<T>> _propertyExpression;
        private PropertyInfo _propertyInfo;

        internal BindingSide(Expression<Func<T>> propertyExpression)
        {
            _propertyExpression = propertyExpression;
            Init();
        }

        internal BindingSide(PropertyWrapper<T> propertyWrapper)
        {
            _propertyExpression = () => propertyWrapper.Value;
            Init();
        }

        internal Delegate Assigner(BindingSide<T> fromSide)
        {
            GuardAssignAvailability(this, fromSide);

            return AssignerFromExpression(_expressionBody, fromSide._expressionBody);
        }

        internal Delegate Assigner<U>(BindingSide<U> fromSide, Func<U, T> adapter)
        {
            GuardAssignAvailability(this, fromSide);

            return AssignerFromExpression(_expressionBody, fromSide._expressionBody, adapter);
        }

        internal Delegate MakeHandler(Action handler)
        {
            Delegate d;

            switch (Target)
            {
                case INotifyPropertyChanged _:
                {
                    PropertyChangedEventHandler h = (s, a) =>
                    {
                        if (a.PropertyName == _propertyInfo.Name)
                        {
                            handler();
                        }
                    };

                    d = h;

                    break;
                }

                case PropertyWrapper<T> _:
                {
                    PropertyChanged<T> h = (v, pv) => { handler(); };
                    d = h;

                    break;
                }

                default:
                {
                    if (ChangeEvent?.Name == _propertyInfo.Name + "Changed")
                    {
                        d = HandlerForAnyEvent(ChangeEvent, handler);
                    }
                    else
                    {
                        throw new BindingException("Cannot determine change event for the property.");
                    }

                    break;
                }
            }

            return d;
        }

        private void Init()
        {
            _expressionBody = _propertyExpression.Body;

            _propertyInfo = PropertyInfoFromExpression(_expressionBody);

            if (_propertyInfo == null)
            {
                throw new BindingException("Incorrect property expression was used.");
            }

            Target = TargetFromExpression(_expressionBody);

            if (Target == null)
            {
                throw new BindingException("Cannot determine target object property belongs to.");
            }

            ChangeEvent = ChangeEventFromObject(Target, _propertyInfo);
            HasEvent = ChangeEvent != null;
        }

        private static void GuardAssignAvailability<U>(BindingSide<T> sideTo, BindingSide<U> sideFrom)
        {
            if (!sideTo.CanWrite)
            {
                throw new BindingException("Target property can not be written.");
            }

            if (!sideFrom.CanRead)
            {
                throw new BindingException("Source property can not be read.");
            }
        }

        private static PropertyInfo PropertyInfoFromExpression(Expression expressionBody)
        {
            PropertyInfo pi = null;

            if (expressionBody is MemberExpression me)
            {
                pi = me.Member as PropertyInfo;
            }

            return pi;
        }

        private static object TargetFromExpression(Expression expressionBody)
        {
            Expression expr = expressionBody;
            Stack<MemberInfo> memberInfos = new Stack<MemberInfo>();

            while (expr is MemberExpression)
            {
                MemberExpression memberExpr = expr as MemberExpression;
                memberInfos.Push(memberExpr.Member);
                expr = memberExpr.Expression;
            }

            if (!(expr is ConstantExpression constExpr))
            {
                return null;
            }

            object objReference = constExpr.Value;

            while (memberInfos.Count > 1)
            {
                MemberInfo mi = memberInfos.Pop();

                switch (mi.MemberType)
                {
                    case MemberTypes.Property:
                        objReference = objReference?.GetType()
                                                   .GetProperty(mi.Name,
                                                                BindingFlags.Instance
                                                                | BindingFlags.Public
                                                                | BindingFlags.NonPublic)
                                                   ?.GetValue(objReference, null);

                        break;

                    case MemberTypes.Field:
                        objReference = objReference?.GetType()
                                                   .GetField(mi.Name,
                                                             BindingFlags.Instance
                                                             | BindingFlags.Public
                                                             | BindingFlags.NonPublic)
                                                   ?.GetValue(objReference);

                        break;
                }
            }

            return objReference;
        }

        private static EventInfo ChangeEventFromObject(object target, PropertyInfo propInfo)
        {
            EventInfo ei;

            switch (target)
            {
                case INotifyPropertyChanged t:
                    ei = t.GetType().GetEvent("PropertyChanged");

                    break;

                case PropertyWrapper<T> t:
                    ei = t.GetType().GetEvent("ValueChanged");

                    break;

                default:
                    ei = target.GetType().GetEvent(propInfo.Name + "Changed");

                    break;
            }

            return ei;
        }

        private static Delegate AssignerFromExpression(Expression to, Expression from)
        {
            BinaryExpression assign = Expression.Assign(to, from);
            LambdaExpression lambda = Expression.Lambda(assign);
            Delegate func = lambda.Compile();

            return func;
        }

        private static Delegate AssignerFromExpression<U>(Expression to, Expression from, Func<U, T> adapter)
        {
            ConstantExpression instance = adapter.Target == null ? null : Expression.Constant(adapter.Target);
            BinaryExpression assign = Expression.Assign(to, Expression.Call(instance, adapter.Method, from));
            LambdaExpression lambda = Expression.Lambda(assign);
            Delegate func = lambda.Compile();

            return func;
        }

        private static Delegate HandlerForAnyEvent(EventInfo eventInfo, Action action)
        {
            Type delegateType = eventInfo.EventHandlerType;
            MethodInfo invokeMethod = delegateType.GetMethod("Invoke");

            ParameterExpression[] @params = invokeMethod
                                            .GetParameters()
                                            .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                                            .ToArray();

            ConstantExpression instance = action.Target == null ? null : Expression.Constant(action.Target);
            MethodCallExpression call = Expression.Call(instance, action.Method);

            Expression body = invokeMethod.ReturnType == typeof(void)
                ? (Expression)call
                : Expression.Convert(call, invokeMethod.ReturnType);

            LambdaExpression expr = Expression.Lambda(delegateType, body, @params);

            return expr.Compile();
        }
    }
}