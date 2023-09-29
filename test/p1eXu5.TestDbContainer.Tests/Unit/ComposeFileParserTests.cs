using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using p1eXu5.TestDbContainer.Models;

namespace p1eXu5.TestDbContainer.Tests.Unit;

public sealed class ComposeFileParserTests
{
    private ComposeFileParser _composeFileParser = default!;

    [SetUp]
    public void Initialize()
    {
        _composeFileParser = new ComposeFileParser();
    }

    [Test]
    public void Parse_YamlComposeFile_Success()
    {
        // Arrange:
        ComposeFilePath composeFilePath = new("test-compose.yaml");

        var path =
           Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, composeFilePath.Path);

        using var sw = File.CreateText(path);
        sw.Write(@"
name: drug_room

services:
  identity_db:
    image: mysql:8.0.33
    environment:
      MYSQL_ROOT_PASSWORD: admin
      MYSQL_DATABASE: db_name
      MYSQL_USER: admin
      MYSQL_PASSWORD: admin
    ports:
      - ""3377:3306""
    restart: unless-stopped
    healthcheck:
      test: [""CMD"", ""mysqladmin"" ,""ping"", ""-h"", ""localhost""]
      timeout: 5s
      retries: 10
    networks:
      - backend

# https://docs.docker.com/compose/networking/#configure-the-default-network
networks:
  backend:
    driver: bridge".Trim());

        sw.Flush();
        sw.Close();

        // Action:
        ComposeFile composeFile = _composeFileParser.Parse(composeFilePath);

        // Assert:
        composeFile.Name.Should().Be("drug_room");
        composeFile.Services.Should().NotBeEmpty();
        composeFile.Services.Should().ContainKey("identity_db");
        composeFile.Services["identity_db"].Environment.Should().NotBeEmpty();
        composeFile.Services["identity_db"].Environment.Should().ContainKey("MYSQL_DATABASE");
        composeFile.Services["identity_db"].Environment["MYSQL_DATABASE"].Should().Be("db_name");
        composeFile.Services["identity_db"].Ports.Should().NotBeEmpty();
        composeFile.Services["identity_db"].Ports.Should().Contain("3377:3306");
    }
}
