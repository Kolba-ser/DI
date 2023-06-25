using Autofac;
using BenchmarkDotNet.Attributes;
using DI;
using Microsoft.Extensions.DependencyInjection;

//BenchmarkRunner.Run<ContainerBenchmark>();
/*IContainerBuilder builder = new DI.ContainerBuilder(new LambdaBaseActivationBuilder());
using (var container = builder.RegisterTransient<IService, Service>()
    .RegisterScoped<Controller, Controller>()
    .RegisterSingleton<IAnotherService>(AnotherServiceInstance.Instance)
    .Build())
{
    var scope = container.CreateScope();
    var controller1 = scope.Resolve(typeof(Controller));
    var controller2 = scope.Resolve(typeof(Controller));
    var scope2 = container.CreateScope();
    var controller3 = scope2.Resolve(typeof(Controller));
    var i1 = scope.Resolve(typeof(IAnotherService));
    var i2 = scope2.Resolve(typeof(IAnotherService));

    if (controller1 != controller2)
    {
        throw new InvalidOperationException();
    }

    if (controller1 == controller3)
    {
        throw new InvalidOperationException();
    }
}*/

[MemoryDiagnoser]
public class ContainerBenchmark
{
    private readonly IScope reflectionBased, lambdaBased;
    private readonly Autofac.ILifetimeScope scopeAutofac;
    private readonly IServiceScope scopeMSDI;

    public ContainerBenchmark()
    {
        var reflectionBasedBuilder = new DI.ContainerBuilder(new ReflectionBasedActivationBuilder());
        var lambdsBasedBuilder = new DI.ContainerBuilder(new LambdaBaseActivationBuilder());

        InitContainer(reflectionBasedBuilder);
        InitContainer(lambdsBasedBuilder);

        reflectionBased = reflectionBasedBuilder.Build().CreateScope();
        lambdaBased = lambdsBasedBuilder.Build().CreateScope();
        scopeAutofac = InitAutofac();
        scopeMSDI = InitMSDI();
    }

    private void InitContainer(DI.ContainerBuilder builder)
    {
        builder.RegisterTransient<IService, Service>()
            .RegisterTransient<Controller, Controller>();
    }

    private ILifetimeScope InitAutofac()
    {
        var containerBuilder = new Autofac.ContainerBuilder();
        containerBuilder.RegisterType<Service>().As<IService>();
        containerBuilder.RegisterType<Controller>().AsSelf();
        return containerBuilder.Build().BeginLifetimeScope();
    }

    private IServiceScope InitMSDI()
    {
        var collection = new ServiceCollection();
        collection.AddTransient<IService, Service>();
        collection.AddTransient<Controller, Controller>();
        return collection.BuildServiceProvider().CreateScope();
    }

    [Benchmark(Baseline = true)]
    public Controller Create() => new Controller(new Service());

    [Benchmark]
    public Controller Reflection() => (Controller)reflectionBased.Resolve(typeof(Controller));

    [Benchmark]
    public Controller Lambda() => (Controller)lambdaBased.Resolve(typeof(Controller));

    [Benchmark]
    public Controller Autofac() => scopeAutofac.Resolve<Controller>();

    [Benchmark]
    public Controller MSDI() => scopeMSDI.ServiceProvider.GetRequiredService<Controller>();
}


public class Helper : IHelper
{
}

public interface IHelper
{
}

internal class AnotherServiceInstance
{
    public static AnotherServiceInstance Instance = new();
}

internal interface IAnotherService
{
}

public class Controller
{
    private readonly IService service;

    public Controller(IService service)
    {
        this.service = service;
    }
}

public interface IService
{
}

public class Service : IService
{
}