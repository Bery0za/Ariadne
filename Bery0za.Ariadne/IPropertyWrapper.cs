namespace Bery0za.Ariadne
{
    public interface IPropertyWrapper<T>
    {
        event PropertyChanged<T> ValueChanged;
        T Value { get; set; }
    }
}