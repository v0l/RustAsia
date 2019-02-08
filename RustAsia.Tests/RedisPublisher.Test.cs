using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RustAsia.Tests
{
    public class RedisPublisherTests
    {
        [Fact]
        public async Task TestSimple()
        {
            using (var rp = await RedisPublisher.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 6379)))
            {
                for (var x = 0; x < 10; x++)
                {
                    await rp.Publish("test", Encoding.UTF8.GetBytes("test"));
                }
            }
        }

        [Fact]
        public async Task TestParallelCalls()
        {
            using (var rp = await RedisPublisher.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 6379)))
            {
                var tl = new List<Task<int>>();

                for (var x = 0; x < 10; x++)
                {
                    tl.Add(rp.Publish("test", Encoding.UTF8.GetBytes("test")));
                }

                await Task.WhenAll(tl);
            }
        }
    }
}
