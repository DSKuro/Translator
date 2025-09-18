using Avalonia.Platform.Storage;

namespace Translator.Services.Dialogues.Storage
{
    public class StorageOpenConstants
    {
        public string Value { get; private set; }
        public FilePickerFileType Type { get; private set; }

        private StorageOpenConstants(string value, FilePickerFileType type)
        {
            Value = value;
            Type = type;
        }

        public static StorageOpenConstants OpenBaseTextFile
        {
            get
            {
                return new StorageOpenConstants("Выберите текстовый файл:",
                    new("Text files")
                    {
                        Patterns = new[] { "*.txt" },
                        AppleUniformTypeIdentifiers = new[] { "*.txt" },
                        MimeTypes = null
                    });
            }
        }
    }
}
