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
        public List<Enemy> enemies;
        public List<PressurePlate> plates;
        public List<Hazard> hazards;
        public List<SwingingHazard> swingingHazards;
        public TimeState currentTime;
        private Texture2D dummyTexture;
        private Texture2D terrainTexture;
        private Texture2D spikesTexture;
        private Texture2D heartTexture;
        private Texture2D maceTexture;
        private Texture2D maceBlockTexture;

        private Texture2D bgBrown;
        private Texture2D bgGray;
        private float bgScrollX = 0f;
        private float bgScrollY = 0f;

        public ItemManager itemManager;
        public UIManager uiManager;
        public ParticleManager particleManager;
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
            enemies = new List<Enemy>();
            hazards = new List<Hazard>();
            swingingHazards = new List<SwingingHazard>();
            plates = new List<PressurePlate>();
            currentTime = TimeState.Present;
        }

        public void LoadContent(ContentManager content, Texture2D dummy)
        {
            dummyTexture = dummy;
            Texture2D heartTex = content.Load<Texture2D>("Image/Heart");
            SpriteFont font = content.Load<SpriteFont>("GameFont");
            terrainTexture = content.Load<Texture2D>("Terrain/Terrain");
            spikesTexture = content.Load<Texture2D>("Spike/Idle");
            maceTexture = content.Load<Texture2D>("Spike/Spiked Ball");
            maceBlockTexture = content.Load<Texture2D>("Spike/Block");

            bgBrown = content.Load<Texture2D>("Terrain/Brown");
            bgGray = content.Load<Texture2D>("Terrain/Gray");


            playerAnimations = new Dictionary<Player.PlayerState, Animation>()
            {
                { Player.PlayerState.Idle, new Animation(content.Load<Texture2D>("PlayerModel/Idle"), 11) },
                { Player.PlayerState.Running, new Animation(content.Load<Texture2D>("PlayerModel/Run"), 12) },
                { Player.PlayerState.Jumping, new Animation(content.Load<Texture2D>("PlayerModel/Jump"), 1) },
                { Player.PlayerState.Falling, new Animation(content.Load<Texture2D>("PlayerModel/Fall"), 1) },
                { Player.PlayerState.DoubleJumping, new Animation(content.Load<Texture2D>("PlayerModel/Double_Jump"), 6) },
                { Player.PlayerState.WallClinging, new Animation(content.Load<Texture2D>("PlayerModel/Wall_Jump"), 5) },
                { Player.PlayerState.Hurt, new Animation(content.Load<Texture2D>("PlayerModel/Hit"), 7, false) },
                { Player.PlayerState.Die, new Animation(content.Load<Texture2D>("PlayerModel/Hit"), 7, false)},
            };

            itemManager = new ItemManager(heartTex);
            uiManager = new UIManager(font, heartTex);
            particleManager = new ParticleManager();
            LoadLevel();
        }

        public void Update(GameTime gameTime)
        {
            bgScrollX = 0f;
            bgScrollY += 0.5f;
            // สลับเวลาผ่าน InputManager
            if (InputManager.IsKeyPressed(Keys.LeftControl))
            {
                currentTime = (currentTime == TimeState.Present) ? TimeState.Past : TimeState.Present;
            }

            foreach (var platform in platforms) platform.Update(currentTime, plates); ;
            foreach (var spike in spikes) spike.Update(currentTime);
            foreach (var hazard in hazards) hazard.Update(gameTime, currentTime);
            foreach (var box in boxes) box.Update(platforms, player);
            foreach (var enemy in enemies) enemy.Update(platforms, boxes);
            foreach (var plate in plates) plate.Update(player, boxes);

            player.Update(platforms, boxes, particleManager);
            player.CheckSpikeCollision(spikes);
            itemManager.Update(player);
            particleManager.Update();

            if (hasExitDoor && player.Hitbox.Intersects(exitDoor))
            {
                currentLevel++;
                LoadLevel();
            }

            foreach (var enemy in enemies)
            {
                if (player.Hitbox.Intersects(enemy.Hitbox))
                {
                    int pushDir = (player.Position.X < enemy.Position.X) ? -1 : 1;
                    player.TakeDamage(pushDir);
                }
            }

            foreach (var hazard in hazards)
            {
                if (hazard.IsDangerous && player.Hitbox.Intersects(hazard.Hitbox))
                {
                    // Push player away from hazard center
                    int pushDir = (player.Position.X < hazard.Hitbox.Center.X) ? -1 : 1;
                    player.TakeDamage(pushDir);
                }
            }

            foreach (var mace in swingingHazards)
            {
                mace.Update(gameTime);
                if (player.Hitbox.Intersects(mace.Hitbox))
                {
                    int pushDir = (player.Position.X < mace.HeadPosition.X) ? -1 : 1;
                    player.TakeDamage(pushDir);
                }
            }

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
            foreach (var enemy in enemies) enemy.Draw(spriteBatch);
            foreach (var hazard in hazards) hazard.Draw(spriteBatch);
            foreach (var mace in swingingHazards) mace.Draw(spriteBatch);
            foreach (var plate in plates) plate.Draw(spriteBatch);
            if (hasExitDoor) spriteBatch.Draw(dummyTexture, exitDoor, Color.Gold);
            itemManager.Draw(spriteBatch);
            particleManager.Draw(spriteBatch);
            player.Draw(spriteBatch);
        }

        public void DrawBackground(SpriteBatch spriteBatch)
        {
            // ✨ 1. ตัดสินใจว่าจะใช้รูปไหน (ปัจจุบัน = Gray, อดีต = Brown)
            Texture2D currentBg = (currentTime == TimeState.Present) ? bgGray : bgBrown;

            // ✨ 2. ระบบปูกระเบื้อง (Tiling) ให้เต็มฉาก
            // ตั้งค่าสเกลพื้นหลังให้ใหญ่ขึ้นนิดนึง (คูณ 2) ลายซิกแซกจะได้ดูสวยสมส่วนกับบล็อก 32px
            int bgScale = 2;
            int drawWidth = currentBg.Width * bgScale;
            int drawHeight = currentBg.Height * bgScale;

            int offsetX = (int)(Math.Abs(bgScrollX) % drawWidth) * Math.Sign(bgScrollX);
            int offsetY = (int)(Math.Abs(bgScrollY) % drawHeight) * Math.Sign(bgScrollY);

            // ลูปวาดพื้นหลังให้กว้าง 4000x2000 พิกเซล (คลุมมิดด่านใหญ่แน่นอน)
            for (int y = -drawHeight; y < 2000; y += drawHeight)
            {
                for (int x = -drawWidth; x < 4000; x += drawWidth)
                {
                    spriteBatch.Draw(currentBg, new Rectangle(x + offsetX, y + offsetY, drawWidth, drawHeight), Color.White);
                }
            }
        }

        private (string[] presentMap, string[] pastMap) GetLevelDesign(int levelNumber)
        {
            switch (levelNumber)
            {
                case 1:
                    // 🌍 มิติปัจจุบัน (Present - หญ้า)
                    // ใช้ T สำหรับหนามที่จะโผล่มาในเวลานี้
                    // ใช้ X สำหรับหนามถาวรที่มีตลอด
                    string[] present = new string[]
                    {
                        "0000000000000000000000000000000000000000",
                        "8......................................8",
                        "8.....................................D8",
                        "8....O..............................0008",
                        "8...................................8888",
                        "8...................................8888",
                        "8..........[11].....................8888",
                        "8..........{44}.....................8888",
                        "8..........{44}.........T...........8888", // <--- หนามปัจจุบัน
                        "8..........{44}.......[111].........8888",
                        "8.....................{444}.........8888",
                        "8.....................{444}............8",
                        "8......o..............{444}............8",
                        "8.....................{444}............8",
                        "8...............[1]...{444}............8",
                        "8P..............{4}...{444}............8",
                        "0000............{4}...{444}............8",
                        "8888000.........{4}...{444}............8",
                        "888888800.......{4}...{444}....000000008",
                        "888888888WXXXXXX888XXX88888XXXX888888888", // <--- พื้นเป็นหนามถาวร (X)
                        "8888888888888888888888888888888888888888",
                        "8888888888888888888888888888888888888888",
                        "8888888888888888888888888888888888888888"
                    };

                    // 🌌 มิติอดีต (Past - ไม้)
                    // ใช้ t สำหรับหนามที่จะโผล่มาในเวลานี้
                    string[] past = new string[]
                    {
                        "........................................",
                        "........................................",
                        "........................................",
                        "........................................",
                        "........................................",
                        "........................................",
                        "........................................",
                        "........................................",
                        "........................................",
                        "........................................",
                        "........................t...............", // <--- หนามอดีต
                        "......................222...............",
                        "......................555...............",
                        "......................555...............",
                        "......................555...............",
                        "........................................",
                        "........................................",
                        ".........222............................",
                        ".........555............................",
                        "........................................",
                        "........................................",
                        "........................................",
                        "........................................"
                    };
                    return (present, past);

                default:
                    return (new string[] { "00P0000D00" }, new string[] { ".........." });
            }
        }

        private void LoadLayer(string[] levelLayer, int tileSize)
        {
            for (int y = 0; y < levelLayer.Length; y++)
            {
                for (int x = 0; x < levelLayer[y].Length; x++)
                {
                    char tile = levelLayer[y][x];
                    if (tile == '.') continue; // ถ้าเป็นจุดว่างๆ ให้ข้ามไปเลย (ช่วยให้เกมรันเร็วขึ้น)

                    Rectangle rect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);

                    Rectangle stoneTop = new Rectangle(16, 0, 16, 16);
                    Rectangle stoneCenter = new Rectangle(16, 16, 16, 16);

                    Rectangle grassLeft = new Rectangle(96, 0, 16, 16);
                    Rectangle grassMid = new Rectangle(112, 0, 16, 16);
                    Rectangle grassRight = new Rectangle(128, 0, 16, 16);

                    Rectangle dirtLeft = new Rectangle(96, 16, 16, 16);
                    Rectangle dirtCenter = new Rectangle(112, 16, 16, 16);
                    Rectangle dirtRight = new Rectangle(128, 16, 16, 16);

                    Rectangle woodTop = new Rectangle(16, 64, 16, 16);
                    Rectangle woodCenter = new Rectangle(16, 80, 16, 16);

                    // --- สร้าง Platforms ---
                    if (tile == '0') platforms.Add(new Platform(rect, TimeState.Permanent, terrainTexture, stoneTop));
                    else if (tile == '8') platforms.Add(new Platform(rect, TimeState.Permanent, terrainTexture, stoneCenter));

                    else if (tile == '[') platforms.Add(new Platform(rect, TimeState.Present, terrainTexture, grassLeft));
                    else if (tile == '1') platforms.Add(new Platform(rect, TimeState.Present, terrainTexture, grassMid));
                    else if (tile == ']') platforms.Add(new Platform(rect, TimeState.Present, terrainTexture, grassRight));
                    else if (tile == '{') platforms.Add(new Platform(rect, TimeState.Present, terrainTexture, dirtLeft));
                    else if (tile == '4') platforms.Add(new Platform(rect, TimeState.Present, terrainTexture, dirtCenter));
                    else if (tile == '}') platforms.Add(new Platform(rect, TimeState.Present, terrainTexture, dirtRight));

                    else if (tile == '2') platforms.Add(new Platform(rect, TimeState.Past, terrainTexture, woodTop));
                    else if (tile == '5') platforms.Add(new Platform(rect, TimeState.Past, terrainTexture, woodCenter));
                    else if (tile == 'X' || tile == 'T' || tile == 't')
                    {
                        int drawWidth = 32;
                        int drawHeight = 64;

                        // 3. จัดตำแหน่งให้อยู่ติดพื้นบล็อกพอดี
                        Rectangle drawRect = new Rectangle(
                            x * tileSize + 4,                     // ขยับแกน X มาตรงกลางนิดนึง
                            y * tileSize + (tileSize - drawHeight), // ดึงแกน Y ลงมาติดพื้นบล็อก
                            drawWidth,
                            drawHeight);

                        int hitboxWidth = 24;
                        int hitboxHeight = 18;
                        Rectangle spikeHitbox = new Rectangle(
                            (x * tileSize) + 4,
                            (y * tileSize) + (tileSize - hitboxHeight),
                            hitboxWidth, hitboxHeight);

                        // ✨ 3. ส่งกล่องทั้ง 2 ใบเข้าไปตอนสร้างหนาม
                        if (tile == 'X') spikes.Add(new Spike(spikeHitbox, drawRect, TimeState.Permanent, spikesTexture));
                        else if (tile == 'T') spikes.Add(new Spike(spikeHitbox, drawRect, TimeState.Present, spikesTexture));
                        else if (tile == 't') spikes.Add(new Spike(spikeHitbox, drawRect, TimeState.Past, spikesTexture));
                    }
                    else if (tile == 'P') player = new Player(new Vector2(x * tileSize, y * tileSize), playerAnimations);
                    else if (tile == 'B') boxes.Add(new Box(new Vector2(x * tileSize, y * tileSize), dummyTexture));
                    else if (tile == 'E')
                    {
                        Vector2 enemyPos = new Vector2(x * tileSize, (y * tileSize) + 32);
                        enemies.Add(new Enemy(enemyPos, dummyTexture));
                    }
                    else if (tile == 'W')
                    {
                        // Normal floor hazard
                        hazards.Add(new Hazard(new Vector2(x * tileSize, y * tileSize), TimeState.Permanent, dummyTexture));
                    }
                    else if (tile == 'M')
                    {
                        // Ceiling hazard (Upside Down)
                        var ceilingHazard = new Hazard(new Vector2(x * tileSize, y * tileSize), TimeState.Permanent, dummyTexture);
                        ceilingHazard.IsUpsideDown = true;
                        hazards.Add(ceilingHazard);
                    }
                    // else if (tile == '3') platforms.Add(new Platform(rect, TimeState.Permanent, dummyTexture, 1));
                    else if (tile == 'A') plates.Add(new PressurePlate(rect, dummyTexture, 1));
                    else if (tile == 'H') itemManager.AddHeart(rect);
                    else if (tile == 'D')
                    {
                        exitDoor = rect;
                        hasExitDoor = true;
                    }
                    else if (tile == 'O' || tile == 'o') // Handle both Normal and 360 modes
                    {
                        // 1. Calculate Positions
                        Vector2 anchorPos = new Vector2(rect.Center.X, rect.Top);
                        Rectangle platformRect = new Rectangle(rect.X, rect.Y, 32, 32); // Standard tile size
                        Rectangle sourceRect = new Rectangle(192, 16, 16, 16); // Your stone/block texture

                        // 2. Create and add the Swinging Hazard
                        if (tile == 'O') // Normal Mode
                        {
                            var mace = new SwingingHazard(anchorPos, 2.0f, 150f, maceTexture, dummyTexture);
                            mace.IsFullRotation = false;
                            swingingHazards.Add(mace);
                        }
                        else if (tile == 'o') // 360 Mode
                        {
                            var mace = new SwingingHazard(anchorPos, 2.5f, 100f, maceTexture, dummyTexture);
                            mace.IsFullRotation = true;
                            swingingHazards.Add(mace);
                        }

                        // 3. ADD THIS: Create a solid platform at the same spot
                        // This makes the block "platformable" using your existing logic
                        platforms.Add(new Platform(platformRect, TimeState.Permanent, terrainTexture, sourceRect));
                    }

                }
            }
        }
        private void LoadLevel()
        {
            // 1. ล้างข้อมูลเก่าทิ้ง
            platforms.Clear();
            spikes.Clear();
            boxes.Clear();
            enemies.Clear();
            swingingHazards.Clear();
            itemManager.hearts.Clear();
            hasExitDoor = false;

            // 2. ดึงแผนที่ทั้ง 2 เลเยอร์มา
            var maps = GetLevelDesign(currentLevel);
            int tileSize = 32;

            // 3. สั่งโหลดแผนที่ทีละชั้นให้มันซ้อนกัน!
            LoadLayer(maps.presentMap, tileSize);
            LoadLayer(maps.pastMap, tileSize);
        }
    }
}