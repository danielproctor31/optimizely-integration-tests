using System.Net;
using System.Net.Sockets;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Data.SqlClient;

namespace Opti.Starter.Web.Integration.Tests.Utilities;

public class DatabaseFixture : IDisposable
{
    private const string DbPassword = "optiR0cks";
    private const string DbUser = "sa";
    private const string DbImage = "mcr.microsoft.com/azure-sql-edge";
    private const string DbImageTag = "latest";
    private const string DbContainerName = "opti-integration-test-db";
    private const string DbVolumeName = "opti-integration-test-db-volume";
    private const string CmsDbName = "opticms";
    private const string CommerceDbName = "opticommerce";

    private static string _port = "1433";

    private readonly DockerClient _dockerClient = GetDockerClient();

    public DatabaseFixture()
    {
        Task.Run(Teardown).Wait();
        Task.Run(SpinUp).Wait();
    }

    public void Dispose()
    {
        Task.Run(Teardown).Wait();
        GC.SuppressFinalize(this);
    }

    public static string GetCmsSqlConnectionString()
    {
        return $"Data Source=localhost,{_port};" +
               $"Initial Catalog={CmsDbName};" +
               "Integrated Security=False;" +
               $"User ID={DbUser};" +
               $"Password={DbPassword};" +
               "Trusted_Connection=False;Encrypt=False;" +
               "Connection Timeout=30";
    }

    public static string GetCommerceSqlConnectionString()
    {
        return $"Data Source=localhost,{_port};" +
               $"Initial Catalog={CommerceDbName};" +
               "Integrated Security=False;" +
               $"User ID={DbUser};" +
               $"Password={DbPassword};" +
               "Trusted_Connection=False;Encrypt=False;" +
               "Connection Timeout=30";
    }

    private async Task SpinUp()
    {
        try
        {
            _port = GetFreePort();
            await CreateVolume();
            await CreateContainer();
            await StartContainer();
            await CreateDatabases();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error creating integration test database: " + e.Message);
            throw;
        }
    }

    private async Task StartContainer()
    {
        var existingContainer = await GetContainer();

        await _dockerClient
            .Containers
            .StartContainerAsync(existingContainer!.ID, new ContainerStartParameters());
    }

    private async Task<ContainerListResponse?> GetContainer()
    {
        var containerList = await _dockerClient
            .Containers.ListContainersAsync(new ContainersListParameters { All = true });
        var existingContainer = containerList.FirstOrDefault(c => c.Names.Any(n => n.Contains(DbContainerName)));

        if (existingContainer == null)
        {
            throw new Exception($"Container not found with name {DbContainerName}");
        }

        return existingContainer;
    }

    private async Task CreateContainer()
    {
        await _dockerClient
            .Containers
            .CreateContainerAsync(new CreateContainerParameters
            {
                Name = DbContainerName,
                Image = $"{DbImage}:{DbImageTag}",
                Env = new List<string> { "ACCEPT_EULA=Y", $"SA_PASSWORD={DbPassword}" },
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        { "1433/tcp", new PortBinding[] { new() { HostPort = _port } } }
                    },
                    Binds = new List<string> { $"{DbVolumeName}:/var/opt/mssql" }
                }
            });
    }

    private async Task CreateVolume()
    {
        var volumeList = await _dockerClient.Volumes.ListAsync();
        var volumeCount = volumeList.Volumes.Count(v => v.Name == DbVolumeName);
        if (volumeCount <= 0)
        {
            await _dockerClient.Volumes.CreateAsync(new VolumesCreateParameters { Name = DbVolumeName });
        }
    }

    private async Task Teardown()
    {
        try
        {
            var container = await GetContainer();

            await _dockerClient.Containers
                .StopContainerAsync(container!.ID, new ContainerStopParameters());

            await _dockerClient.Containers
                .RemoveContainerAsync(container.ID, new ContainerRemoveParameters());

            await _dockerClient.Volumes.RemoveAsync(DbVolumeName);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error tearing down integration test database: " + e.Message);
        }
    }

    private static DockerClient GetDockerClient()
    {
        var dockerUri = Environment.OSVersion.Platform == PlatformID.Win32NT
            ? "npipe://./pipe/docker_engine"
            : "unix:///var/run/docker.sock";
        return new DockerClientConfiguration(new Uri(dockerUri))
            .CreateClient();
    }

    private static async Task CreateDatabases()
    {
        var start = DateTime.UtcNow;
        const int maxWaitTimeSeconds = 60;
        var connectionEstablished = false;
        while (!connectionEstablished && start.AddSeconds(maxWaitTimeSeconds) > DateTime.UtcNow)
        {
            try
            {
                var sqlConnectionString = $"Data Source=localhost,{_port};" +
                                          "Integrated Security=False;" +
                                          $"User ID={DbUser};" +
                                          $"Password={DbPassword};" +
                                          "Trusted_Connection=False;Encrypt=False;" +
                                          "Connection Timeout=30";

                await using var sqlConnection = new SqlConnection(sqlConnectionString);
                await sqlConnection.OpenAsync();

                var command = sqlConnection.CreateCommand();
                command.CommandText =
                    $@"IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{CmsDbName}')
                        BEGIN
                            CREATE DATABASE [{CmsDbName}]
                        END
                    IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{CommerceDbName}')
                        BEGIN
                            CREATE DATABASE [{CommerceDbName}]
                        END";
                command.ExecuteNonQuery();

                connectionEstablished = true;
            }
            catch
            {
                await Task.Delay(500);
            }
        }

        if (!connectionEstablished)
        {
            throw new Exception(
                $"Connection to the database could not be established within {maxWaitTimeSeconds} seconds.");
        }
    }

    private static string GetFreePort()
    {
        var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();
        var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();
        return port.ToString();
    }
}
