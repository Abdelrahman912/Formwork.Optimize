using CSharp.Functional.Constructs;
using Unit = System.ValueTuple;

namespace FormworkOptimize.App.ViewModels.Interfaces
{
    public interface IValidateViewModel
    {
        Validation<Unit> Validate();
    }
}
