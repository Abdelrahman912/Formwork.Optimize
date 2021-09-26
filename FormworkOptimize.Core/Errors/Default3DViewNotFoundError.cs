using CSharp.Functional.Errors;

namespace FormworkOptimize.Core.Errors
{
    public class Default3DViewNotFoundError:Error
    {
        public override string Message
            => "\"{3D}\" View not found, Please Click 3D Button in the quick acess bar to add one";
    }
}
