using System.Linq.Expressions;
using System.Reflection;

namespace DI
{
    public abstract class BasedActivationBuilder : IActiovationBuilder
    {
        public Func<IScope, object> BuildActivation(ServiceDescriptor descriptor)
        {
            var tb = (TypeBasedServiceDescriptor)descriptor;

            var ctor = tb.ImplementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Single();
            var args = ctor.GetParameters();

            return BuildActivationInternal(tb, ctor, args, descriptor);
        }

        protected abstract Func<IScope, object> BuildActivationInternal(TypeBasedServiceDescriptor tb, ConstructorInfo ctor, ParameterInfo[] args, ServiceDescriptor descriptor);
    }

    public class ReflectionBasedActivationBuilder : BasedActivationBuilder
    {
        protected override Func<IScope, object> BuildActivationInternal(TypeBasedServiceDescriptor tb, ConstructorInfo ctor, ParameterInfo[] args, ServiceDescriptor descriptor)
        {
            return scope =>
            {
                var argsForCtor = new object[args.Length];

                for (int i = 0; i < args.Length; i++)
                {
                    argsForCtor[i] = scope.Resolve(args[i].ParameterType);
                }

                return ctor.Invoke(argsForCtor);
            };
        }
    }

    public class LambdaBaseActivationBuilder : BasedActivationBuilder
    {
        private static readonly MethodInfo ResolveMethod = typeof(IScope).GetMethod("Resolve");

        protected override Func<IScope, object> BuildActivationInternal(TypeBasedServiceDescriptor tb, ConstructorInfo ctor, ParameterInfo[] args, ServiceDescriptor descriptor)
        {
            var scopeParameter = Expression.Parameter(typeof(IScope), "scope");

            //new Controller((IService)scope.Resolve(typeof(Service)), .....);
            var ctorArgs = 
                args.Select(x => 
                    Expression.Convert(Expression.Call(scopeParameter, ResolveMethod,
                        Expression.Constant(x.ParameterType)), x.ParameterType));

            var @new = Expression.New(ctor, ctorArgs);

            var lambda = Expression.Lambda<Func<IScope, object>>(@new, scopeParameter);
            return lambda.Compile();
        }
    }
}