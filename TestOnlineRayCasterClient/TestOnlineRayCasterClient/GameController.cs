using LiteNetLib.Utils;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace TestOnlineRayCasterClient
{
    internal class GameController
    {
        private GameModel Game;
        private EventBasedNetListener listener;
        private NetManager client;
        private NetPacketProcessor processor;
        private NetPeer peer;
        private int PlayerID;
        private bool formOpened;
        public GameController(string adress, int port, StartUpdateTimer startUpdateTimer)
        {
            Game = new GameModel();
            listener = new EventBasedNetListener();
            client = new NetManager(listener);
            processor = new NetPacketProcessor();
            client.Start();
            client.Connect(adress, port, "SomeKey");
            Application.ApplicationExit += (sender, e) => formOpened=false;
            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
            {
                peer = fromPeer;
                int packetType = dataReader.GetInt();
                if (packetType == 0)
                {
                    PlayerID = dataReader.GetInt();
                    GamePackage gamePacket = new GamePackage();
                    gamePacket.Deserialize(dataReader);
                    Game.SetMapWidth(gamePacket.MapWidth);
                    Game.SetMapHeight(gamePacket.MapHeight);
                    Game.SetMap(gamePacket.Map);
                    Game.SetPlayers(gamePacket.Players);
                    startUpdateTimer();
                }
                if (packetType == 1)
                {
                    GamePackage gamePacket = new GamePackage();
                    gamePacket.Deserialize(dataReader);
                    Game.SetMapWidth(gamePacket.MapWidth);
                    Game.SetMapHeight(gamePacket.MapHeight);
                    Game.SetMap(gamePacket.Map);
                    Game.SetPlayers(gamePacket.Players);
                }
                dataReader.Recycle();
            };
            formOpened = true;
            new Thread(() =>
            {
                while (formOpened)
                {
                    client.PollEvents();
                    Thread.Sleep(15);
                }
            }).Start();

            Thread.Sleep(15);
            
            startUpdateTimer();
        }
        public Player GetPlayer()
        {
            List<Player> players = Game.GetPlayers();
            foreach (Player player in players)
            {
                if(PlayerID==player.Id) return player;
            }
            return null;
        }
        public List<Player> GetPlayers()
        {
            return Game.GetPlayers();
        }
        public int GetPlayerId() { return PlayerID; }
        public int[,] GetMap() { return Game.GetMap(); }
        public int AddPlayer()
        {
            return Game.AddPlayer(new Player());
        }
        public void MovePlayer(double dX, double dY)
        {
            Game.MovePlayer(dX, dY, PlayerID);
            NetDataWriter writer = new NetDataWriter();
            writer.Put(2);
            writer.Put(dX);
            writer.Put(dY);
            writer.Put(PlayerID);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        public void Close()
        {
            formOpened = false;
        }
        public void ChangePlayerAngle(double newDirX, double newDirY, double newPlaneX, double newPlaneY)
        {
            Game.ChangePlayerAngle(newDirX, newDirY, newPlaneX, newPlaneY, PlayerID);
            NetDataWriter writer = new NetDataWriter();
            writer.Put(3);
            writer.Put(newDirX);
            writer.Put(newDirY);
            writer.Put(newPlaneX);
            writer.Put(newPlaneY);
            writer.Put(PlayerID);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }
}
