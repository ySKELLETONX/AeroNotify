using PacketDotNet;
using SharpPcap;
using System.Text;

namespace AeroNotify.IVAO
{
    public class IvaoChatSniffer
    {
        private ICaptureDevice _device;

        public void Start()
        {
            var devices = CaptureDeviceList.Instance;

            if (devices.Count == 0)
            {
                Console.WriteLine("[IVAO] Nenhuma interface encontrada.");
                return;
            }

            _device = devices[0];

            Console.WriteLine($"Usando interface: {_device.Description}");

            // Evento compatível com sua versão
            _device.OnPacketArrival += new PacketArrivalEventHandler(OnPacketArrival);

            // Modo compatível com sua versão
            _device.Open(DeviceMode.Promiscuous);

            _device.Filter = "udp port 6809";

            Console.WriteLine("Capturando pacotes UDP 6809...");

            _device.StartCapture();
        }

        private void OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var rawPacket = e.Packet;

            var packet = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

            var udp = packet.Extract<UdpPacket>();
            if (udp == null) return;

            var payload = udp.PayloadData;
            if (payload == null || payload.Length == 0) return;

            string text = Encoding.UTF8.GetString(payload);

            Console.WriteLine("====== IVAO PACKET ======");
            Console.WriteLine($"{udp.SourcePort} → {udp.DestinationPort}");
            Console.WriteLine(text);
            Console.WriteLine("==========================\n");
        }
    }
}
