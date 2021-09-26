using CSharp.Functional.Errors;

namespace FormworkOptimize.Core.Errors
{
    public class FileUsedByAnotherProcessError:Error
    {

        override public string Message {  get; }

        public FileUsedByAnotherProcessError(string filePath)
        {
            Message = $"File: \"{filePath}\" is Opened by another process. Please Close the file first.";
        }


    }
}
