# xunit.MySql
[![CI](https://github.com/twsl/xunit.MySql/workflows/CI/badge.svg)](https://github.com/twsl/xunit.MySql/actions?query=workflow%3ACI)
[![license](https://img.shields.io/github/license/twsl/xunit.MySql)](LICENSE)

Easy unit testing integration for MySql with support for real queries using the [Entity Framework Core provider for MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql).
It is not the fastest nor the prettiest solution, but it integrates [stumpdk's MySql.Server](https://github.com/stumpdk/MySql.Server) adequately into [xunit](https://github.com/xunit/xunit).
While testing, the library spins up a local MySql instance and cleans up afterwards.

## Disclaimer
This is no official package, I just used the Xunit.* namespace to hightlight, that this is an extension highly depending on that project.
Even though this package targets .NET Standard, it currently only works on Windows due to the supplied MySql binaries.

The usage of [MySql](https://www.mysql.com/) is covered by [The Universal FOSS Exception](https://oss.oracle.com/licenses/universal-foss-exception/).

## License
[License](LICENSE)

## Prepare unit tests
Install the latest version of the ef tool with:

```
dotnet tool install --global dotnet-ef --version 3.1.6
```

Or update an existing version with:

```
dotnet tool update --global dotnet-ef
```

Then execute the following command from the `xunit.MySql.Tests` directory:

```
dotnet ef migrations add Init -v --context TestDbContext
```

## Example

```csharp
public class SimpleFixture : DatabaseFixture<DbContext, MySqlServiceV8<Version_8_0_12>>
{
    public SimpleFixture()
    {
        // Context.ModelName.Add...
        Context.SaveChanges();
    }
}
```

```csharp
public class SimpleQueryTest : IClassFixture<SimpleFixture>
{
    private readonly SimpleFixture _fixture;

    public SimpleQueryTest(SimpleFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void TestAllElements()
    {
        var list = _fixture.Context.ModelName.ToList();
        Assert.NotNull(list);
    }
}
```
