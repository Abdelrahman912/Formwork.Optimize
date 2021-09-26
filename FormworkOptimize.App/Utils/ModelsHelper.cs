using CSharp.Functional.Constructs;
using CSharp.Functional.Errors;
using FormworkOptimize.App.Models;
using FormworkOptimize.App.UI.Services;

namespace FormworkOptimize.App.Utils
{
    public static  class ModelsHelper
    {

        public static ResultMessage ToResult(this Exceptional<string> message)=>
            message.Match(ex=>new ResultMessage(ex.Message,ResultMessageType.ERROR),
                          value=>new ResultMessage(value,ResultMessageType.DONE));

        public static ResultMessage ToResult(this Error err) =>
            new ResultMessage(err.Message, ResultMessageType.ERROR);

    }
}
