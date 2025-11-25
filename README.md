# AeroNotify

![AeroNotify Logo](assets/logo.png)

## ✈️ Descrição

**AeroNotify** é um serviço de monitoramento e notificação que captura mensagens ATC (torre) das redes de simulação de voo **IVAO** e **VATSIM** e as retransmite como alertas via **WhatsApp**, **Telegram**, **SMS** ou **webhooks** personalizados.

O objetivo é permitir que pilotos, staff de VA e operadores de treinamento recebam notificações instantâneas sobre comunicações da torre, sem precisar monitorar o cliente de voz/texto o tempo todo.

---

## ✅ Recursos

- Captura em tempo real de mensagens ATC (IVAO e VATSIM)
- Envio de notificações para WhatsApp (Cloud API), Telegram Bot e SMS (Twilio/Nexmo)
- Arquitetura modular com providers para cada rede e canal de notificação
- Logging e persistência de mensagens (opcional, SQLite / PostgreSQL)
- Configuração por arquivo JSON / appsettings
- Cross-platform (.NET 8)

---


## 🛠️ Guia rápido — Instalação e execução

1. Clone o repositório:

```
git clone https://github.com/ySKELLETONX/AeroNotify.git
cd AeroNotify
```

2. Configure variáveis e segredos (appsettings.json)

3. Execute:

```
cd src/AeroNotify.CLI
dotnet run --project AeroNotify.CLI
```

---

## License

MIT License
