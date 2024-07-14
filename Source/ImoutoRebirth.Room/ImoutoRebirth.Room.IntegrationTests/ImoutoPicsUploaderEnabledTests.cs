using Xunit;
using System.Net.Http.Json;
using ImoutoRebirth.Room.IntegrationTests.Fixtures;

namespace ImoutoRebirth.Room.IntegrationTests
{
    [Collection("WebApplication")]
    public class ImoutoPicsUploaderEnabledTests
    {
        private readonly TestWebApplicationFactory<Program> _webApp;

        public ImoutoPicsUploaderEnabledTests(TestWebApplicationFactory<Program> webApp) => _webApp = webApp;

        [Fact(Skip = "Other tests are interfering with this one, can be run separately.")]
        public async Task ImoutoPicsUploaderEnabledByDefault()
        {
            var response = await _webApp.Client.GetFromJsonAsync<bool>("/imouto-pics-uploader-enabled");
            Assert.True(response);
        }

        [Fact]
        public async Task DisableImoutoPicsUploader()
        {
            var response = await _webApp.Client.DeleteAsync("/imouto-pics-uploader-enabled");
            response.EnsureSuccessStatusCode();

            var isEnabled = await _webApp.Client.GetFromJsonAsync<bool>("/imouto-pics-uploader-enabled");
            Assert.False(isEnabled);
        }

        [Fact]
        public async Task EnableImoutoPicsUploader()
        {
            await _webApp.Client.PostAsync("/imouto-pics-uploader-enabled/disable", null);

            var response = await _webApp.Client.PostAsync("/imouto-pics-uploader-enabled", null);
            response.EnsureSuccessStatusCode();

            var isEnabled = await _webApp.Client.GetFromJsonAsync<bool>("/imouto-pics-uploader-enabled");
            Assert.True(isEnabled);
        }
    }
}
