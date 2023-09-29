using p1eXu5.TestDbContainer.Models;
using p1eXu5.TestDbContainer.Options;

namespace p1eXu5.TestDbContainer.Interfaces;

internal interface IDotnetCli
{
    int CreateInitialMigration(TestDbOptionsBase testDb);

    int UpdateDatabase(TestDbOptionsBase testDb, Func<LocalIP, string> connectionString );

    int Compose(ComposeVerb composeVerb);
}