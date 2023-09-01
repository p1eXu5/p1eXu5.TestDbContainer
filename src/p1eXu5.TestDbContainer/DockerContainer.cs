using Docker.DotNet;
using Docker.DotNet.Models;
using p1eXu5.TestDbContainer.Interfaces;

namespace TestDbContainer;

internal sealed class DockerContainer : IDockerContainer
{
    public static DockerClient? _client;

    public static DockerClient Client => _client ??= new DockerClientConfiguration().CreateClient();

    /// <summary>
    /// </summary>
    /// <param name="name">For example: <c>"drug-room-core-dev"</c></param>
    /// <returns></returns>
    public async Task<ContainerListResponse?> FindAsync(string name)
    {
        IList<ContainerListResponse> containers =
            await Client.Containers.ListContainersAsync(
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
                });

        return containers.FirstOrDefault();
    }

    /// <summary>
    /// Creates container without start
    /// </summary>
    /// <param name="testDb"></param>
    /// <returns></returns>
    public async Task<CreateContainerResponse> CreateContainerAsync(TestDbOptions testDb)
    {
        var exposedPorts = new Dictionary<string, EmptyStruct>()
        {
            { "3306", new EmptyStruct() },
            { "33060", new EmptyStruct() },
        };

        var oneHostBinding = new PortBinding()
        {
            HostIP = "0.0.0.0",
            HostPort = testDb.ContainerExternalPort.ToString(),
        };

        var hostBindingsList = new List<PortBinding>() { oneHostBinding };

        var portBindings = new Dictionary<string, IList<PortBinding>>()
        {
            { "3306/tcp", hostBindingsList },
            { "33060/tcp", new List<PortBinding>() },
        };

        return await Client.Containers.CreateContainerAsync(new CreateContainerParameters()
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
                "MYSQL_DATABASE=drug_room",
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
        });
    }

    public async Task StartContainerAsync(CreateContainerResponse createContainerResponse)
    {
        await Client.Containers.StartContainerAsync(
            createContainerResponse.ID,
            new ContainerStartParameters { });
    }

    public async Task StartContainerAsync(ContainerListResponse createListResponse)
    {
        await Client.Containers.StartContainerAsync(
            createListResponse.ID,
            new ContainerStartParameters { });
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
            cancellationToken);
    }

    public async Task StopContainerAsync(ContainerListResponse containerListResponse, CancellationToken cancellationToken)
    {
        if (IsRunning(containerListResponse))
        {
            await Client.Containers.StopContainerAsync(
                containerListResponse.ID,
                new ContainerStopParameters { },
                cancellationToken);
        }
    }
}
