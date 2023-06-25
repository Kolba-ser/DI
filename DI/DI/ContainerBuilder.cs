namespace DI
{
    public class ContainerBuilder : IContainerBuilder
    {
        private readonly List<ServiceDescriptor> descriptors = new();
        private readonly IActiovationBuilder actiovationBuilder;

        public ContainerBuilder(IActiovationBuilder actiovationBuilder)
        {
            this.actiovationBuilder = actiovationBuilder;
        }

        public IContainer Build()
        {
            return new Container(descriptors, actiovationBuilder);
        }

        public void Register(ServiceDescriptor descriptor)
        {
            descriptors.Add(descriptor);
        }
    }
}