using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Opti.Starter.Web.Integration.Tests.Utilities;

[CollectionDefinition(nameof(IntegrationTestCollectionDefinition))]
public class IntegrationTestCollectionDefinition : ICollectionFixture<DatabaseFixture>,
    IClassFixture<WebApplicationFactory<Program>>
{
}
