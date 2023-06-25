namespace DI
{
    public enum LifeTime
    {
        Transient,
        Scoped,
        Singleton
    }

    public interface IContainerBuilder
    {
        IContainer Build();

        void Register(ServiceDescriptor descriptor);
    }

    public interface IContainer : IDisposable, IAsyncDisposable
    {
        IScope CreateScope();
    }

    public interface IScope
    {
        /// <summary>
        /// Выдает экземпляр объекта.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        object Resolve(Type service);
    }
}