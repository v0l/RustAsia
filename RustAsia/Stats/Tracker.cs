/*using ProtoBuf;
using RustAsia.System;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RustAsia.Stats
{
    public static class Tracker
    {
        private static CancellationToken IsRunningToken { get; set; }
        private static Task StatsWorker { get; set; }
        private static BufferBlock<BaseStat> StatsQueue { get; set; }

        public static void Start()
        {
            IsRunningToken = new CancellationToken();
            StatsQueue = new BufferBlock<BaseStat>();

            StatsWorker = SendStats();
        }

        private static async Task SendStats()
        {
            while (!IsRunningToken.IsCancellationRequested)
            {
                var st = await StatsQueue.ReceiveAsync();
                if (st != null)
                {
                    try
                    {
                        using (var ms = new MemoryStream())
                        {
                            Serializer.Serialize(ms, st);
                            await Core.RedisPublisher.Publish("rust-stats", ms.ToArray());
                        }
                    }
                    catch (Exception ex)
                    {
                        Core.Log($"Failed to send stat {st.GetType().ToString()}, {ex.ToString()}");
                    }
                }
            }
        }

    }
}
*/