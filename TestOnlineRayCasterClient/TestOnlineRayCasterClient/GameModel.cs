using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestOnlineRayCasterClient
{
    internal class GameModel
    {
        private int MapWidth;
        private int MapHeight;
        private int[,] Map;
        private List<Player> Players;
        public GameModel() 
        {
            //MapWidth = 10;
            //MapHeight = 10;
            //Map = new int[MapWidth, MapHeight];
            //for (int i = 0; i < MapWidth; i++)
            //{
            //    for (int j = 0; j < MapHeight; j++)
            //    {
            //        if (j == 0 | i == 0 | i==MapWidth-1 | j == MapHeight-1 ) { Map[i, j] = 1; } else { Map[i, j] = 0; }
            //    }
            //}
            //Players = new List<Player>();
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
            p.posX = 3; p.posY = 3;
            p.dirX = -1; p.dirY = 0;
            p.planeX = 0; p.planeY = 0.66;
            Players.Add(p);
            return p.Id;
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
        public void SetPlayers(List<Player> players, int playerID)
        {
            if(Players == null)
            {
                this.Players = players;
                return;
            }
            Player pl = null;
            foreach(Player p in this.Players)
            {
                if (p.Id == playerID) pl = p;
            }
            for(int i = 0; i<players.Count; i++)
            {
                if (players[i].Id == playerID) players[i] = pl;
            }
            this.Players = players;
        }
        public void SetPlayers(List<Player> players)
        {
            this.Players = players;
        }
        public void SetMapWidth(int w) { this.MapWidth = w; } 
        public void SetMapHeight(int h) { this.MapHeight = h; }
        public void SetMap(int[,] map) { this.Map = map; }
    }
}
