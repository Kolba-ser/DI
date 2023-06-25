using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;

namespace DI
{

    public class Container : IContainer, IDisposable, IAsyncDisposable
    {
        private class Scope : IScope, IDisposable, IAsyncDisposable
        {
            private readonly Container container;
            private readonly ConcurrentDictionary<Type, object> scopedInstances = new();
            // При вызове метода Dispose у другие сервисы могут все еще ссылаться на текущей освобождаемый.
            // И например если какой то сервис во время вызова Dispose должен воспользоваться другим сервисов,
            // то у нас нет гарантии на то что сервис все еще существует.
            // Логика тут такая. Последний зарегестрированный сервис не имеет зависимости на другие сервисы, 
            // а первый созданные еще как может иметь зависимости. 
            // По этому освобождать объекты можно с конца.
            private readonly ConcurrentStack<object> disposables = new();

            public Scope(Container container)
            {
                this.container = container;
            }

            /// <summary>
            /// Выдает экземпляр объекта.
            /// </summary>
            /// <param name="service"></param>
            /// <returns></returns>
            public object Resolve(Type service)
            {
                var descriptor = container.FindDescriptor(service);

                if (descriptor.LifeTime == LifeTime.Transient)
                    return CreateInstanceInternal(service, this);

                // Если LifeTime == Scoped то ищет в этом Scope
                // Если LifeTime == Singletone, то добираемся до rootScope и ищем там
                // Поскольку все Scope создаются от одного экземпляра контейнера,
                // то rootScope содержит все(зарегестрированные) дескрипторы с LifeTime Singeton
                if (descriptor.LifeTime == LifeTime.Scoped || container.rootScope == this)
                    return scopedInstances.GetOrAdd(service, s => container.CreateInstance(s, this));
                else
                    return container.rootScope.Resolve(service);
            }

            public async ValueTask DisposeAsync()
            {
                foreach (var disposable in disposables)
                {
                    if (disposable is IAsyncDisposable ad)
                        await ad.DisposeAsync();
                    else if (disposable is IDisposable d)
                        d.Dispose();
                }
            }

            public void Dispose()
            {
                foreach (var disposable in disposables)
                {
                    if (disposable is IDisposable d)
                        d.Dispose();
                    else if (disposable is IAsyncDisposable ad)
                        ad.DisposeAsync().GetAwaiter().GetResult();
                }
            }

            private object CreateInstanceInternal(Type service, Scope scope)
            {
                var result = container.CreateInstance(service, scope);
                
                if (result is IDisposable)
                    disposables.Push(result);
                else if (result is IAsyncDisposable)
                    disposables.Push(result);

                return result;
            }
        }

        private readonly ImmutableDictionary<Type, ServiceDescriptor> descriptors;
        private readonly ConcurrentDictionary<Type, Func<IScope, object>> buildActivators = new();

        /// <summary>
        /// Здесь содержатся <b>синглтоны</b>
        /// </summary>
        private readonly Scope rootScope;
        private readonly IActiovationBuilder activationBuilder;

        public Container(IEnumerable<ServiceDescriptor> descriptors, IActiovationBuilder actiovationBuilder)
        {
            this.descriptors = descriptors.ToImmutableDictionary(x => x.ServiceType);
            rootScope = new(this);
            this.activationBuilder = actiovationBuilder;
        }

        public IScope CreateScope()
        {
            return new Scope(this);
        }

        private ServiceDescriptor? FindDescriptor(Type service)
        {
            descriptors.TryGetValue(service, out var result);
            return result;
        }

        private Func<IScope, object> BuildActivation(Type service)
        {
            if (!descriptors.TryGetValue(service, out var descriptor))
                throw new InvalidOperationException($"Service {service} is not registered");

            if (descriptor is InstanceBasedServiceDescriptor ib)
                return _ => ib.Instance;

            if (descriptor is FactoryBasedServiceDescriptor fb)
                return fb.Factory;

            var tb = (TypeBasedServiceDescriptor)descriptor;

            var ctor = tb.ImplementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Single();
            var args = ctor.GetParameters();

            return activationBuilder.BuildActivation(descriptor);
        }

        /// <summary>
        /// Создаёт(получает) экземпляр сервиса в зависимости от зарегестрированного <b>ServiceDescriptor</b>
        /// <br></br>
        /// <br>- <b>TypeBasedServiceDescriptor</b> создаётся при помощи преданного IActivationBuilder</br>
        /// <br>- <b>FactoryBasedServiceDescriptor</b> создается при помощи переданного делегата</br>
        /// <br>- <b>InstanceBasedServiceDescriptor</b> возвращает переданный при создании объект</br>
        ///
        /// </summary>
        /// <param name="service"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private object CreateInstance(Type service, IScope scope)
            => buildActivators.GetOrAdd(service, BuildActivation)(scope);

        public ValueTask DisposeAsync() => rootScope.DisposeAsync();

        public void Dispose() => rootScope.Dispose();
    }
}