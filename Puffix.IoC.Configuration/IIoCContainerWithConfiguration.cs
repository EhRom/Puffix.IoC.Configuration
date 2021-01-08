using Microsoft.Extensions.Configuration;

namespace Puffix.IoC.Configuration
{
    public interface IIoCContainerWithConfiguration : IIoCContainer
    {
        IConfigurationRoot ConfigurationRoot { get; }
    }
}
