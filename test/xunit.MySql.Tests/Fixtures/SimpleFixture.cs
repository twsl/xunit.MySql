using Bogus;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit.MySql.Services;
using Xunit.MySql.Tests.Infrastructure;
using Xunit.MySql.Tests.Models;
using Xunit.MySql.Versions;

namespace Xunit.MySql.Tests.Fixtures
{
    public class SimpleFixture : DatabaseFixture<TestDbContext, MySqlServiceV8<Version_8_0_12>>
    {
        public int Number => 1;

        public SimpleFixture() => SeedDatabase();

        private void SeedDatabase()
        {
            var ctx = Context;
            var faker = new Faker<TestModel>()
                .RuleFor(d => d.Id, (f, c) => (uint)0) // Keep zero, assigned after adding to database
                .RuleFor(m => m.Created, (f, m) => f.Date.Between(DateTime.Now.AddDays(-1), DateTime.Now))
                .RuleFor(c => c.Description, f => f.Lorem.Slug());

            var elements = faker.Generate(Number);
            ctx.TestModels.AddRange(elements);
            ctx.SaveChanges();
        }
    }
}
