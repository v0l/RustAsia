using RustAsia.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace RustAsia.Tests
{
    public class CoreTests
    {
        private readonly ITestOutputHelper output;

        public CoreTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task TestInit()
        {
            await Core.Init(output.WriteLine);
        }
    }
}
