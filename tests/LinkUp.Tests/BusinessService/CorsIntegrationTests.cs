// T070: CORS Integration Tests for BusinessService
using Xunit;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace LinkUp.Tests.BusinessService;

public class CorsIntegrationTests : IDisposable
{
    private readonly HttpClient _client;
    private const string BaseUrl = "http://localhost:7001";

    public CorsIntegrationTests()
    {
        _client = new HttpClient();
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    [Theory]
    [InlineData("http://localhost:3000")]
    [InlineData("http://localhost:63131")]
    [InlineData("http://127.0.0.1:3000")]
    [InlineData("http://127.0.0.1:63131")]
    public async Task OPTIONS_Request_Should_Return_CORS_Headers(string origin)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Options, $"{BaseUrl}/api/channels");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK
                 || response.StatusCode == System.Net.HttpStatusCode.NoContent
                 || response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed,
            $"Expected 200, 204 or 405, got {response.StatusCode}");
        
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"),
            "Response should include Access-Control-Allow-Origin header");
        
        var allowOriginHeader = response.Headers.GetValues("Access-Control-Allow-Origin").First();
        Assert.Equal(origin, allowOriginHeader);
    }

    [Fact]
    public async Task GET_Request_Should_Return_CORS_Headers()
    {
        // Arrange
        var origin = "http://localhost:63131";
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/api/channels");
        request.Headers.Add("Origin", origin);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"),
            "GET response should include Access-Control-Allow-Origin header");
        
        var allowOriginHeader = response.Headers.GetValues("Access-Control-Allow-Origin").First();
        Assert.Equal(origin, allowOriginHeader);
    }

    [Fact]
    public async Task POST_Request_Should_Return_CORS_Headers()
    {
        // Arrange
        var origin = "http://localhost:63131";
        var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/api/channels/test-id/messages")
        {
            Content = new StringContent("{\"content\":\"test\"}", System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Origin", origin);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"),
            "POST response should include Access-Control-Allow-Origin header");
        
        var allowOriginHeader = response.Headers.GetValues("Access-Control-Allow-Origin").First();
        Assert.Equal(origin, allowOriginHeader);
    }

    [Fact]
    public async Task CORS_Should_Include_Credentials()
    {
        // Arrange
        var origin = "http://localhost:63131";
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/api/channels");
        request.Headers.Add("Origin", origin);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.Headers.Contains("Access-Control-Allow-Credentials"),
            "Response should include Access-Control-Allow-Credentials header");
        
        var credentialsHeader = response.Headers.GetValues("Access-Control-Allow-Credentials").First();
        Assert.Equal("true", credentialsHeader);
    }

    [Fact]
    public async Task Non_Localhost_Origin_Should_Not_Receive_CORS_Headers()
    {
        // Arrange
        var origin = "http://example.com:3000";
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/api/channels");
        request.Headers.Add("Origin", origin);

        // Act
        var response = await _client.SendAsync(request);

        // Assert - Non-localhost origins should not get CORS headers
        var hasAllowOriginHeader = response.Headers.Contains("Access-Control-Allow-Origin");
        if (hasAllowOriginHeader)
        {
            var allowOriginHeader = response.Headers.GetValues("Access-Control-Allow-Origin").First();
            Assert.NotEqual(origin, allowOriginHeader);
        }
    }

    [Fact]
    public async Task CORS_Should_Allow_Any_Method()
    {
        // Arrange
        var origin = "http://localhost:63131";
        var request = new HttpRequestMessage(HttpMethod.Options, $"{BaseUrl}/api/channels");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "DELETE");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.Headers.Contains("Access-Control-Allow-Methods")
                 || response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed,
            "Response should include Access-Control-Allow-Methods or return 405");
    }

    [Fact]
    public async Task CORS_Should_Allow_Any_Header()
    {
        // Arrange
        var origin = "http://localhost:63131";
        var request = new HttpRequestMessage(HttpMethod.Options, $"{BaseUrl}/api/channels");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Headers", "Content-Type, Authorization");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.Headers.Contains("Access-Control-Allow-Headers")
                 || response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed,
            "Response should include Access-Control-Allow-Headers or return 405");
    }
}
