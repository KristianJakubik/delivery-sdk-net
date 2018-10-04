using System;

namespace KenticoCloud.Delivery.Configuration
{
    public class DeliveryOptionsBuilder
    {
        private readonly DeliveryOptions _deliveryOptions = new DeliveryOptions();

        public DeliveryOptionsBuilder WithProductionEndpoint(string productionEndpoint)
        {
            productionEndpoint.Validate(nameof(productionEndpoint));
            _deliveryOptions.ProductionEndpoint = productionEndpoint;

            return this;
        }

        public DeliveryOptionsBuilder WithPreviewEndpoint(string previewEndpoint)
        {
            previewEndpoint.Validate(nameof(previewEndpoint));
            _deliveryOptions.PreviewEndpoint = previewEndpoint;

            return this;
        }

        public DeliveryOptionsBuilder WithProjectId(string projectId)
        {
            Validation.ValidateProjectId(projectId);
            _deliveryOptions.ProjectId = projectId;

            return this;
        }

        public DeliveryOptionsBuilder WithPreviewApiKey(string previewApiKey)
        {
            previewApiKey.Validate(nameof(previewApiKey));
            _deliveryOptions.PreviewApiKey = previewApiKey;

            return this;
        }

        public DeliveryOptionsBuilder UsePreviewApi()
        {
            _deliveryOptions.UsePreviewApi = true;

            return this;
        }

        public DeliveryOptionsBuilder WaitForLoadingNewContent()
        {
            _deliveryOptions.WaitForLoadingNewContent = true;

            return this;
        }

        public DeliveryOptionsBuilder UseSecuredProductionApi()
        {
            _deliveryOptions.UseSecuredProductionApi = true;

            return this;
        }

        public DeliveryOptionsBuilder WithSecuredProductionApiKey(string securedProductionApiKey)
        {
            securedProductionApiKey.Validate(nameof(securedProductionApiKey));
            _deliveryOptions.SecuredProductionApiKey = securedProductionApiKey;

            return this;
        }

        public DeliveryOptionsBuilder EnableResilienceLogic()
        {
            _deliveryOptions.EnableResilienceLogic = true;

            return this;
        }

        public DeliveryOptionsBuilder WithMaxRetryAttempts(int attempts)
        {
            _deliveryOptions.MaxRetryAttempts = attempts;

            return this;
        }

        public DeliveryOptions Build()
        {
            ValidateOptions();
            return _deliveryOptions;
        }

        private void ValidateOptions()
        {
            if (_deliveryOptions.UsePreviewApi && _deliveryOptions.PreviewApiKey == null)
            {
                throw new Exception("Preview API key is not specified");
            }

            if (_deliveryOptions.UseSecuredProductionApi && _deliveryOptions.SecuredProductionApiKey == null)
            {
                throw new Exception("Production API key is not specified");
            }
        }
    }

    internal static class StringValidationExtensions
    {
        public static void Validate(this string text, string parameterName)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException(parameterName, "Parameter is not specified");
            }
        }
    }
}
