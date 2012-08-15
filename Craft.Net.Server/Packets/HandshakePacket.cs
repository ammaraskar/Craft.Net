using System;
using System.Net;

namespace Craft.Net.Server.Packets
{
    public class HandshakePacket : Packet
    {
        public string Hostname;
        public int Port;
        public byte ProtocolVersion;
        public string Username;

        public override byte PacketID
        {
            get { return 0x02; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!TryReadByte(buffer, ref offset, out ProtocolVersion))
                return -1;
            if (!TryReadString(buffer, ref offset, out Username))
                return -1;
            if (!TryReadString(buffer, ref offset, out Hostname))
                return -1;
            if (!TryReadInt(buffer, ref offset, out Port))
                return -1;
            return offset;
        }

        public override void HandlePacket(MinecraftServer server, ref MinecraftClient client)
        {
            if (ProtocolVersion < MinecraftServer.ProtocolVersion)
            {
                client.SendPacket(new DisconnectPacket("Outdated client!"));
                server.ProcessSendQueue();
                return;
            }
            if (ProtocolVersion > MinecraftServer.ProtocolVersion)
            {
                client.SendPacket(new DisconnectPacket("Outdated server!"));
                server.ProcessSendQueue();
                return;
            }
            client.Username = Username;
            client.Hostname = Hostname + ":" + Port.ToString();
            // Respond with encryption request
            if (server.OnlineMode)
                client.AuthenticationHash = CreateHash();
            else
                client.AuthenticationHash = "-";
            if (server.EncryptionEnabled)
            {
                var keyRequest =
                    new EncryptionKeyRequestPacket(client.AuthenticationHash,
                                                   server.ServerKey);
                client.SendPacket(keyRequest);
                server.ProcessSendQueue();
            }
            else
                server.LogInPlayer(client);

            client.StartKeepAliveTimer();
        }

        public override void SendPacket(MinecraftServer server, MinecraftClient client)
        {
            throw new InvalidOperationException();
        }

        private string CreateHash()
        {
            byte[] hash = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(new Random().Next()));
            string response = "";
            foreach (byte b in hash)
            {
                if (b < 0x10)
                    response += "0";
                response += b.ToString("x");
            }
            return response;
        }
    }
}