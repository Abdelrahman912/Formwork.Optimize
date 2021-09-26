using CSharp.Functional.Errors;

namespace FormworkOptimize.Core.Errors
{
  public  class InvalidPolygonError:Error
    {

        override public string Message =>
            "Selected Lines don't form a closed polygon.";
    }
}
