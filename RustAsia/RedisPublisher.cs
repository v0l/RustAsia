using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RustAsia
{
    public class RedisPublisher : IDisposable
    {
        private Socket Sock { get; set; }
        private NetworkStream Stream { get; set; }
        private StreamReader Reader { get; set; }
        private SemaphoreSlim Lock { get; set; } = new SemaphoreSlim(1);

        public static async Task<RedisPublisher> ConnectAsync(IPEndPoint ip)
        {
            var ret = new RedisPublisher();
            await ret.ConnectInternalAsync(ip);
            return ret;
        }

        public RedisPublisher()
        {
            Sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        private async Task ConnectInternalAsync(IPEndPoint ip)
        {
            if (Sock.Connected)
            {
                Sock.Disconnect(true);
                Stream = null;
                Reader = null;
            }

#if NETCOREAPP
            await Sock.ConnectAsync(ip);
#else
            var tcs = new TaskCompletionSource<bool>();
            var sae = new SocketAsyncEventArgs();
            sae.RemoteEndPoint = ip;
            sae.Completed += (s, e) =>
            {
                tcs.SetResult(true);
            };

            if (Sock.ConnectAsync(sae))
            {
                await tcs.Task;
            }
#endif
            Stream = new NetworkStream(Sock);
            Reader = new StreamReader(Stream, Encoding.UTF8, false, 1024, true);
        }

        public async Task<int> Publish(string key, byte[] value)
        {
            await Lock.WaitAsync();

            try
            {
                var data = Encoding.UTF8.GetBytes($"*3\r\n$7\r\nPUBLISH\r\n${key.Length}\r\n{key}\r\n${value.Length}\r\n");
                await Stream.WriteAsync(data, 0, data.Length);
                await Stream.WriteAsync(value, 0, value.Length);
                await Stream.WriteAsync(new byte[] { 13, 10 }, 0, 2);
                await Stream.FlushAsync();

                var rsp = await Reader.ReadLineAsync();
                if (!string.IsNullOrEmpty(rsp) && rsp.Length > 1)
                {
                    if (int.TryParse(rsp.Substring(1), out int subs))
                    {
                        Lock.Release();
                        return subs;
                    }
                    else
                    {
                        throw new Exception($"Invalid response: \"{rsp}\"");
                    }
                }
                else
                {
                    throw new Exception("Unknown error??");
                }
            }
            catch(Exception ex)
            {
                Lock.Release();
                throw ex;
            }
        }

        public void Dispose()
        {
            Reader.Dispose();
            Stream.Dispose();
            Sock.Close();
            Sock.Dispose();
            Lock.Dispose();
        }
    }
}
