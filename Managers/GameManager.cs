using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
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
        private Texture2D heartTexture;
        public ItemManager itemManager;
        public UIManager uiManager;
        private Dictionary<Player.PlayerState, Animation> playerAnimations;

        public int currentLevel = 1;
        public int maxLevel = 2;

        public Rectangle exitDoor;
        public bool hasExitDoor;

        public GameManager()
        {
            platforms = new List<Platform>();
            spikes = new List<Spike>();
            boxes = new List<Box>();
            currentTime = TimeState.Present;
        }

        public void LoadContent(ContentManager content, Texture2D dummy)
        {
            dummyTexture = dummy;
            Texture2D heartTex = content.Load<Texture2D>("Image/Heart");
            SpriteFont font = content.Load<SpriteFont>("GameFont");

            playerAnimations = new Dictionary<Player.PlayerState, Animation>()
            {
                { Player.PlayerState.Idle, new Animation(content.Load<Texture2D>("PlayerModel/Cyborg_idle"), 4) },
                { Player.PlayerState.Running, new Animation(content.Load<Texture2D>("PlayerModel/Cyborg_run"), 6) },
                { Player.PlayerState.Jumping, new Animation(content.Load<Texture2D>("PlayerModel/Cyborg_jump"), 4) },
                { Player.PlayerState.DoubleJumping, new Animation(content.Load<Texture2D>("PlayerModel/Cyborg_doublejump"), 6) },
                { Player.PlayerState.Hurt, new Animation(content.Load<Texture2D>("PlayerModel/Cyborg_hurt"), 2) },
                { Player.PlayerState.Die, new Animation(content.Load<Texture2D>("PlayerModel/Cyborg_death"), 6, false)}

            };

            itemManager = new ItemManager(heartTex);
            uiManager = new UIManager(font, heartTex);
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
            itemManager.Update(player);

            if (player.IsDead)
            {
                LoadLevel();
                return;
            }
            if (player.Position.Y > 1000)
            {
                LoadLevel();
                return;
            }

            if (hasExitDoor && player.Hitbox.Intersects(exitDoor))
            {
                if (currentLevel < maxLevel)
                {
                    currentLevel++;
                    LoadLevel();
                }
                else
                {
                    currentLevel = 1;
                    LoadLevel();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var platform in platforms) platform.Draw(spriteBatch);
            foreach (var spike in spikes) spike.Draw(spriteBatch);
            foreach (var box in boxes) box.Draw(spriteBatch);
            if (hasExitDoor) spriteBatch.Draw(dummyTexture, exitDoor, Color.Gold);
            itemManager.Draw(spriteBatch);
            player.Draw(spriteBatch);
        }

        private string[] GetLevelDesign(int levelNumber)
        {
            switch (levelNumber)
            {
                case 1:
                    return new string[]
                    {

                "0000000000000000000000000000000000000000",
                "0......................................0",
                "0......................................0",
                "0.......................0000000........0",
                "0.......................0.....0........0",
                "0.......................0....D0........0",
                "0.......................00...00........0",
                "0........................0...0.........0",
                "0........................0...0.........0",
                "0........................1...2.........0",
                "0........................1...2.........0",
                "0..........000....00.....1...2.........0",
                "0..B.........0...........0...0.........0",
                "0.H..P.......0......B....0...0.........0",
                "0000000......0SSSSSSSSSSS0SSS0SSSSSSSSS0",
                "0000000000000000000000000000000000000000"
            };
                case 2:
                    return new string[]
                    {
                "0000000000000000000000000000000000000000",
                "0P....................................D0",
                "000000..............................0000",
                "0....0..............................0..0",
                "0SSSS0SSSSSSSSSSSSSSSSSSSSSSSSSSSSSS0SS0",
                "0000000000000000000000000000000000000000"
                };
                default:
                    return new string[] { "00P0000D00" };
            }
        }
        private void LoadLevel()
        {
            platforms.Clear();
            spikes.Clear();
            boxes.Clear();
            itemManager.hearts.Clear();
            hasExitDoor = false;

            string[] levelDesign = GetLevelDesign(currentLevel);

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
                    else if (tile == 'P') player = new Player(new Vector2(x * tileSize, y * tileSize), playerAnimations);
                    else if (tile == 'B') boxes.Add(new Box(new Vector2(x * tileSize, y * tileSize), dummyTexture));
                    else if (tile == 'H') itemManager.AddHeart(rect);
                    else if (tile == 'D')
                    {
                        exitDoor = rect;
                        hasExitDoor = true;
                    }
                }
            }

        }
    }
}