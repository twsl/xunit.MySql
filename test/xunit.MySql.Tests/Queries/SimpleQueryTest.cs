using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit.Abstractions;
using Xunit.MySql.Tests.Fixtures;

namespace Xunit.MySql.Tests.Queries
{
    public class SimpleQueryTest : IClassFixture<SimpleFixture>
    {
        private readonly SimpleFixture _fixture;
        private readonly ITestOutputHelper _output;

        public SimpleQueryTest(SimpleFixture fixture, ITestOutputHelper output)
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
