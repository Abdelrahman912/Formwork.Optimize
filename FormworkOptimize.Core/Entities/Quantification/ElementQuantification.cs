using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace FormworkOptimize.Core.Entities.Quantification
{
    public class ElementQuantification
    {
        #region Properties

        public string Name { get;  }

        public int Count { get; }

        public Level Level { get;}

        public IEnumerable<ElementId> Elements { get; }

        #endregion

        #region Constructors

        public ElementQuantification(string name, int count, Level level, IEnumerable<ElementId> elements)
        {
            Name = name;
            Count = count;
            Level = level;
            Elements = elements;
        }
        #endregion

    }
}
