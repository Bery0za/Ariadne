namespace Bery0za.Ariadne.Framework
{
    public interface IContextWrapper<T> : IContext, IVariableContext, IPropertyWrapper<T>
    {

    }
}