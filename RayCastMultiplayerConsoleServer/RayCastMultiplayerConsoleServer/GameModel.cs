﻿using System.Collections.Generic;

namespace RayCastMultiplayerConsoleServer
{
    internal class GameModel
    {
        private int MapWidth;
        private int MapHeight;
        private int[,] Map;
        private List<Player> Players;
        public GameModel() 
        {
            MapWidth = 10;
            MapHeight = 10;
            Map = new int[MapWidth, MapHeight];
            //for (int i = 0; i < MapWidth; i++)
            //{
            //    for (int j = 0; j < MapHeight; j++)
            //    {
            //        if (j == 0 | i == 0 | i==MapWidth-1 | j == MapHeight-1 ) { Map[i, j] = 1; } else { Map[i, j] = 0; }
            //    }
            //}
            int[,] map = { { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                           { 1, 0, 0, 0, 0, 0, 0, 0, 1 },
                           { 1, 1, 1, 0, 0, 0, 1, 1, 1 },
                           { 1, 0, 0, 0, 0, 0, 1, 1, 1 },
                           { 1, 1, 1, 0, 0, 0, 1, 1, 1 },
                           { 1, 1, 1, 0, 0, 0, 0, 0, 1 },
                           { 1, 0, 0, 0, 0, 0, 0, 0, 1 },
                           { 1, 0, 1, 0, 0, 0, 1, 0, 1 },
                           { 1, 0, 1, 0, 0, 0, 0, 0, 1 },
                           { 1, 0, 1, 0, 0, 1, 1, 1, 1 },
                           { 1, 0, 0, 0, 0, 0, 0, 0, 1 },
                           { 1, 1, 1, 1, 1, 1, 1, 1, 1 }, };
            Map = map;
            MapHeight = map.GetLength(0);
            MapWidth = map.GetLength(1);
            //Map[5, 5] = 3;
            Players = new List<Player>();
        }
        public int AddPlayer(Player p)
        {
            int maxID = -1;
            if (Players.Count != 0)
            {
                foreach (Player p2 in Players)
                {
                    if (maxID < p2.Id) maxID = p2.Id;
                }
            }
            p.Id = maxID + 1;
            p.posX = 1; p.posY = 1;
            p.dirX = -1; p.dirY = 0;
            p.planeX = 0; p.planeY = 0.66;
            Players.Add(p);
            return p.Id;
        }
        public void RemovePlayer(int playerID)
        {
            if (Players.Count != 0)
            {
                foreach (Player p2 in Players)
                {
                    if (playerID == p2.Id) 
                    { 
                        Players.Remove(p2);
                        return;
                    }
                }
            }
        }
        public List<Player> GetPlayers()
        {
            return Players;
        }
        public int[,] GetMap() { return Map; }
        public int GetMapWidth() { return MapWidth; }
        public int GetMapHeight() { return MapHeight; }
        public void MovePlayer(double dX, double dY, int playerID)
        {
            if (Players.Count != 0)
            {
                foreach (Player p in Players)
                {
                    if (p.Id == playerID)
                    {
                        p.posX += dX;
                        p.posY += dY;
                        return;
                    }
                }
            }
        }
        public void ChangePlayerAngle(double newDirX,  double newDirY, double newPlaneX, double newPlaneY, int playerID)
        {
            if (Players.Count != 0)
            {
                foreach (Player p in Players)
                {
                    if (p.Id == playerID)
                    {
                        p.dirX = newDirX;
                        p.dirY = newDirY;
                        p.planeX = newPlaneX;
                        p.planeY = newPlaneY;
                        return;
                    }
                }
            }
        }
    }
}
