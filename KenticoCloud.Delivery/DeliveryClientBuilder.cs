using System;
using System.Net.Http;
using KenticoCloud.Delivery.Configuration;
using KenticoCloud.Delivery.InlineContentItems;
using KenticoCloud.Delivery.ResiliencePolicy;
using Microsoft.Extensions.Options;

namespace KenticoCloud.Delivery
{
    public interface IBuildStep
    {
        IDeliveryClient Build();
    }

    public interface IMandatoryStep
    {
        IOptionalStep WithProjectId(string projectId);
        IOptionalStep WithDeliveryOptions(Func<DeliveryOptionsBuilder, DeliveryOptionsBuilder> buildDeliveryOptions);
    }

    public interface IOptionalStep : IBuildStep
    {
        IOptionalStep WithPreviewApiKey(string previewApiKey);
        IOptionalStep WithHttpClient(HttpClient httpClient);
        IOptionalStep WithContentLinkUrlResolver(IContentLinkUrlResolver contentLinkUrlResolver);
        IOptionalStep WithInlineContentItemsProcessor(IInlineContentItemsProcessor inlineContentItemsProcessor);
        IOptionalStep WithCodeFirstModelProvider(ICodeFirstModelProvider codeFirstModelProvider);
        IOptionalStep WithCodeFirstTypeProvider(ICodeFirstTypeProvider codeFirstTypeProvider);
        IOptionalStep WithResiliencePolicyProvider(IResiliencePolicyProvider resiliencePolicyProvider);
        IOptionalStep WithCodeFirstPropertyMapper(ICodeFirstPropertyMapper propertyMapper);
    }

    public class DeliveryClientBuilder
    {
        public static IMandatoryStep GetBuilder()
        {
            return new Steps();
        }

        private class Steps : IMandatoryStep, IOptionalStep
        {
            private readonly Container _container;
            private HttpClient _httpClient;
            private DeliveryOptions _deliveryOptions;

            public Steps()
            {
                _container = new Container();
            }

            public IOptionalStep WithProjectId(string projectId)
            {
                Validation.ValidateProjectId(projectId);

                _deliveryOptions = new DeliveryOptions {ProjectId = projectId};

                return this;
            }

            public IOptionalStep WithDeliveryOptions(Func<DeliveryOptionsBuilder, DeliveryOptionsBuilder> buildDeliveryOptions)
            {
                _deliveryOptions = buildDeliveryOptions(new DeliveryOptionsBuilder()).Build();

                return this;
            }

            public IOptionalStep WithPreviewApiKey(string previewApiKey)
            {
                Validation.ValidatePreviewApiKey(previewApiKey);

                _deliveryOptions.PreviewApiKey = previewApiKey;
                _deliveryOptions.UsePreviewApi = true;

                return this;
            }

            public IOptionalStep WithHttpClient(HttpClient httpClient)
            {
                _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient), "Http client is not specified");

                return this;
            }

            public IOptionalStep WithContentLinkUrlResolver(IContentLinkUrlResolver contentLinkUrlResolver)
            {
                if (contentLinkUrlResolver != null)
                {
                    _container.TryAdd<IContentLinkUrlResolver>(contentLinkUrlResolver);
                }
                
                return this;
            }

            public IOptionalStep WithInlineContentItemsProcessor(IInlineContentItemsProcessor inlineContentItemsProcessor)
            {
                if (inlineContentItemsProcessor != null)
                {
                    _container.TryAdd<IInlineContentItemsProcessor>(inlineContentItemsProcessor);
                }

                return this;
            }

            public IOptionalStep WithCodeFirstModelProvider(ICodeFirstModelProvider codeFirstModelProvider)
            {
                if (codeFirstModelProvider != null)
                {
                    _container.TryAdd<ICodeFirstModelProvider>(codeFirstModelProvider);
                }

                return this;
            }

            public IOptionalStep WithCodeFirstTypeProvider(ICodeFirstTypeProvider codeFirstTypeProvider)
            {
                if (codeFirstTypeProvider != null)
                {
                    _container.TryAdd<ICodeFirstTypeProvider>(codeFirstTypeProvider);
                }

                return this;
            }

            public IOptionalStep WithResiliencePolicyProvider(IResiliencePolicyProvider resiliencePolicyProvider)
            {
                if (resiliencePolicyProvider != null)
                {
                    _container.TryAdd<IResiliencePolicyProvider>(resiliencePolicyProvider);
                }

                return this;
            }

            public IOptionalStep WithCodeFirstPropertyMapper(ICodeFirstPropertyMapper propertyMapper)
            {
                if (propertyMapper != null)
                {
                    _container.TryAdd<ICodeFirstPropertyMapper>(propertyMapper);
                }

                return this;
            }

            public IDeliveryClient Build()
            {
                _container.TryAdd<IOptions<DeliveryOptions>>(new OptionsWrapper<DeliveryOptions>(_deliveryOptions));
                _container.RegisterAllDependencies();

                var deliveryOptions = _container.GetService<IOptions<DeliveryOptions>>();
                var inlineContentItemsProcessor = _container.GetService<IInlineContentItemsProcessor>();
                var codeFirstModelProvider = _container.GetService<ICodeFirstModelProvider>();
                var contentLinkUrlResolver = _container.GetService<IContentLinkUrlResolver>();
                var codeFirstTypeProvider = _container.GetService<ICodeFirstTypeProvider>();
                var resiliencePolicyProvider = _container.GetService<IResiliencePolicyProvider>();
                var codeFirstPropertyMapper = _container.GetService<ICodeFirstPropertyMapper>();

                return new DeliveryClient(
                    deliveryOptions,
                    contentLinkUrlResolver,
                    inlineContentItemsProcessor,
                    codeFirstModelProvider,
                    resiliencePolicyProvider,
                    codeFirstTypeProvider,
                    codeFirstPropertyMapper
                )
                {
                    HttpClient = _httpClient
                };
            }
        }
    }

    public static class Validation
    {
        public static void ValidatePreviewApiKey(string previewApiKey)
        {
            if (previewApiKey == null)
            {
                throw new ArgumentNullException(nameof(previewApiKey), "The Preview API key is not specified.");
            }

            if (previewApiKey == string.Empty)
            {
                throw new ArgumentException("The Preview API key is not specified.", nameof(previewApiKey));
            }
        }

        public static void ValidateProjectId(string projectId)
        {
            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId), "Kentico Cloud project identifier is not specified.");
            }

            if (projectId == string.Empty)
            {
                throw new ArgumentException("Kentico Cloud project identifier is not specified.", nameof(projectId));
            }

            if (!Guid.TryParse(projectId, out Guid projectIdGuid))
            {
                throw new ArgumentException("Provided string is not a valid project identifier ({ProjectId}). Haven't you accidentally passed the Preview API key instead of the project identifier?", nameof(projectId));
            }
        }
    }
}
