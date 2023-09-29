using System.Diagnostics.CodeAnalysis;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using p1eXu5.TestDbContainer.Interfaces;

namespace TestDbContainer;

internal sealed class DockerContainer : IDockerContainer, IDisposable
{
    public DockerClient? _client;
    private bool _disposedValue;
    private readonly ILogger<DockerContainer> _logger;

    public DockerContainer(ILogger<DockerContainer> logger)
    {
        _logger = logger;
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "See Dispose()")]
    public DockerClient Client => _client ??= new DockerClientConfiguration().CreateClient();

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// </summary>
    /// <param name="name">For example: <c>"drug-room-core-dev"</c></param>
    /// <returns></returns>
    public async Task<ContainerListResponse?> FindAsync(string name, CancellationToken cancellationToken)
    {
        try
        {
            IList<ContainerListResponse> containers =
                await Client.Containers
                    .ListContainersAsync(
                        new ContainersListParameters()
                        {
                            // https://docs.docker.com/engine/reference/commandline/ps/#filter
                            Filters = new Dictionary<string, IDictionary<string, bool>>
                            {
                                ["name"] = new Dictionary<string, bool>
                                {
                                    [name] = true // "drug-room-core-dev"
                                }
                            },
                            All = true,
                        },
                        cancellationToken)
                    .ConfigureAwait(false);

            return containers.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Find container error!");
            throw;
        }
    }

    /// <summary>
    /// Creates container without start
    /// </summary>
    /// <param name="testDb"></param>
    /// <returns></returns>
    public async Task<CreateContainerResponse> CreateContainerAsync(IMySqlContainerParameters testDb, CancellationToken cancellationToken)
    {
        var exposedPorts = new Dictionary<string, EmptyStruct>()
        {
            { "3306", new EmptyStruct() },
            { "33060", new EmptyStruct() },
        };

        var oneHostBinding = new PortBinding()
        {
            HostIP = "0.0.0.0",
            HostPort = testDb.ContainerExternalPort.ToString(provider: null),
        };

        var hostBindingsList = new List<PortBinding>() { oneHostBinding };

        var portBindings = new Dictionary<string, IList<PortBinding>>()
        {
            { "3306/tcp", hostBindingsList },
            { "33060/tcp", new List<PortBinding>() },
        };

        return
            await Client.Containers
                .CreateContainerAsync(
                    new CreateContainerParameters()
                    {
                        Image = "mysql:8.0.33",
                        Name = testDb.ContainerName,
                        HostConfig = new HostConfig()
                        {
                            RestartPolicy = new RestartPolicy() { Name = RestartPolicyKind.No },
                            PortBindings = portBindings,
                            NetworkMode = "bridge"
                        },
                        Env = new List<string>()
                        {
                            "ACCEPT_EULA=Y",
                            "MYSQL_ROOT_PASSWORD=admin",
                            $"MYSQL_DATABASE={testDb.DatabaseName}",
                            "MYSQL_USER=admin",
                            "MYSQL_PASSWORD=admin",
                        },
                        // ExposedPorts = exposedPorts, // only inside docker world
                        Healthcheck = new HealthConfig()
                        {
                            Test = new List<string> { "CMD", "mysqladmin", "ping", "-h", "localhost" },
                            Timeout = TimeSpan.FromSeconds(5),
                            Retries = 5,
                        },
                    },
                    cancellationToken)
                .ConfigureAwait(false);
    }

    public async Task StartContainerAsync(CreateContainerResponse createContainerResponse, CancellationToken cancellationToken)
    {
        await Client.Containers
            .StartContainerAsync(
                createContainerResponse.ID,
                new ContainerStartParameters { },
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task StartContainerAsync(ContainerListResponse createListResponse, CancellationToken cancellationToken)
    {
        await Client.Containers
            .StartContainerAsync(
                createListResponse.ID,
                new ContainerStartParameters { },
                cancellationToken)
            .ConfigureAwait(false);
    }

    public bool IsRunning(ContainerListResponse containerListResponse)
    {
        return containerListResponse.State.Equals("running", StringComparison.OrdinalIgnoreCase);
    }

    public async Task RemoveContainerAsync(ContainerListResponse containerListResponse, CancellationToken cancellationToken)
    {
        await Client.Containers.RemoveContainerAsync(
            containerListResponse.ID,
            new ContainerRemoveParameters { Force = true, RemoveVolumes = true, },
            cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task StopContainerAsync(ContainerListResponse containerListResponse, CancellationToken cancellationToken)
    {
        if (IsRunning(containerListResponse))
        {
            await Client.Containers.StopContainerAsync(
                containerListResponse.ID,
                new ContainerStopParameters { },
                cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                _client?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~DockerContainer()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }
}
