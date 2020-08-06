using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using Xunit.MySql.Extensions;
using Xunit.MySql.Tests.Fixtures;
using Xunit.MySql.Tests.Models;

namespace Xunit.MySql.Tests.Queries
{
    public class RawQueryTestV5 : IClassFixture<RawDbFixtureV5>
    {
        private readonly RawDbFixtureV5 _fixture;

        public RawQueryTestV5(RawDbFixtureV5 fixture) => _fixture = fixture;

        [Fact]
        public void TestAllElementsDynamic()
        {
            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"UPPER(m.{nameof(TestModel.Description)}) ");
            sb.Append("FROM TestModels m ");
            sb.Append($"WHERE m.Created >= '{DateTime.Now.AddDays(-2):yyyy-MM-dd HH:mm:ss.ffff}';");

            string query = sb.ToString();

            var elements = ((DbContext)_fixture.Context).GetEntity<object>(query).Select(x => (string)x);
            var list = elements.ToList();

            Assert.Equal(list.Count, _fixture.Number);
        }

        [Fact]
        public void TestAllElements()
        {
            var sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append($"{nameof(TestModel.Description)}, ");
            sb.Append($"{nameof(TestModel.Id)}, ");
            sb.Append($"{nameof(TestModel.Created)} ");
            sb.Append("FROM TestModels ");
            sb.Append($"WHERE Created >= '{DateTime.Now.AddDays(-2):yyyy-MM-dd HH:mm:ss.ffff}';");

            string query = sb.ToString();

            var list = _fixture.Context.TestModels.FromSqlRaw(query).ToList();

            Assert.Equal(list.Count, _fixture.Number);
        }
    }
}