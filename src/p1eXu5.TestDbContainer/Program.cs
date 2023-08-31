using p1eXu5.TestDbContainer;
using TestDbContainer;

await DbContext.Instance.InitAsync();
await new TestDbContainerBootstrap().RunAsync(args);