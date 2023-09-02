using p1eXu5.TestDbContainer;
using TestDbContainer;

await DbContext.Instance.InitAsync().ConfigureAwait(false);
await new TestDbContainerBootstrap().RunAsync(args).ConfigureAwait(false);