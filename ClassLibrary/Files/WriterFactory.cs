using ClassLibrary.Files.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ClassLibrary.Files
{
    public class WriterFactory : IWriterFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public WriterFactory(IServiceProvider serviceProviders)
        {
            _serviceProvider = serviceProviders;
        }

        public IWriter CreateWriter(bool append, string path)
        {
            return ActivatorUtilities.CreateInstance<Writer>(_serviceProvider, append, path);
        }
    }
}
