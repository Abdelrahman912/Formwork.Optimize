using System.IO;

namespace FormworkOptimize.App.Utils
{
    public static class IOUtils
    {
        public static bool IsFileinUse(this FileInfo file)
        {
            if (!File.Exists(file.FullName))
                return false;

            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }

    }
}
