using Microsoft.Extensions.DependencyInjection;
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
            AddViewModelsAndWindows();
        }

        private void AddStorage()
        {
            _serviceCollection.AddTransient<IDialogueManager, DialogueManager>();
            _serviceCollection.AddTransient<IDialogueHelper, DialogueHelper>();
            _serviceCollection.AddTransient<IMessageBoxService, MessageBoxService>();
            _serviceCollection.AddTransient<IStorageService, StorageService>();
        }

        private void AddViewModelsAndWindows()
        {
            _serviceCollection.AddTransient<MainWindowViewModel>();
        }
    }
}