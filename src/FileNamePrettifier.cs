using System;
using System.IO;
using System.Linq;

namespace FolderPrettifier
{
    public class PrettifyOptions
    {
        public bool Capitalize { get; set; }

        public bool Replace { get; set; }

        public string ReplaceFrom { get; set; }

        public string ReplaceTo { get; set; }

        public bool UseNameWith { get; set; }

        public string Prefix { get; set; }

        public string Suffix { get; set; }
    }

    public static class FileNamePrettifier
    {
        public static string Prettify(string fileName, PrettifyOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (fileName == null) throw new ArgumentNullException("fileName");

            string newFileName = fileName;

            if (options.Capitalize && newFileName.Length > 0)
            {
                newFileName = char.ToUpper(newFileName[0]) + newFileName.Substring(1);
            }

            if (options.Replace && !string.IsNullOrEmpty(options.ReplaceFrom))
            {
                newFileName = newFileName.Replace(options.ReplaceFrom, options.ReplaceTo);
            }

            if (options.UseNameWith)
            {
                int dotIndex = newFileName.LastIndexOf('.');
                if (dotIndex > 0)
                {
                    string namePart = newFileName.Substring(0, dotIndex);
                    string extPart = newFileName.Substring(dotIndex);
                    newFileName = options.Prefix + namePart + options.Suffix + extPart;
                }
                else
                {
                    newFileName = options.Prefix + newFileName + options.Suffix;
                }
            }

            return newFileName;
        }

        public static string Sanitize(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            char[] invalidChars = Path.GetInvalidFileNameChars();
            return new string(name.Where(c => !invalidChars.Contains(c)).ToArray());
        }
    }
}
