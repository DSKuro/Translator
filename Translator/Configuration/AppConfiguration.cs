using Avalonia.Controls;
using Avalonia.Logging;
using ClassLibrary.Files;
using ClassLibrary.Files.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Translator.Services.Dialogues.Base;
using Translator.Services.Dialogues.MessageBox;
using Translator.Services.Dialogues.Storage;
using Translator.ViewModels;

namespace Translator.Configuration
{
    public class AppConfiguration
    {
        private ServiceCollection _serviceCollection;

        public AppConfiguration()
        {
            _serviceCollection = new ServiceCollection();
        }

        public ServiceProvider BuildServiceProvider()
        {
            BuildServiceCollectionImpl();
            return _serviceCollection.BuildServiceProvider();
        }

        private void BuildServiceCollectionImpl()
        {
            AddStorage();
            AddFiles();
            AddViewModelsAndWindows();
        }

        private void AddStorage()
        {
            _serviceCollection.AddTransient<IDialogueManager, DialogueManager>();
            _serviceCollection.AddTransient<IDialogueHelper, DialogueHelper>();
            _serviceCollection.AddTransient<IMessageBoxService, MessageBoxService>();
            _serviceCollection.AddTransient<IStorageService, StorageService>();
        }

        private void AddFiles()
        {
            _serviceCollection.AddTransient<IReaderFactory, ReaderFactory>();
            _serviceCollection.AddTransient<IWriterFactory, WriterFactory>();
        }

        private void AddViewModelsAndWindows()
        {
            _serviceCollection.AddTransient<MainWindowViewModel>();
        }
    }
}
