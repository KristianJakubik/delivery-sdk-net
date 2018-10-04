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
            private IContentLinkUrlResolver _contentLinkUrlResolver;
            private IInlineContentItemsProcessor _inlineContentItemsProcessor;
            private ICodeFirstModelProvider _codeFirstModelProvider;
            private IResiliencePolicyProvider _resiliencePolicyProvider;
            private ICodeFirstTypeProvider _codeFirstTypeProvider;
            private ICodeFirstPropertyMapper _codeFirstPropertyMapper;
            private HttpClient _httpClient;
            private DeliveryOptions _deliveryOptions;
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
                _contentLinkUrlResolver = contentLinkUrlResolver;

                return this;
            }

            public IOptionalStep WithInlineContentItemsProcessor(IInlineContentItemsProcessor inlineContentItemsProcessor)
            {
                _inlineContentItemsProcessor = inlineContentItemsProcessor;

                return this;
            }

            public IOptionalStep WithCodeFirstModelProvider(ICodeFirstModelProvider codeFirstModelProvider)
            {
                _codeFirstModelProvider = codeFirstModelProvider;

                return this;
            }

            public IOptionalStep WithCodeFirstTypeProvider(ICodeFirstTypeProvider codeFirstTypeProvider)
            {
                _codeFirstTypeProvider = codeFirstTypeProvider;

                return this;
            }

            public IOptionalStep WithResiliencePolicyProvider(IResiliencePolicyProvider resiliencePolicyProvider)
            {
                _resiliencePolicyProvider = resiliencePolicyProvider;

                return this;
            }

            public IOptionalStep WithCodeFirstPropertyMapper(ICodeFirstPropertyMapper propertyMapper)
            {
                _codeFirstPropertyMapper = propertyMapper;

                return this;
            }

            public IDeliveryClient Build()
            {
                var deliveryOptionsWrapper = new OptionsWrapper<DeliveryOptions>(_deliveryOptions);

                var inlineContentItemsProcessor =
                    _inlineContentItemsProcessor ??
                    new InlineContentItemsProcessor(
                        new ReplaceWithWarningAboutRegistrationResolver(),
                        new ReplaceWithWarningAboutUnretrievedItemResolver()
                    );
                var codeFirstModelProvider =
                    _codeFirstModelProvider ??
                    new CodeFirstModelProvider(
                        _contentLinkUrlResolver,
                        inlineContentItemsProcessor,
                        _codeFirstTypeProvider,
                        _codeFirstPropertyMapper
                    );

                return new DeliveryClient(
                    deliveryOptionsWrapper,
                    _contentLinkUrlResolver,
                    inlineContentItemsProcessor,
                    codeFirstModelProvider,
                    _resiliencePolicyProvider,
                    _codeFirstTypeProvider,
                    _codeFirstPropertyMapper
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
