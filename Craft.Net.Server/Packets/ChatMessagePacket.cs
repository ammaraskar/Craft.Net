using System.Linq;
using Craft.Net.Server.Events;

namespace Craft.Net.Server.Packets
{
    public class ChatMessagePacket : Packet
    {
        public string Message;

        public ChatMessagePacket()
        {
        }

        public ChatMessagePacket(string message)
        {
            this.Message = message;
        }

        public override byte PacketID
        {
            get { return 0x3; }
        }

        public override int TryReadPacket(byte[] buffer, int length)
        {
            int offset = 1;
            if (!TryReadString(buffer, ref offset, out Message))
                return -1;
            return offset;
        }

        public override void HandlePacket(MinecraftServer server, ref MinecraftClient client)
        {
            server.Log("<" + client.Username + "> " + Message);
            var args = new ChatMessageEventArgs(client, Message);
            server.FireOnChatMessage(args);
            if (!args.Handled)
                server.SendChat("<" + client.Username + "> " + Message);
        }

        public override void SendPacket(MinecraftServer server, MinecraftClient client)
        {
            byte[] buffer = new[] {PacketID}
                .Concat(CreateString(Message)).ToArray();
            client.SendData(buffer);
        }
    }
}