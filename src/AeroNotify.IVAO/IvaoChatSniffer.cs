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
            // Obtém lista de interfaces
            var devices = CaptureDeviceList.Instance;

            if (devices.Count == 0)
            {
                Console.WriteLine("[IVAO] Nenhuma interface de rede encontrada.");
                return;
            }

            // Seleciona a primeira interface (ideal: escolher pela descrição)
            _device = devices[0];

            Console.WriteLine($"[IVAO] Usando interface: {_device.Description}");

            // Evento assíncrono de chegada de pacote
            _device.OnPacketArrival += OnPacketArrival;

            // Abre em modo promiscuo (captura tudo)
            _device.Open(DeviceMode.Promiscuous);

            // FILTRO: Capturar apenas pacotes da porta oficial de dados da IVAO (UDP 6809)
            _device.Filter = "udp port 6809";

            Console.WriteLine("[IVAO] Capturando pacotes da porta UDP 6809 (chat/posições)...");

            // Inicia captura
            _device.StartCapture();
        }

        public void Stop()
        {
            if (_device != null)
            {
                _device.StopCapture();
                _device.Close();
                Console.WriteLine("[IVAO] Captura encerrada.");
            }
        }

        private void OnPacketArrival(object sender, CaptureEventArgs e)
        {
            try
            {
                // Decodifica o pacote
                var packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

                var udp = packet.Extract<UdpPacket>();
                if (udp == null) return;

                byte[] payload = udp.PayloadData;

                if (payload == null || payload.Length == 0) return;

                // Converte bytes -> texto (IVAO usa UTF-8/ASCII)
                string text = Encoding.UTF8.GetString(payload);

                Console.WriteLine("===== PACOTE IVAO =====");
                Console.WriteLine($"Origem: {udp.SourcePort} -> Destino: {udp.DestinationPort}");
                Console.WriteLine("Conteúdo:");
                Console.WriteLine(text);
                Console.WriteLine("========================\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IVAO] Erro ao processar pacote: {ex.Message}");
            }
        }
    }
}
