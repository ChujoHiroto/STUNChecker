using LumiSoft.Net.STUN.Client;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace STUNChecker
{
    class Program
    {
        // Google
        const string GoogleStunAddress = "stun.l.google.com";
        const int GoogleStunPort = 19302;
        
        //  NTT 多分Skyway以外で使うのは駄目そう？ 中国未検証
        const string NTTStunAddress = "stun.webrtc.ecl.ntt.com";
        const int NTTStunPort = 3478;

        // Twilio
        const string TwilioStunAddress = "global.stun.twilio.com";
        const int TwilioStunPort = 3478;

        // Any
        const string AnyStunAddress = "stun.xten.com";
        const int AnyStunPort = 3478;

        static async Task Main(string[] args)
        {
            object syncObject = new object();

            Console.WriteLine("STUN Checker Start!");

            var udp = new UdpClient(new IPEndPoint(IPAddress.Any, 8787));

            var task1 = Task.Run(() =>
            {
                lock (syncObject)
                {
                    StartStun(GoogleStunAddress, GoogleStunPort, udp);
                }
                Console.WriteLine("STUN Google OK!");
            });

            var task2 = Task.Run(() =>
            {
                lock (syncObject)
                {
                    StartStun(NTTStunAddress, NTTStunPort, udp);
                }
                Console.WriteLine("STUN NTT OK!");
            });

            var task3 = Task.Run(() =>
            {
                /*lock (syncObject)
                {
                    StartStun(TwilioStunAddress, TwilioStunPort, udp);
                }*/
                Console.WriteLine("STUN Twilio OK!");
            });

            await Task.WhenAll(task1, task2, task3);

            Console.WriteLine("Press Any Key...");
            Console.ReadLine();
        }

        private static STUN_Result StartStun(string stunaddress, int stunport, UdpClient udp = null)
        {
            if (udp == null)
            {
                udp = new UdpClient(new IPEndPoint(IPAddress.Any, 8787));
            }

            // GoogleのSTUNサーバーに問い合わせる
            Console.WriteLine("Connecting... "  + stunaddress + ":" + stunport);

            STUN_Result result = STUN_Client.Query(stunaddress, stunport, udp);

            Console.WriteLine("[STUN] EndPoint:" + result.PublicEndPoint.ToString());
            Console.WriteLine("[STUN] NetType:" + result.NetType.ToString());

            if (result.NetType == STUN_NetType.UdpBlocked)
            {
                // UDPがブロックされる
                Console.WriteLine("[STUN] UDP is always blocked.");
            }
            else if (result.NetType == STUN_NetType.SymmetricUdpFirewall)
            {
                Console.WriteLine("[STUN] SymmetricUdpFirewall");
            }
            else if (result.NetType == STUN_NetType.Symmetric)
            {
                Console.WriteLine("[STUN] Symmetric NAT");
            }
            else if (result.NetType == STUN_NetType.PortRestrictedCone)
            {
                // 相手がシンメトリックだった場合に通信できないので、ホスト対応から外す
                Console.WriteLine("[STUN] PortRestrictedCone");
            }
            else if (result.NetType == STUN_NetType.RestrictedCone)
            {
                // Clientのエフェメラルポートがわかれば対応できるが、netcodeの実装上使うポートが不明なので一旦非対応
                Console.WriteLine("[STUN] RestrictedCone");
            } else
            {
                Console.WriteLine("[STUN] Host Ready");
            }

            // 上記以外のNATはUDPホールパンチング対応

            return result;
        }

    }
}
