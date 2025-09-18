using Avalonia.Platform.Storage;

namespace Translator.Models
{
    public class FileOpenOptions
    {
        public bool AllowMultiple
        {
            get;
        }
        public string Title
        {
            get;
        }
        public FilePickerFileType Filter
        {
            get;
        }

        public FileOpenOptions(string title, FilePickerFileType filter,
            bool allowMultiple = false)
        {
            Title = title;
            Filter = filter;
            AllowMultiple = allowMultiple;
        }
    }
}
