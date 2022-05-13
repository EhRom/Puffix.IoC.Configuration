# Puffix IoC Configuration

Extension for including Microsoft.Extensions.Configuration in the [Puffix IoC](https://github.com/EhRom/Puffix.IoC) library.

[![NuGet version (Puffix.IoC.Configuration)](https://img.shields.io/nuget/v/Puffix.IoC.Configuration.svg?style=flat-square)](https://www.nuget.org/packages/Puffix.IoC.Configuration/)
[![Build status](https://github.com/EhRom/Puffix.IoC.Configuration/workflows/.NET%20Core/badge.svg)](https://github.com/EhRom/Puffix.IoC.Configuration/actions?query=workflow%3A%22.NET+Core%22)


## Minimal implementation
In the sample below, [Autofac](https://autofac.org/) is used.

First, a base class is needed to reference the objects to map:

``` csharp
using Autofac;
using Microsoft.Extensions.Configuration;
using Puffix.IoC;
using Puffix.IoC.Configuration;

namespace YourAppName;

public class IoCContainer : IIoCContainerWithConfiguration
{
    private readonly IContainer? container;

    public IConfigurationRoot ConfigurationRoot { get; }

    public IoCContainer(ContainerBuilder containerBuilder, IConfigurationRoot configuration)
    {
        // Self-register the container.
        containerBuilder.Register(_ => this).As<IIoCContainerWithConfiguration>().SingleInstance();
        containerBuilder.Register(_ => this).As<IIoCContainer>().SingleInstance();

        container = containerBuilder.Build();
        ConfigurationRoot = configuration;
    }

    public static IIoCContainerWithConfiguration BuildContainer(IConfigurationRoot configuration)
    {
        ContainerBuilder containerBuilder = new ContainerBuilder();

        containerBuilder.RegisterAssemblyTypes
                        (
                            typeof(IoCContainer).Assembly // Current Assembly.
                        )
                        .AsSelf()
                        .AsImplementedInterfaces();

        containerBuilder.RegisterInstance(configuration).SingleInstance();

        return new IoCContainer(containerBuilder, configuration);
    }

    public ObjectT Resolve<ObjectT>(params IoCNamedParameter[] parameters)
        where ObjectT : class
    {
        if (container == null)
            throw new ArgumentNullException($"The class {GetType().Name} is not well initialized.");

        ObjectT resolvedObject;
        if (parameters != null)
            resolvedObject = container.Resolve<ObjectT>(ConvertIoCNamedParametersToAutfac(parameters));
        else
            resolvedObject = container.Resolve<ObjectT>();

        return resolvedObject;
    }

    public object Resolve(Type objectType, params IoCNamedParameter[] parameters)
    {
        if (container == null)
            throw new ArgumentNullException($"The class {GetType().Name} is not well initialized.");

        object resolvedObject;
        if (parameters != null)
            resolvedObject = container.Resolve(objectType, ConvertIoCNamedParametersToAutfac(parameters));
        else
            resolvedObject = container.Resolve(objectType);

        return resolvedObject;
    }

    private IEnumerable<NamedParameter> ConvertIoCNamedParametersToAutfac(IEnumerable<IoCNamedParameter> parameters)
    {
        foreach (var parameter in parameters)
        {
            if (parameter != null)
                yield return new NamedParameter(parameter.Name, parameter.Value);
        }
    }
}
```

To access [Microsoft .NET configuration framework](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration), you need to reference some *Microsoft.Extensions.Configuration.\** packages.

In the sample below, the following packages are referenced:
- [Microsoft.Extensions.Configuration.Binder](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Binder/) to easily get typed values,
- [Microsoft.Extensions.Configuration.Json](https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Json/) to load configuration from a json file.

The container may be initialized like this:

``` csharp
IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
            .Build();

IIoCContainerWithConfiguration container = IoCContainer.BuildContainer();
```

And then you can resolve your objects in the code:

``` csharp
MyObject myObject = container.Resolve<MyObject>();
```

You can also access to the configuration like this:

``` csharp
string myStringParameter = container.ConfigurationRoot[nameof(myStringParameter)];
long myLongParameter = container.ConfigurationRoot.GetValue<int>(nameof(myLongParameter));
```

## Use Polly to manage HttpClient

[Polly](http://www.thepollyproject.org/) is a library used to better manage the `HttpClient` instances.

The following packages are needed:
- [Autofac.Extensions.DependencyInjection](https://www.nuget.org/packages/Autofac.Extensions.DependencyInjection/),
- [Microsoft.Extensions.Http](https://www.nuget.org/packages/Microsoft.Extensions.Http/),
- [Microsoft.Extensions.Http.Polly](https://www.nuget.org/packages/Microsoft.Extensions.Http.Polly/),
- [Polly](https://www.nuget.org/packages/Polly/).

First, a base class is needed to reference the objects to map:

``` csharp
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Puffix.IoC;
using Puffix.IoC.Configuration;

namespace YourAppName;

public class IoCContainer : IIoCContainerWithConfiguration
{
    private readonly IContainer? container;

    public IConfigurationRoot ConfigurationRoot { get; }

    public IoCContainer(ContainerBuilder containerBuilder, IConfigurationRoot configuration)
    {
        // Self-register the container.
        containerBuilder.Register(_ => this).As<IIoCContainerWithConfiguration>().SingleInstance();
        containerBuilder.Register(_ => this).As<IIoCContainer>().SingleInstance();

        container = containerBuilder.Build();
        ConfigurationRoot = configuration;
    }

    public static IIoCContainerWithConfiguration BuildContainer(IConfigurationRoot configuration)
    {
        // Register HttpClientMessageFactory
        ServiceCollection services = new ServiceCollection();
        services.AddHttpClient();

        AutofacServiceProviderFactory providerFactory = new AutofacServiceProviderFactory();
        ContainerBuilder containerBuilder = providerFactory.CreateBuilder(services);

        containerBuilder.RegisterAssemblyTypes
                        (
                            typeof(IoCContainer).Assembly // Current Assembly.
                        )
                        .AsSelf()
                        .AsImplementedInterfaces();

        containerBuilder.RegisterInstance(configuration).SingleInstance();

        return new IoCContainer(containerBuilder, configuration);
    }

    public ObjectT Resolve<ObjectT>(params IoCNamedParameter[] parameters)
        where ObjectT : class
    {
        if (container == null)
            throw new ArgumentNullException($"The class {GetType().Name} is not well initialized.");

        ObjectT resolvedObject;
        if (parameters != null)
            resolvedObject = container.Resolve<ObjectT>(ConvertIoCNamedParametersToAutfac(parameters));
        else
            resolvedObject = container.Resolve<ObjectT>();

        return resolvedObject;
    }

    public object Resolve(Type objectType, params IoCNamedParameter[] parameters)
    {
        if (container == null)
            throw new ArgumentNullException($"The class {GetType().Name} is not well initialized.");

        object resolvedObject;
        if (parameters != null)
            resolvedObject = container.Resolve(objectType, ConvertIoCNamedParametersToAutfac(parameters));
        else
            resolvedObject = container.Resolve(objectType);

        return resolvedObject;
    }

    private IEnumerable<NamedParameter> ConvertIoCNamedParametersToAutfac(IEnumerable<IoCNamedParameter> parameters)
    {
        foreach (var parameter in parameters)
        {
            if (parameter != null)
                yield return new NamedParameter(parameter.Name, parameter.Value);
        }
    }
}
```

The container may be initialized like this:

``` csharp
IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
            .Build();

IIoCContainerWithConfiguration container = IoCContainer.BuildContainer();
```

And then you can resolve your objects in the code:

``` csharp
MyObject myObject = container.Resolve<MyObject>();
```

You can also access to the configuration like this (refer to the paragraph below for the required packages):

``` csharp
string myStringParameter = container.ConfigurationRoot[nameof(myStringParameter)];
long myLongParameter = container.ConfigurationRoot.GetValue<int>(nameof(myLongParameter));
```

Finally, here is a sample class where an `HttpClient` is needed:

``` csharp
using System.Text.Json;

namespace Puffix.Ovh.Api.Infra;

public MyClassWithHttpClient
{
    private readonly IHttpClientFactory httpClientFactory;

    public MyClassWithHttpClient(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public Task<string> CallHttpAsync(Uri targetUri)
    {
        using HttpClient httpClient = httpClientFactory.CreateClient();

        await AddHeadersAsync(httpClient, queryInformation);

        using HttpResponseMessage response = await httpClient.GetAsync(targetUri);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error {response.StatusCode}: {response.ReasonPhrase} >> {errorContent}");
        }

        return  await response.Content.ReadAsStringAsync();
    }
}
```