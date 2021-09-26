using Autodesk.Revit.DB;
using FormworkOptimize.Core.Constants;

namespace FormworkOptimize.App.Models
{
    public class FloorModel
    {
        public Floor Floor { get; }

        public string Name { get; }

        public FloorModel(Floor floor)
        {
            var doc = RevitBase.Document;
            Floor = floor;
            Name =$"{doc.GetElement(floor.LevelId).Name} | { floor.Name}";
        }

    }
}
