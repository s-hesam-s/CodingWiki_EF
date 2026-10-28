using System.Net;
using CodingWiki_DataAccess.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class PreviewHttpTests : IAsyncLifetime
{
    private WebApplication app = null!;
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        var root = PreviewSetup.Root("CodingWiki_Web");
        var web = typeof(CodingWiki_Web.Controllers.HomeController).Assembly;
        var builder = PreviewSetup.Builder(root, 0, Path.Combine(root, "CodingWiki_Web", "wwwroot"), web.GetName().Name);
        builder.WebHost.UseTestServer();
        builder.Services.AddControllersWithViews().AddApplicationPart(web)
            .ConfigureApplicationPartManager(manager => manager.FeatureProviders.Add(new PreviewControllers()));
        Assert.DoesNotContain(builder.Services, service => service.ServiceType == typeof(ApplicationDbContext));
        app = builder.Build();
        PreviewSetup.Guard(app, true);
        app.MapControllerRoute("preview", "{controller=Preview}/{action=Index}");
        await app.StartAsync();
        client = app.GetTestClient();
    }

    [Fact]
    public async Task BookListRendersActualViewWithPublisherAndAuthors()
    {
        var response = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("EF relationship sample 1", html);
        Assert.Contains("Fixture publisher", html);
        Assert.Contains("Sample Writer, Second Writer", html);
        Assert.Contains("READ-ONLY FIXTURE", html);
        Assert.Equal("no-store", response.Headers.CacheControl!.ToString());
    }

    [Fact]
    public async Task ValidDetailsRenderTheirBoundFixtureValues()
    {
        var response = await client.GetAsync("/Preview/Details?id=1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("EF relationship sample 1", html);
        Assert.Contains("value=\"320\"", html);
        Assert.Contains("Sample 450 g", html);
    }

    [Fact]
    public async Task UnknownBookIsNotFound()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/Preview/Details?id=999")).StatusCode);
    }

    [Theory]
    [InlineData("/Book/Delete?id=1")]
    [InlineData("/Book/Playground")]
    [InlineData("/Book/ManageAuthors?id=1")]
    public async Task OriginalMutatingAndQueryPlaygroundRoutesAreNotRegistered(string path)
    {
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(path)).StatusCode);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task FixtureRejectsAllWriteMethodsBeforeRouting(string method)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), "/Preview/Details?id=1");
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.SendAsync(request)).StatusCode);
    }

    [Fact]
    public async Task PreviewInfoDeclaresItsRuntimeBoundaries()
    {
        var response = await client.GetAsync("/preview-info");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"database\":false", json);
        Assert.Contains("\"externalProviders\":false", json);
        Assert.Contains("\"originalHost\":false", json);
    }

    public async Task DisposeAsync()
    {
        client?.Dispose();
        if (app is not null) await app.DisposeAsync();
    }
}
