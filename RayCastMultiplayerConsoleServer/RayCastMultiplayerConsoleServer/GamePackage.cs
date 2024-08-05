using LiteNetLib.Utils;
using System.Collections.Generic;

namespace RayCastMultiplayerConsoleServer
{
    internal class GamePackage : INetSerializable
    {
        public int MapWidth { get; set; }
        public int MapHeight { get; set; }
        public int[,] Map { get; set; }
        public List<Player> Players { get; set; }

        public GamePackage() { }

        public void Deserialize(NetDataReader reader)
        {
            MapWidth = reader.GetInt();
            MapHeight = reader.GetInt();
            Map = ArraySerializer.DeserializeMultidimensionalArray(reader);
            Players = PlayerListSerializer.DeserializeList(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(MapWidth);
            writer.Put(MapHeight);
            ArraySerializer.SerializeMultidimensionalArray(writer, Map);
            PlayerListSerializer.SerializeList(writer, Players);
        }
    }
}
