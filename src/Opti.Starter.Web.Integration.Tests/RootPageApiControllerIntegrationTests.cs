using Microsoft.AspNetCore.Mvc.Testing;
using Opti.Starter.Web.Integration.Tests.Utilities;
using Xunit;

namespace Opti.Starter.Web.Integration.Tests;

public class RootPageApiControllerIntegrationTests : IntegrationTest
{
    public RootPageApiControllerIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Test()
    {
        var response = await Client.GetAsync("/api/v1/rootPage");
        response.EnsureSuccessStatusCode();
    }
}
