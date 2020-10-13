namespace Bery0za.Ariadne.Framework
{
    public static class Extensions
    {
        public static void DestroyWithChildren(this IContext context)
        {
            foreach (IContext child in context.DefineChildren())
            {
                child.DestroyWithChildren();
            }

            context.Destroy();
        }
    }
}