using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Opti.Starter.Web.Integration.Tests.Utilities;

[Collection(nameof(IntegrationTestCollectionDefinition))]
public abstract class IntegrationTest
{
    protected readonly HttpClient Client;

    protected IntegrationTest(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("CONNECTIONSTRINGS__EPISERVERDB",
            DatabaseFixture.GetCmsSqlConnectionString());
        Environment.SetEnvironmentVariable("CONNECTIONSTRINGS__ECFSQLCONNECTION",
            DatabaseFixture.GetCommerceSqlConnectionString());

        Client = factory.CreateClient();
    }
}
