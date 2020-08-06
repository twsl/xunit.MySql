using System;
using System.Collections.Generic;
using System.Text;
using Xunit.MySql.Services;
using Xunit.MySql.Versions;

namespace Xunit.MySql.Tests.Fixtures
{
    public class EfDbFixtureV5: BaseQueryFixture<MySqlServiceV5<Version_5_7_24>>
    {
    }
}
