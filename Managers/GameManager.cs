using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace FinalProject.Managers
{
    public class GameManager
    {
        public Player player;
        public List<Platform> platforms;
        public List<Spike> spikes;
        public List<Box> boxes;
        public TimeState currentTime;
        private Texture2D dummyTexture;

        public GameManager()
        {
            platforms = new List<Platform>();
            spikes = new List<Spike>();
            boxes = new List<Box>();
            currentTime = TimeState.Present;
        }

        public void LoadContent(Texture2D texture)
        {
            dummyTexture = texture;
            LoadLevel();
        }

        public void Update()
        {
            // สลับเวลาผ่าน InputManager
            if (InputManager.IsKeyPressed(Keys.LeftShift))
            {
                currentTime = (currentTime == TimeState.Present) ? TimeState.Past : TimeState.Present;
            }

            foreach (var platform in platforms) platform.Update(currentTime);
            foreach (var spike in spikes) spike.Update(currentTime);
            foreach (var box in boxes) box.Update(platforms, player);

            player.Update(platforms, boxes);
            player.CheckSpikeCollision(spikes);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var platform in platforms) platform.Draw(spriteBatch);
            foreach (var spike in spikes) spike.Draw(spriteBatch);
            foreach (var box in boxes) box.Draw(spriteBatch);
            player.Draw(spriteBatch);
        }

        private void LoadLevel()
        {
            // วางดีไซน์ด่านของคุณไว้ตรงนี้เหมือนเดิมครับ
            string[] levelDesign = new string[]
            {
                "0000000000000000000000000000000000000000",
                "0......................................0",
                "0......................................0",
                "0.......................0000000........0",
                "0.......................0.....0........0",
                "0.......................0.....0........0",
                "0.......................00...00........0",
                "0........................0...0.........0",
                "0........................0...0.........0",
                "0........................1...2.........0",
                "0........................1...2.........0",
                "0..........000....00.....1...2.........0",
                "0..B.........0...........0...0.........0",
                "0....P.......0......B....0...0.........0",
                "0000000......0SSSSSSSSSSS0SSS0SSSSSSSSS0",
                "0000000000000000000000000000000000000000"
            };

            int tileSize = 64;

            for (int y = 0; y < levelDesign.Length; y++)
            {
                for (int x = 0; x < levelDesign[y].Length; x++)
                {
                    char tile = levelDesign[y][x];
                    Rectangle rect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);

                    if (tile == '0') platforms.Add(new Platform(rect, TimeState.Permanent, dummyTexture));
                    else if (tile == '1') platforms.Add(new Platform(rect, TimeState.Present, dummyTexture));
                    else if (tile == '2') platforms.Add(new Platform(rect, TimeState.Past, dummyTexture));
                    else if (tile == 'S') spikes.Add(new Spike(rect, TimeState.Present, dummyTexture));
                    else if (tile == 'P') player = new Player(new Vector2(x * tileSize, y * tileSize), dummyTexture);
                    else if (tile == 'B') boxes.Add(new Box(new Vector2(x * tileSize, y * tileSize), dummyTexture));
                }
            }
        }
    }
}