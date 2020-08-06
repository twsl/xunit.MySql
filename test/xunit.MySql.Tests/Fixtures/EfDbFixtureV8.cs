using System;
using System.Collections.Generic;
using System.Text;
using Xunit.MySql.Services;
using Xunit.MySql.Versions;

namespace Xunit.MySql.Tests.Fixtures
{
    public class EfDbFixtureV8 : BaseQueryFixture<MySqlServiceV8<Version_8_0_12>>
    {
    }
}
