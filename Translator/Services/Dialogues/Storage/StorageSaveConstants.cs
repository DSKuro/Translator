using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translator.Services.Dialogues.Storage
{
    public class StorageSaveConstants
    {
        public bool ShowPrompt { get; private set; }

        public string Extension { get; private set; }

        public string Value { get; private set; }

        public IReadOnlyList<FilePickerFileType> Types { get; private set; }

        private StorageSaveConstants(bool showPrompt,
            string extension,
            string value,
            IReadOnlyList<FilePickerFileType> types)
        {
            ShowPrompt = showPrompt;
            Extension = extension;
            Value = value;
            Types = types;
        }

        public static StorageSaveConstants OpenBaseTextFile
        {
            get
            {
                return new StorageSaveConstants(true,
                    ".txt",
                    "Выберите текстовый файл:",
                    new List<FilePickerFileType> 
                    {
                        new("Text files")
                        {
                            Patterns = new[] { "*.txt" },
                            AppleUniformTypeIdentifiers = new[] { "*.txt" },
                            MimeTypes = null
                        }
                    }
                );          
            }
        }
    }
}
