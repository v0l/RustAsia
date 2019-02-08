using RustAsia.Stats;
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace RustAsia.System
{
    public static class Core
    {
        public static readonly string Name = $"RustAsia v{Assembly.GetExecutingAssembly().GetName().Version} (Rust v{Rust.Protocol.printable})";

        public static async Task Init(Action<string> logFunction)
        {
            Log = logFunction;

            try
            {
                Log($"Starting {Name}");
                RedisPublisher = await RedisPublisher.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 6379));
                Tracker.Start();
                Admin.Commands.InsertCommands();
            }
            catch (Exception ex)
            {
                Log($"{Name} failed to start: {ex.ToString()}");
            }
            finally { InitSuccess = true; }
        }

        public static RedisPublisher RedisPublisher { get; private set; }
        public static bool InitSuccess { get; private set; }
        public static Action<string> Log { get; private set; }
    }
}
