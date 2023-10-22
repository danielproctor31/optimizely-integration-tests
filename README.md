# optimizely-integration-tests

## Prerequisites
- Docker (Needs to be up and running)

This test project has been setup to allow for integration tests to easily be created. When tests are ran it will perform the following tasks:
- Start a docker container with a sql database, it will automatically find a random open port - no need to stop your existing containers!
- Set the environment variables for the CMS to use the correct connection string and environment.
- Start the CMS and run all selected integration tests against it.
- Stop the CMS and clean up the database docker container and volume.

## Setup
To setup a integration test class it will need the following:
- Inherit from IntegrationTestBase
- Pass the WebApplicationFactory<Program> to the base constructor
- A HttpClient will be exposed from the base class as Client. This can be used to make calls to the api.

Example:

```
using Opti.Starter.Web.Integration.Tests.Utilities;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Opti.Starter.Web.Integration.Tests;

public class SiteInitializationIntegrationTests : IntegrationTest
{
    public SiteInitializationIntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task GivenSiteHasBeenInitialized_WhenLoginPageIsRequested_ReturnsOk()
    {
        var response = await Client.GetAsync("/Util/Login");
        response.EnsureSuccessStatusCode();
    }
}
```

Inheriting from IntegrationTest will add the class to theIntegrationTestCollectionDefinition collection. This will ensure that the tests are ran synchronously and the Database and CMS are only started once at the beginning of all tests and stopped at the end of all tests.
