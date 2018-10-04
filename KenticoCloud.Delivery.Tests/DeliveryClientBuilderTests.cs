using FakeItEasy;
using KenticoCloud.Delivery.ResiliencePolicy;
using Xunit;

namespace KenticoCloud.Delivery.Tests
{
    public class DeliveryClientBuilderTests
    {
        private const string Guid = "e5629811-ddaa-4c2b-80d2-fa91e16bb264";
        private const string PreviewEndpoint = "https://preview-deliver.test.com/";
        private readonly IMandatoryStep _deliveryClientBuilder;

        public DeliveryClientBuilderTests()
        {
            _deliveryClientBuilder = DeliveryClientBuilder.GetBuilder();
        }

        [Fact]
        public void BuildWithProjectId_ReturnsDeliveryClientWithProjectIdSet()
        {
            var deliveryClient = (DeliveryClient) _deliveryClientBuilder.WithProjectId(Guid).Build();

            Assert.Equal(deliveryClient.DeliveryOptions.ProjectId, Guid);
        }

        [Fact]
        public void BuildWithDeliveryOptions_ReturnsDeliveryClientWithDeliveryOptions()
        {
            var deliveryClient = (DeliveryClient) _deliveryClientBuilder
                .WithDeliveryOptions(builder => builder.WithProjectId(Guid).WithPreviewEndpoint(PreviewEndpoint)).Build();

            Assert.Equal(deliveryClient.DeliveryOptions.ProjectId, Guid);
            Assert.Equal(deliveryClient.DeliveryOptions.PreviewEndpoint, PreviewEndpoint);
        }

        [Fact]
        public void BuildWithOptionalSteps_ReturnsDeliveryClientWithSetInstances()
        {
            var mockCodeFirstModelProvider = A.Fake<ICodeFirstModelProvider>();
            var mockResiliencePolicyProvider = A.Fake<IResiliencePolicyProvider>();
            var mockCodeFirstPropertyMapper = A.Fake<ICodeFirstPropertyMapper>();

            var deliveryClient = (DeliveryClient) _deliveryClientBuilder
                .WithProjectId(Guid)
                .WithCodeFirstModelProvider(mockCodeFirstModelProvider)
                .WithCodeFirstPropertyMapper(mockCodeFirstPropertyMapper)
                .WithResiliencePolicyProvider(mockResiliencePolicyProvider)
                .Build();

            Assert.True(deliveryClient.CodeFirstModelProvider != null);
            Assert.True(deliveryClient.CodeFirstPropertyMapper != null);
            Assert.True(deliveryClient.ResiliencePolicyProvider != null);
        }
    }
}
