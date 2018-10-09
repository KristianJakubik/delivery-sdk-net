using System;
using System.Collections;
using System.Collections.Generic;
using KenticoCloud.Delivery.InlineContentItems;
using KenticoCloud.Delivery.ResiliencePolicy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KenticoCloud.Delivery
{
    internal class Container : IServiceCollection
    {
        private readonly IDictionary<Type, Lazy<object>> _services = new Dictionary<Type, Lazy<object>>(9);

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void RegisterAllDependencies()
        {
            TryAdd<IInlineContentItemsResolver<object>>(new ReplaceWithWarningAboutRegistrationResolver());
            TryAdd<IInlineContentItemsResolver<UnretrievedContentItem>>(new ReplaceWithWarningAboutUnretrievedItemResolver());
            TryAdd<IInlineContentItemsProcessor>(new InlineContentItemsProcessor(
                GetService<IInlineContentItemsResolver<object>>(),
                GetService<IInlineContentItemsResolver<UnretrievedContentItem>>()
            ));
            TryAdd<ICodeFirstPropertyMapper>(new CodeFirstPropertyMapper());
            TryAdd<ICodeFirstModelProvider>(new CodeFirstModelProvider(
                GetService<IContentLinkUrlResolver>(),
                GetService<IInlineContentItemsProcessor>(),
                GetService<ICodeFirstTypeProvider>(),
                GetService<ICodeFirstPropertyMapper>()
            ));
            TryAdd<IOptions<DeliveryOptions>>(new OptionsWrapper<DeliveryOptions>(new DeliveryOptions()));
            TryAdd<IResiliencePolicyProvider>(new DefaultResiliencePolicyProvider(GetService<IOptions<DeliveryOptions>>().Value.MaxRetryAttempts));
        }

        public void TryAdd<TService>(object implementation)
        {
            if (implementation == null || _services.ContainsKey(typeof(TService))) return;

            _services[typeof(TService)] = new Lazy<object>(() => implementation);
        }

        public void Add(ServiceDescriptor item)
        {

        }

        public TType GetService<TType>()
        {
            _services.TryGetValue(typeof(TType), out var implementation);

            return (TType)implementation?.Value;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(ServiceDescriptor item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ServiceDescriptor item)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsReadOnly { get; }

        public int IndexOf(ServiceDescriptor item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public ServiceDescriptor this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }
}
