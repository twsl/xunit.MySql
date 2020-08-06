using System;
using System.Collections.Generic;
using System.Text;

namespace Xunit.MySql.Tests.Models
{
    public class TestModel
    {
        public uint Id { get; set; }

        public DateTime Created { get; set; }

        public string Description { get; set; }
    }
}
