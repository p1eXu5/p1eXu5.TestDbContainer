namespace p1eXu5.TestDbContainer.Interfaces;

internal interface IMySqlContainerParameters
{
    int ContainerExternalPort { get; }

    string ContainerName { get; }

    string DatabaseName { get; }
}
