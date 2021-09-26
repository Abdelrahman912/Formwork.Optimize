using FormworkOptimize.Core.Constants;
using FormworkOptimize.Core.Enums;
using FormworkOptimize.Core.Exceptions;
using System.Linq;

namespace FormworkOptimize.Core.Helpers.DesignHelper
{
    public static class ShoringHelper
    {
        public static double GetCuplockCapacity(double lcr , SteelType steelType)
        {
            var allowableList = Database.SteelTypesAllowableLoad[steelType];
            var greaterLcrTuple = allowableList.OrderBy(tuple => tuple.Item1)
                                               .FirstOrDefault(tuple => tuple.Item1 >= lcr);
            if(greaterLcrTuple == null)
            {
                var maxLcrTuple = allowableList.OrderBy(tuple => tuple.Item1).Last();
                throw new LayoutException($"Lcr = {lcr} cm is greater than permissible {maxLcrTuple.Item1} cm.");
            }
            var smallerLcrTuple = allowableList.OrderByDescending(tuple => tuple.Item1)
                                               .FirstOrDefault(tuple => tuple.Item1 < lcr);
            if (smallerLcrTuple == null)
            {
                var minLcrTuple = allowableList.OrderBy(tuple => tuple.Item1).First();
                return minLcrTuple.Item2;
            }

            return greaterLcrTuple.Item2+ (((smallerLcrTuple.Item2-greaterLcrTuple.Item2)*(greaterLcrTuple.Item1-lcr))/(greaterLcrTuple.Item1-smallerLcrTuple.Item1));
        }
    }
}
