using LiteNetLib.Utils;
using LiteNetLib;
using RayCastMultiplayerConsoleServer;
using System.Collections.Generic;
using System.Threading;
using System;


class Program {
    static void Main(string[] args) {
        NetPacketProcessor processor = new NetPacketProcessor();
        processor.RegisterNestedType<Player>(() => { return new Player(); });
        processor.RegisterNestedType<GamePackage>(() => { return new GamePackage(); });
        GameModel game = new GameModel();
        EventBasedNetListener listener = new EventBasedNetListener();
        NetManager server = new NetManager(listener);
        Dictionary<int, int> peerPlayerIDs = new Dictionary<int, int>();
        server.Start(9999 /* port */);



        listener.ConnectionRequestEvent += request =>
        {
            if (server.ConnectedPeersCount < 10 /* max connections */)
                request.AcceptIfKey("SomeKey");
            else
                request.Reject();
        };

        listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine("We got connection: {0}", peer);  // Show peer ip
            NetDataWriter writer = new NetDataWriter();         // Create writer class
                                                                //writer.Put("Hello client!");                        // Put some string
                                                                //peer.Send(writer, DeliveryMethod.ReliableOrdered);  // Send with reliability
            writer.Put(0);
            int newPlayerId = game.AddPlayer(new Player());
            peerPlayerIDs.Add(peer.Id, newPlayerId);
            writer.Put(newPlayerId);
            GamePackage pack = new GamePackage
            {
                MapWidth = game.GetMapWidth(),
                MapHeight = game.GetMapHeight(),
                Map = game.GetMap(),
                Players = game.GetPlayers(),
            };
            pack.Serialize(writer);
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        };
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {
            //dataReader.Recycle();
            int packetType = dataReader.GetInt();
            if (packetType == 2)
            {
                game.MovePlayer(dataReader.GetDouble(), dataReader.GetDouble(), dataReader.GetInt());
            }
            if (packetType == 3)
            {
                game.ChangePlayerAngle(dataReader.GetDouble(), dataReader.GetDouble(), dataReader.GetDouble(), dataReader.GetDouble(), dataReader.GetInt());
            }
            NetDataWriter writer = new NetDataWriter();
            writer.Put(1);
            //int newPlayerId = game.AddPlayer(new Player());
            //writer.Put(newPlayerId);
            GamePackage pack = new GamePackage
            {
                MapWidth = game.GetMapWidth(),
                MapHeight = game.GetMapHeight(),
                Map = game.GetMap(),
                Players = game.GetPlayers(),
            };
            pack.Serialize(writer);
            server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
        };
        listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            game.RemovePlayer(peerPlayerIDs[peer.Id]);
            peerPlayerIDs.Remove(peer.Id);
        };
        while (true)
        {
            server.PollEvents();
            NetDataWriter writer = new NetDataWriter();
            writer.Put(1);
            GamePackage pack = new GamePackage
            {
                MapWidth = game.GetMapWidth(),
                MapHeight = game.GetMapHeight(),
                Map = game.GetMap(),
                Players = game.GetPlayers(),
            };
            pack.Serialize(writer);
            server.SendToAll(writer, DeliveryMethod.ReliableOrdered);
            Thread.Sleep(15);
        }
        server.Stop();
    }
}