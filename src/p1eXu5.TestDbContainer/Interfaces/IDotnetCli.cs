using TestDbContainer;

namespace p1eXu5.TestDbContainer.Interfaces;

internal interface IDotnetCli
{
    int UpdateDatabase(TestDbOptions testDb);
}