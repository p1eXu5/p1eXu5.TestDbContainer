using Docker.DotNet.Models;
using TestDbContainer;

namespace p1eXu5.TestDbContainer.Interfaces;
internal interface IDockerContainer
{
    Task<CreateContainerResponse> CreateContainerAsync(TestDbOptions testDb);
    Task<ContainerListResponse?> FindAsync(string name);
    bool IsRunning(ContainerListResponse containerListResponse);
    Task RemoveContainerAsync(ContainerListResponse containerListResponse, CancellationToken cancellationToken);
    Task StartContainerAsync(CreateContainerResponse createContainerResponse);
    Task StartContainerAsync(ContainerListResponse createListResponse);
}