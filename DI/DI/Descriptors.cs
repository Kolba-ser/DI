namespace DI
{
    public abstract class ServiceDescriptor
    {
        public Type ServiceType
        {
            get; init;
        }

        public LifeTime LifeTime
        {
            get; init;
        }
    }

    /// <summary>
    /// Описывает Transient(недолновечный) экземпляр сервиса
    /// </summary>
    public class TypeBasedServiceDescriptor : ServiceDescriptor
    {
        public Type ImplementationType
        {
            get; init;
        }
    }

    /// <summary>
    /// Описывает сервис создающийся при помощи переданного делегата
    /// </summary>
    public class FactoryBasedServiceDescriptor : ServiceDescriptor
    {
        public Func<IScope, object> Factory
        {
            get; init;
        }
    }

    /// <summary>
    /// Описывает сервис(Singleton)
    /// </summary>
    public class InstanceBasedServiceDescriptor : ServiceDescriptor
    {
        public object Instance
        {
            get; init;
        }

        public InstanceBasedServiceDescriptor(Type serviceType, object instance)
        {
            Instance = instance;
            LifeTime = LifeTime.Singleton;
            ServiceType = serviceType;
        }
    }
}