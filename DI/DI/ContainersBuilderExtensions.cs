using DI;

public static class ContainersBuilderExtensions
{
    private static IContainerBuilder RegisterFactory(this IContainerBuilder builder, Type service, Func<IScope, object> factory, LifeTime lifeTime)
    {
        builder.Register(new FactoryBasedServiceDescriptor()
        {
            ServiceType = service,
            Factory = factory,
            LifeTime = lifeTime
        });

        return builder;
    }

    private static IContainerBuilder RegisterType(this IContainerBuilder builder, Type serviceInstance, Type serviceImplementation, LifeTime lifeTime)
    {
        builder.Register(new TypeBasedServiceDescriptor()
        {
            ImplementationType = serviceImplementation,
            ServiceType = serviceInstance,
            LifeTime = lifeTime
        });

        return builder;
    }
    
    private static IContainerBuilder RegisterInstance(this IContainerBuilder builder, Type service, object instance)
    {
        builder.Register(new InstanceBasedServiceDescriptor(service, instance));
        return builder;
    }

    public static IContainerBuilder RegisterSingleton(this IContainerBuilder builder, Type @serviceInstance, Type serviceImplementation) =>
        builder.RegisterType(serviceInstance, serviceImplementation, LifeTime.Singleton);

    public static IContainerBuilder RegisterTransient(this IContainerBuilder builder, Type @serviceInstance, Type serviceImplementation) =>
        builder.RegisterType(serviceInstance, serviceImplementation, LifeTime.Transient);

    public static IContainerBuilder RegisterScoped(this IContainerBuilder builder, Type @serviceInstance, Type serviceImplementation) =>
        builder.RegisterType(serviceInstance, serviceImplementation, LifeTime.Scoped);

    public static IContainerBuilder RegisterSingleton<TService, TImplementation>(this IContainerBuilder builder)
        where TImplementation : TService
        => builder.RegisterType(typeof(TService), typeof(TImplementation), LifeTime.Singleton);

    public static IContainerBuilder RegisterTransient<TService, TImplementation>(this IContainerBuilder builder)
        where TImplementation : TService
        => builder.RegisterType(typeof(TService), typeof(TImplementation), LifeTime.Transient);

    public static IContainerBuilder RegisterScoped<TService, TImplementation>(this IContainerBuilder builder)
        where TImplementation : TService
        => builder.RegisterType(typeof(TService), typeof(TImplementation), LifeTime.Scoped);

    public static IContainerBuilder RegisterTransient<TService>(this IContainerBuilder builder, Func<IScope, TService> factory) =>
        builder.RegisterFactory(typeof(TService), s => factory(s), LifeTime.Transient);

    public static IContainerBuilder RegisterScoped(this IContainerBuilder builder, Type implementation, Func<IScope, object> factory) =>
        builder.RegisterFactory(implementation, factory, LifeTime.Scoped);

    public static IContainerBuilder RegisterSingleton<TService>(this IContainerBuilder builder, object instance)
        => builder.RegisterInstance(typeof(TService), instance);
}