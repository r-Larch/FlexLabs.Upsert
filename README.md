
[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/r-Larch/FlexLabs.Upsert/ci.yml)](https://github.com/r-Larch/FlexLabs.Upsert/actions) [![NuGet Version](https://img.shields.io/nuget/v/LarchSys.FlexLabs.Upsert)](https://www.nuget.org/packages/LarchSys.FlexLabs.Upsert)

**Install Nuget**

```bash
> dotnet add package LarchSys.FlexLabs.Upsert --prerelease
```

# This is a fork of `artiomchi/FlexLabs.Upsert`

This fork provides support for **Owned** entities and **Owned JSON** entities.

**Owned Entities**
```c#
using var dbContext = new TestDbContext(_fixture.DataContextOptions);

var newParent = new Parent
{
    ID = 1,
    Child = new Child
    {
        ChildName = "Someone else",
        Age = 10,
        SubChild = new SubChild
        {
            SubChildName = "SubChild foobar",
            Age = 10,
        }
    },
};

dbContext.Parents.Upsert(newParent)
    .On(p => p.ID)
    .WhenMatched((a, b) => new Parent
    {
        Counter = b.Counter + 1,
        Child = new Child
        {
            SubChild = b.Child.SubChild, // nested owned direct mapping - should expand to all columns.
        }
    })
    .Run();
```

**Owned JSON Entities**
```c#
using var dbContext = new TestDbContext(_fixture.DataContextOptions);

var company = new CompanyOwnedJson
{
    Id = 1,
    Name = "Company 1",
    Meta = new CompanyMeta // .OwnsOne(_ => _.Meta, _ => _.ToJson()...)
    {
        Required = "required-value",
        JsonOverride = "col with [JsonPropertyName]",
        Nested = new CompanyNestedMeta
        {
            Title = "I'm a nested json",
        },
        Properties = [
            new CompanyMetaValue {
                Key = "foo",
                Value = "bar",
            },
            new CompanyMetaValue {
                Key = "cat",
                Value = "dog",
            }
        ],
    }
};

dbContext.CompanyOwnedJson.Upsert(company)
    .On(p => p.Id)
    .WhenMatched((a, b) => new CompanyOwnedJson
    {
        Name = b.Name,
        Meta = b.Meta, // assigning a JSON is supported.
    })
    .Run();
```


FlexLabs.Upsert
==========

[![Build status](https://ci.appveyor.com/api/projects/status/a64hu4iyx7r4a3yo?svg=true)](https://ci.appveyor.com/project/artiomchi/flexlabs-upsert)
[![FlexLabs.EntityFrameworkCore.Upsert on NuGet](https://img.shields.io/nuget/v/FlexLabs.EntityFrameworkCore.Upsert.svg)](https://www.nuget.org/packages/FlexLabs.EntityFrameworkCore.Upsert)  
CI build: [![FlexLabs.EntityFrameworkCore.Upsert on MyGet](https://img.shields.io/myget/artiomchi/vpre/FlexLabs.EntityFrameworkCore.Upsert.svg)](https://github.com/artiomchi/FlexLabs.Upsert/wiki/CI-Builds)

This library adds basic support for "Upsert" operations to EF Core.

Uses `INSERT … ON CONFLICT DO UPDATE` in PostgreSQL/Sqlite, `MERGE` in SqlServer & Oracle and `INSERT INTO … ON DUPLICATE KEY UPDATE` in MySQL.

Also supports injecting sql command runners to add support for other providers

A typical upsert command could look something like this:

```csharp
DataContext.DailyVisits
    .Upsert(new DailyVisit
    {
        UserID = userID,
        Date = DateTime.UtcNow.Date,
        Visits = 1,
    })
    .On(v => new { v.UserID, v.Date })
    .WhenMatched(v => new DailyVisit
    {
        Visits = v.Visits + 1,
    })
    .RunAsync();
```

In this case, the upsert command will ensure that a new `DailyVisit` will be added to the database. If a visit with the same `UserID` and `Date` already exists, it will be updated by incrementing it's `Visits` value by 1.

Please read our [Usage](https://github.com/artiomchi/FlexLabs.Upsert/wiki/Usage) page for more examples