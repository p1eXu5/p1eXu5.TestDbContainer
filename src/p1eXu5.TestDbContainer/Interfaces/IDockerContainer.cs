using Docker.DotNet.Models;
using TestDbContainer;

namespace p1eXu5.TestDbContainer.Interfaces;
internal interface IDockerContainer
{
    Task<CreateContainerResponse> CreateContainerAsync(TestDbOptions testDb, CancellationToken cancellationToken);
    Task<ContainerListResponse?> FindAsync(string name, CancellationToken cancellationToken);
    bool IsRunning(ContainerListResponse containerListResponse);
    Task RemoveContainerAsync(ContainerListResponse containerListResponse, CancellationToken cancellationToken);
    Task StartContainerAsync(CreateContainerResponse createContainerResponse, CancellationToken cancellationToken);
    Task StartContainerAsync(ContainerListResponse createListResponse, CancellationToken cancellationToken);
}