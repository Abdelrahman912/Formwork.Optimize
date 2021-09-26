using CSharp.Functional.Errors;

namespace FormworkOptimize.Core.Errors
{
    public  class MaterialNotFoundError:Error
    {

        override public string Message => 
            "\"Plywood, Sheathing\" is not found, Please add the material before using the Plugin";

    }
}
