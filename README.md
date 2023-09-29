TestDbContainer
===============

## Usage

`test-db [Options]`

### As PostBuild task

```xml
<Target Name="PostBuild" AfterTargets="PostBuildEvent" Condition="'$(Configuration)' == 'Debug'">
    <Exec Command="test-db -m $(SolutionDir)\src\DrugRoom.CoreDomain.Adapters.Persistence\Migrations -p $(SolutionDir)\src\DrugRoom.CoreDomain.Adapters.Persistence -s $(SolutionDir)\src\DrugRoom.CoreDomain.WebApi -c test-core-domain-db -e 3367 -n drug_room --verbose" />
  </Target>
```

## Options

| option                | is required | description                             |
| --------------------- |:-----------:| --------------------------------------- |
| -m, --migrations      | `true`      | Migrations folder path                  |
| -p, --project         | `true`      | The project to use.                     |
| -s, --startup-project | true        | The startup project to use              |
| -c, --container-name  | true        | Database docker container name          |
| -e, --external-port   | true        | Database docker container external port |
| -n, --db-name         | true        | Database name                           |
| --verbose             | false       | Verbose log level                       |