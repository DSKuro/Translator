using ClassLibrary.Files.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ClassLibrary.Files
{
    public class ReaderFactory : IReaderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ReaderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IReader CreateReader(string path)
        {
            return ActivatorUtilities.CreateInstance<Reader>(_serviceProvider, path);
        }
    }
}
