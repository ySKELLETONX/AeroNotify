using PacketDotNet;
using SharpPcap;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace AeroNotify.IVAO
{
    public class IvaoChatSniffer
    {
        private static ICaptureDevice? device;

        public static void Initialize()
        {
            try
            {
                var devices = CaptureDeviceList.Instance;

                if (devices.Count < 1)
                {
                    Console.WriteLine("Nenhuma interface de rede encontrada.");
                    return;
                }

                var activeNic = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic =>
                        nic.OperationalStatus == OperationalStatus.Up &&
                        nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        nic.GetIPProperties().GatewayAddresses.Any() &&
                        !nic.Description.Contains("VPN", StringComparison.OrdinalIgnoreCase) &&
                        !nic.Description.Contains("Virtual", StringComparison.OrdinalIgnoreCase) &&
                        !nic.Description.Contains("Hamachi", StringComparison.OrdinalIgnoreCase) &&
                        !nic.Description.Contains("VMware", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    .ThenByDescending(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    .FirstOrDefault();

                if (activeNic == null)
                {
                    Console.WriteLine("Nenhuma interface Ethernet/Wi-Fi ativa encontrada.");
                    return;
                }

                Console.WriteLine($"Interface de rede ativa detectada: {activeNic.Description}");

                device = devices.FirstOrDefault(d => d.Name.Contains(activeNic.Id));

                if (device == null)
                {
                    Console.WriteLine("Não foi possível casar a interface ativa com o SharpPcap.");
                    return;
                }

                device.OnPacketArrival += Device_OnPacketArrival;
                device.Open();
                device.Filter = "tcp port 6809";

                Console.WriteLine($"Capturando pacotes na interface: {device.Description}...");
                device.StartCapture();
            }catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void Stop()
        {
            if (device != null && device.Started)
            {
                device.StopCapture();
                device.Close();
                Console.WriteLine("Captura encerrada.");
            }
        }

        private static void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            try
            {
                var rawPacket = e.GetPacket();
                var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
                var tcpPacket = packet.Extract<TcpPacket>();

                if (tcpPacket != null &&
                    tcpPacket.PayloadData != null &&
                    tcpPacket.PayloadData.Length > 0)
                {
                    string payload = Encoding.UTF8.GetString(tcpPacket.PayloadData);

                    if (payload.Contains("#TM") || payload.Contains("#TMSERVER:"))
                    {
                        string msg = $"[{DateTime.Now:HH:mm:ss}] {payload.Trim()}";
                        Console.WriteLine($"[CHAT] {msg}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar pacote: {ex.Message}");
            }
        }
    }
}
