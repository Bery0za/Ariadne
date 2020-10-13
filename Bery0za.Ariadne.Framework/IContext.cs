using System.Collections.Generic;

namespace Bery0za.Ariadne.Framework
{
    public interface IContext
    {
        IEnumerable<IContext> DefineChildren();
        void Destroy();
    }
}