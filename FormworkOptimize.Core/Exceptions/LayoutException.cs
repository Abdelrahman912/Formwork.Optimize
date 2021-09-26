using System;

namespace FormworkOptimize.Core.Exceptions
{
    public class LayoutException :Exception
    {
        public LayoutException(string message)
            :base(message)
        {
            //TODO: More properties can be added to describe Exception percisely.
        }
    }
}
