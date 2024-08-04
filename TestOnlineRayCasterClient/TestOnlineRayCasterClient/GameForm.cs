using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestOnlineRayCasterClient
{
    public delegate void StartUpdateTimer();
    public partial class GameForm : Form
    {
        private GameController gameController;

        struct Sprite
        {
            public double x;
            public double y;
            public int texture;
        };
        StartUpdateTimer startUpdateTimer;
        public void StartUpdTimer()
        {
            UpdateTimer.Enabled = true;
        }

        public GameForm(string adress, int port)
        {
            InitializeComponent();
            startUpdateTimer = StartUpdTimer;
            gameController = new GameController(adress, port, startUpdateTimer);
        }
        private void RayCast()
        {
            Pixel[,] pixels = new Pixel[pictureBox1.Width, pictureBox1.Height];
            Player player = gameController.GetPlayer();
            int[,] worldMap = gameController.GetMap();
            double[] ZBuffer = new double[pictureBox1.Width];
            for (int x = 0; x < pictureBox1.Width; x++)
            {
                //calculate ray position and direction
                double cameraX = 2 * x / (double)pictureBox1.Width - 1; //x-coordinate in camera space
                double rayDirX = player.dirX + player.planeX * cameraX;
                double rayDirY = player.dirY + player.planeY * cameraX;
                //which box of the map we're in
                int mapX = (int)(player.posX);
                int mapY = (int)(player.posY);

                //length of ray from current position to next x or y-side
                double sideDistX;
                double sideDistY;

                //length of ray from one x or y-side to next x or y-side
                double deltaDistX = (rayDirX == 0) ? 1e30 : Math.Abs(1 / rayDirX);
                double deltaDistY = (rayDirY == 0) ? 1e30 : Math.Abs(1 / rayDirY);
                double perpWallDist;

                //what direction to step in x or y-direction (either +1 or -1)
                int stepX;
                int stepY;

                int hit = 0; //was there a wall hit?
                int side = -1; //was a NS or a EW wall hit?
                          //calculate step and initial sideDist
                if (rayDirX < 0)
                {
                    stepX = -1;
                    sideDistX = (player.posX - mapX) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (mapX + 1.0 - player.posX) * deltaDistX;
                }
                if (rayDirY < 0)
                {
                    stepY = -1;
                    sideDistY = (player.posY - mapY) * deltaDistY;
                }
                else
                {
                    stepY = 1;
                    sideDistY = (mapY + 1.0 - player.posY) * deltaDistY;
                }
                while (hit == 0)
                {
                    //jump to next map square, either in x-direction, or in y-direction
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapX += stepX;
                        side = 0;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapY += stepY;
                        side = 1;
                    }
                    //Check if ray has hit a wall
                    try { if (worldMap[mapX, mapY] > 0) hit = 1; }
                    catch(IndexOutOfRangeException exc) { hit = -1; }
                }
                //Calculate distance projected on camera direction (Euclidean distance would give fisheye effect!)
                if (side == 0) perpWallDist = (sideDistX - deltaDistX);
                else perpWallDist = (sideDistY - deltaDistY);
                //Calculate height of line to draw on screen
                int lineHeight = (int)(pictureBox1.Height / perpWallDist);

                //calculate lowest and highest pixel to fill in current stripe
                int drawStart = -lineHeight / 2 + pictureBox1.Height / 2;
                if (drawStart < 0) drawStart = 0;
                int drawEnd = lineHeight / 2 + pictureBox1.Height / 2;
                if (drawEnd >= pictureBox1.Height) drawEnd = pictureBox1.Height - 1;
                //choose wall color
                //Pixel pixel = new Pixel();
                if(hit==-1)
                {
                    for (int y = 0; y < pictureBox1.Height; y++)
                    {
                        Pixel pixel = new Pixel();
                        pixel.X = x;
                        pixel.Y = y;
                        pixel.color = Color.Black;
                        if (y < drawStart || y >= drawEnd) pixel.color = Color.Black;
                        pixels[x, y] = pixel;
                    }
                    continue;
                }
                Color color;
                switch (worldMap[mapX, mapY])
                {
                    case 0: color = Color.Black; break; //red
                    case 1: color = Color.Red; break; //red
                    case 2: color = Color.Green; break; //green
                    case 3: color = Color.Blue; break; //blue
                    case 4: color = Color.White; break; //white
                    default: color = Color.Yellow; break; //yellow
                }

                //give x and y sides different brightness
                if (side == 1) 
                {
                    color = Color.FromArgb((byte)(color.A), (byte)(color.R / 2), (byte)(color.G / 2), (byte)(color.B / 2));
                }

                //draw the pixels of the stripe as a vertical line
                for(int y = 0; y<pictureBox1.Height; y++)
                {
                    Pixel pixel = new Pixel();
                    pixel.X = x;
                    pixel.Y = y;
                    pixel.color = color;
                    if (y < drawStart || y >= drawEnd) pixel.color = Color.Black;
                    pixels[x,y] = pixel;
                }
                ZBuffer[x] = perpWallDist;
            }

            //Sprite Casting

            //sort sprites from far to close
            List<Player> players = gameController.GetPlayers();
            int[] spriteOrder = new int[players.Count - 1];
            double[] spriteDistance = new double[players.Count - 1];

            Sprite[] sprite = new Sprite[players.Count - 1];
            List<Sprite> spriteList = new List<Sprite>();
            for(int i = 0; i < players.Count; i++)
            {
                if (players[i].Id != gameController.GetPlayerId())
                {
                    Sprite spr;
                    spr.x = players[i].posX; spr.y = players[i].posY;
                    spr.texture = 0;
                    spriteList.Add(spr);
                }
            }
            sprite = spriteList.ToArray();

            for (int i = 0; i < players.Count-1; i++)
            {
                spriteOrder[i] = i;
                spriteDistance[i] = ((player.posX - sprite[i].x) * (player.posX - sprite[i].x) + (player.posY - sprite[i].y) * (player.posY - sprite[i].y)); //sqrt not taken, unneeded
            }
            sortSprites(ref spriteOrder, ref spriteDistance, players.Count - 1);

            //after sorting the sprites, do the projection and draw them
            for (int i = 0; i < sprite.Length; i++)
            {
                //translate sprite position to relative to camera
                double spriteX = sprite[spriteOrder[i]].x - player.posX;
                double spriteY = sprite[spriteOrder[i]].y - player.posY;

                //transform sprite with the inverse camera matrix
                // [ planeX   dirX ] -1                                       [ dirY      -dirX ]
                // [               ]       =  1/(planeX*dirY-dirX*planeY) *   [                 ]
                // [ planeY   dirY ]                                          [ -planeY  planeX ]

                double invDet = 1.0 / (player.planeX * player.dirY - player.dirX * player.planeY); //required for correct matrix multiplication

                double transformX = invDet * (player.dirY * spriteX - player.dirX * spriteY);
                double transformY = invDet * (-player.planeY * spriteX + player.planeX * spriteY); //this is actually the depth inside the screen, that what Z is in 3D

                transformY = Math.Abs(transformY) <0.01 ? Math.Sign(transformY)*0.01 : transformY;

                int spriteScreenX = (int)((pictureBox1.Width / 2) * (1 + transformX / (transformY)));

                //calculate height of the sprite on screen
                int spriteHeight = Math.Abs((int)(pictureBox1.Height / transformY)); //using 'transformY' instead of the real distance prevents fisheye
                                                                                       //calculate lowest and highest pixel to fill in current stripe
                int drawStartY = -spriteHeight / 2 + pictureBox1.Height / 2;
                if (drawStartY < 0) drawStartY = 0;
                int drawEndY = spriteHeight / 2 + pictureBox1.Height / 2;
                if (drawEndY >= pictureBox1.Height) drawEndY = pictureBox1.Height - 1;

                //calculate width of the sprite
                int spriteWidth = Math.Abs((int)(pictureBox1.Height / (transformY)));
                int drawStartX = -spriteWidth / 2 + spriteScreenX;
                if (drawStartX < 0) drawStartX = 0;
                int drawEndX = spriteWidth / 2 + spriteScreenX;
                if (drawEndX >= pictureBox1.Width) drawEndX = pictureBox1.Width - 1;
                //loop through every vertical stripe of the sprite on screen
                for (int stripe = drawStartX; stripe < drawEndX; stripe++)
                {
                    //int texX = int(256 * (stripe - (-spriteWidth / 2 + spriteScreenX)) * texWidth / spriteWidth) / 256;
                    //the conditions in the if are:
                    //1) it's in front of camera plane so you don't see things behind you
                    //2) it's on the screen (left)
                    //3) it's on the screen (right)
                    //4) ZBuffer, with perpendicular distance
                    if (transformY > 0 && stripe > 0 && stripe < pictureBox1.Width && transformY < ZBuffer[stripe])
                        for (int y = drawStartY; y < drawEndY; y++) //for every pixel of the current stripe
                        {
                            int d = (y) * 256 - pictureBox1.Height * 128 + spriteHeight * 128; //256 and 128 factors to avoid floats
                            //int texY = ((d * texHeight) / spriteHeight) / 256;
                            //Uint32 color = texture[sprite[spriteOrder[i]].texture][texWidth * texY + texX]; //get current color from the texture
                            //if ((color & 0x00FFFFFF) != 0) buffer[y][stripe] = color; //paint pixel if it isn't black, black is the invisible color
                            if(y%2==1 || stripe%2==1)
                                pixels[stripe, y].color = Color.Purple;
                            else
                                pixels[stripe, y].color = Color.Aqua;
                        }
                }
            }
                Draw(pixels);
        }
        private void Draw(Pixel[,] rays)
        {
            Bitmap SCREEN = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height);
            BitmapData data = SCREEN.LockBits(new Rectangle(0, 0, SCREEN.Width, SCREEN.Height), ImageLockMode.WriteOnly, SCREEN.PixelFormat);
            int bytesPerPixel = Bitmap.GetPixelFormatSize(SCREEN.PixelFormat) / 8;
            byte[] pixels = new byte[data.Height * data.Stride];
            Parallel.For(0, pictureBox1.Width, x =>
            {
                Parallel.For(0, pictureBox1.Height, y =>
                {
                    int i = (y * data.Stride) + (x * bytesPerPixel);
                    Pixel pixel = rays[x, y];
                    Color color = pixel.color;
                    pixels[i] = color.B;
                    pixels[i + 1] = color.G;
                    pixels[i + 2] = color.R;
                    if (bytesPerPixel == 4)
                        pixels[i + 3] = color.A;
                });
            });
            rays = null;
            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
            SCREEN.UnlockBits(data);
            pictureBox1.Image = SCREEN;
            infoLabel.Text = $"W: {pictureBox1.Width} H: {pictureBox1.Height}";
        }
        void sortSprites(ref int[] order, ref double[] dist, int amount)
        {
            Tuple < double, int>[] sprites = new Tuple<double, int>[amount];
            for (int i = 0; i < amount; i++)
            {
                sprites[i] = new Tuple<double, int>(dist[i], order[i]);
            }
            sprites = sprites.OrderBy(item => item.Item1).ToArray();
            // restore in reverse order to go from farthest to nearest
            for (int i = 0; i < amount; i++)
            {
                dist[i] = sprites[amount - i - 1].Item1;
                order[i] = sprites[amount - i - 1].Item2;
            }
        }
        private void MovePlayer(double dX, double dY)
        {
            gameController.MovePlayer(dX, dY);
        }
        private void ChangePlayerAngle(double newDirX, double newDirY, double newPlaneX, double newPlaneY)
        {
            gameController.ChangePlayerAngle(newDirX, newDirY, newPlaneX, newPlaneY);
        }
        private void GameForm_Resize(object sender, EventArgs e)
        {
            this.pictureBox1.Size = this.Size;
            this.infoLabel.Location = new Point(0, this.Size.Height/2);
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            RayCast();
        }

        private void GameForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 'w')
            {
                int[,] worldMap = gameController.GetMap();
                Player player = gameController.GetPlayer();
                double moveSpeed = 0.05;
                double dX=0, dY=0;
                if (worldMap[(int)(player.posX + player.dirX * moveSpeed), (int)player.posY ] == 0) dX = player.dirX * moveSpeed;
                if (worldMap[(int)(player.posX), (int)(player.posY + player.dirY * moveSpeed)] == 0) dY = player.dirY * moveSpeed;
                MovePlayer(dX, dY);
            }
            if (e.KeyChar == 's')
            {
                int[,] worldMap = gameController.GetMap();
                Player player = gameController.GetPlayer();
                double moveSpeed = 0.05;
                double dX = 0, dY = 0;
                try
                {
                    if (worldMap[(int)(player.posX - player.dirX * moveSpeed), (int)player.posY] == 0) dX = -player.dirX * moveSpeed;
                    if (worldMap[(int)(player.posX), (int)(player.posY - player.dirY * moveSpeed)] == 0) dY = -player.dirY * moveSpeed;
                }
                catch (Exception ex) { }
                MovePlayer(dX, dY);
            }
            if (e.KeyChar == 'd')
            {
                int[,] worldMap = gameController.GetMap();
                Player player = gameController.GetPlayer();
                double rotSpeed = 0.03;
                double oldDirX = player.dirX;
                double newDirX = player.dirX * Math.Cos(-rotSpeed) - player.dirY * Math.Sin(-rotSpeed);
                double newDirY = oldDirX * Math.Sin(-rotSpeed) + player.dirY * Math.Cos(-rotSpeed);
                double oldPlaneX = player.planeX;
                double newPlaneX = player.planeX * Math.Cos(-rotSpeed) - player.planeY * Math.Sin(-rotSpeed);
                double newPlaneY = oldPlaneX * Math.Sin(-rotSpeed) + player.planeY * Math.Cos(-rotSpeed);
                ChangePlayerAngle(newDirX, newDirY, newPlaneX, newPlaneY);
            }
            if (e.KeyChar == 'a')
            {
                int[,] worldMap = gameController.GetMap();
                Player player = gameController.GetPlayer();
                double rotSpeed = 0.03;
                double oldDirX = player.dirX;
                double newDirX = player.dirX * Math.Cos(rotSpeed) - player.dirY * Math.Sin(rotSpeed);
                double newDirY = oldDirX * Math.Sin(rotSpeed) + player.dirY * Math.Cos(rotSpeed);
                double oldPlaneX = player.planeX;
                double newPlaneX = player.planeX * Math.Cos(rotSpeed) - player.planeY * Math.Sin(rotSpeed);
                double newPlaneY = oldPlaneX * Math.Sin(rotSpeed) + player.planeY * Math.Cos(rotSpeed);
                ChangePlayerAngle(newDirX, newDirY, newPlaneX, newPlaneY);
            }
        }

        private void GameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            gameController.Close();
        }

        private void GameForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            gameController.Close();
        }
    }
}
