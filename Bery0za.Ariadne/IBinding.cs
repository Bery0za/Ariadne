namespace Bery0za.Ariadne
{
    public interface IBinding
    {
        void Bind();
        void Invalidate(BindingSide side);
        void Subscribe();
        void Subscribe(BindingSide side);
        void Unsubscribe();
        void Unsubscribe(BindingSide side);
        void Unbind();
    }
}