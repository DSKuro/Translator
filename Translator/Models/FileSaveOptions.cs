using Avalonia.Platform.Storage;
using System.Collections.Generic;

namespace Translator.Models
{
    public class FileSaveOptions
    {
        public bool ShowOverwritePrompt
        {
            get;
        }

        public string Extension
        {
            get;
        }

        public string Title
        {
            get;
        }

        public IReadOnlyList<FilePickerFileType> Types
        {
            get;
        }

        public FileSaveOptions(bool showOverwritePromt,
            string extension,
            string title,
            IReadOnlyList<FilePickerFileType> types)
        {
            ShowOverwritePrompt = showOverwritePromt;
            Extension = extension;
            Title = title;
            Types = types;
        }
    }
}
