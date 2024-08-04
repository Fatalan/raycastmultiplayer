using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOnlineRayCasterClient
{
    internal class Player : INetSerializable
    {
        public int Id;
        public double posX, posY;
        public double dirX, dirY;
        public double planeX, planeY;

        public void Deserialize(NetDataReader reader)
        {
            Id = reader.GetInt();
            posX = reader.GetDouble();
            posY = reader.GetDouble();
            dirX = reader.GetDouble();
            dirY = reader.GetDouble();
            planeX = reader.GetDouble();
            planeY = reader.GetDouble();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Id);
            writer.Put(posX);
            writer.Put(posY);
            writer.Put(dirX);
            writer.Put(dirY);
            writer.Put(planeX);
            writer.Put(planeY);
        }
    }
}
