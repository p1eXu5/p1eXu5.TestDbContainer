using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p1eXu5.TestDbContainer.Tests.Integration;

public sealed class ContainerVerbTests
{
    [Test]
    [Ignore("Leave hear as entry point")]
    public void FulfilledVerbOptions_ExecuteSucceeded()
    {
        // Arrange:
        string[] args =
        {
            "container",
            "-m", "foo",
            "-p", "bar",
            "-s", "baz",
            "-c", "qux",
            "-e", "quux",
            "-n", "corge",

        };

        // Action:


        // Assert:
    }
}
