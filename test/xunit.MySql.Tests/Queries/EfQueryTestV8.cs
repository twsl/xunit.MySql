using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.MySql.Tests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Xunit.MySql.Tests.Queries
{
    public class EfQueryTestV8 : IClassFixture<EfDbFixtureV8>
    {
        private readonly EfDbFixtureV8 _fixture;
        private readonly ITestOutputHelper _output;

        public EfQueryTestV8(EfDbFixtureV8 fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public void TestAllElements()
        {
            var elements = from m in _fixture.Context.TestModels
                           where m.Created >= DateTime.Now.AddDays(-2)
                           let upper = m.Description.ToUpper()
                           select upper;
            var list = elements.ToList();

            Assert.Equal(list.Count, _fixture.Number);
            _output.WriteLine($"Elements: {list.Count}");
        }
    }
}