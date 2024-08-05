using LiteNetLib.Utils;
using System.Collections.Generic;
namespace RayCastMultiplayerConsoleServer
{
    internal class PlayerListSerializer
    {
            public static void SerializeList(NetDataWriter writer, List<Player> list)
            {
                // Write the count of the list
                writer.Put(list.Count);

                // Serialize each Player object
                foreach (var item in list)
                {
                    item.Serialize(writer);
                }
            }

            public static List<Player> DeserializeList(NetDataReader reader)
            {
                // Read the count of the list
                int count = reader.GetInt();
                List<Player> list = new List<Player>(count);

                // Deserialize each Player object
                for (int i = 0; i < count; i++)
                {
                    Player p = new Player();
                    p.Deserialize(reader);
                    list.Add(p);
                }

                return list;
            }
    }
}
