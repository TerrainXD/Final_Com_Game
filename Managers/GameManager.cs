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
        private Texture2D doubleJumpTexture;
        private Texture2D dashTexture;
        private Texture2D wallJumpTexture;
        private Texture2D boxTexture;

        private Texture2D spearTexture;
        private Texture2D maceTexture;
        private Texture2D maceBlockTexture;
        private Texture2D plateTexture;

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

        public int MapWidth { get; private set; }
        public int MapHeight { get; private set; }

        public ExitDoor exitDoor;
        public bool hasExitDoor;

        private Texture2D portalTexture;

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
            doubleJumpTexture = content.Load<Texture2D>("Item/wing");
            dashTexture = content.Load<Texture2D>("Item/Veil");
            wallJumpTexture = content.Load<Texture2D>("Item/shoe");
            boxTexture = content.Load<Texture2D>("Item/Box");
            plateTexture = content.Load<Texture2D>("Item/plate");
            portalTexture = content.Load<Texture2D>("Item/exit_door");
            spearTexture = content.Load<Texture2D>("Item/Spear");


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
            itemManager.powerUpTextures[ItemManager.ItemType.DoubleJump] = doubleJumpTexture;
            itemManager.powerUpTextures[ItemManager.ItemType.Dash] = dashTexture;
            itemManager.powerUpTextures[ItemManager.ItemType.WallJump] = wallJumpTexture;
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
            foreach (var hazard in hazards) hazard.Update(currentTime, gameTime);
            foreach (var box in boxes) box.Update(platforms, player);
            foreach (var enemy in enemies) enemy.Update(platforms, boxes, currentTime);
            foreach (var plate in plates) plate.Update(player, boxes);

            player.Update(platforms, boxes, particleManager);
            player.CheckSpikeCollision(spikes);
            itemManager.Update(player);
            particleManager.Update();

            if (hasExitDoor)
            {
                exitDoor.Update(gameTime);
            }
            if (hasExitDoor && player.Hitbox.Intersects(exitDoor.Hitbox))
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
                mace.Update(gameTime, currentTime);
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

            if (hasExitDoor && player.Hitbox.Intersects(exitDoor.Hitbox))
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
            if (hasExitDoor) exitDoor.Draw(spriteBatch);
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
                    string[] present = new string[]
                    {
                        "000000000000000000000000000000000000000000000000000000000000",
                        // "8..........................................................8",
                        // "8..........................................................8",
                        // "8..........................................................8",
                        // "8..........................................................8",
                        // "8..........................................................8",
                        "8..............M...........................................8",
                        "8..........................................................8",
                        "8..........................................................8",
                        "8..........................................................8",
                        "8..........................................................8",
                        "8.........P................................................8",
                        "8..........................................................8",
                        "8.....E...z...........................G....................8",
                        "8.................................B...F....................8",
                        "8.[111]............[11]...............J....................8",
                        "8.{444}............{44}..........[11].A....................8",
                        "8.{444}............{44}..........{44[11]...................8",
                        "8.........z........{44}.......[11]44444}...................8",
                        "8..................{44}.......{44444444}...................8",
                        "8..................{44}................B...................8",
                        "8[1]...............{44}....................................8",
                        "8{4}...............{44}.......[11111111]...........[111111]8",
                        "8{4}...............{44}.......{44444444}...........{444444}8",
                        "8{4}...........[11]{44}....................................8",
                        "8{4[1]TTTTTTTTT{444444}XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX8",
                        "800000000000000000000000000000000000000000000000000000000008",
                        "888888888888888888888888888888888888888888888888888888888888",
                        "888888888888888888888888888888888888888888888888888888888888",
                        "888888888888888888888888888888888888888888888888888888888888"
                    };

                    // 🌌 มิติอดีต (กว้าง 60 x สูง 30 ให้เท่ากับข้างบนเป๊ะๆ)
                    string[] past = new string[]
                    {
                        "000000000000000000000000000000000000000000000000000000000000",
                        // "8..........................................................8",
                        // "8..........................................................8",
                        // "8..........................................................8",
                        // "8..........................................................8",
                        // "8..........................................................8",
                        "8..........................................................8",
                        "8..........................................................8",
                        "8..........................................................8",
                        "8..........................................................8",
                        "8..........................................................8",
                        "8..........................................................8",
                        "8..........................................................8",
                        "8.........c................................................8",
                        "8..........................................................8",
                        "8..................2222....................................8",
                        "8..................5555..........2222......................8",
                        "8..................5555..........5552222...................8",
                        "8.........c........5555............55555...................8",
                        "8..................5555....................................8",
                        "8..............22225555....................................8",
                        "8..............55555555....................................8",
                        "8..............55555555.......2222222222...................8",
                        "8..............55555555.......5555555555...................8",
                        "8.....222......55555555....................................8",
                        "8222tt555tttttt55555555XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX8",
                        "800000000000000000000000000000000000000000000000000000000008",
                        "888888888888888888888888888888888888888888888888888888888888",
                        "888888888888888888888888888888888888888888888888888888888888",
                        "888888888888888888888888888888888888888888888888888888888888"
                    };
                    // string[] present = new string[]
                    // {
                    //     "000000000000000000000000000000000000000000000000000000000000",
                    //     // "8..........................................................8",
                    //     // "8..........................................................8",
                    //     // "8..........................................................8",
                    //     // "8..........................................................8",
                    //     // "8..........................................................8",
                    //     "8{44}........{44}..............................................8",
                    //     "8{44}........{44}..............................................8",
                    //     "8{44}........{44}..............................................8",
                    //     "8{44}........{44}..............................................8",
                    //     "8{44}........{44}..............................................8",
                    //     "8{44}........{44}..............................................8",
                    //     "8{44}........{44}..............................................8",
                    //     "8{44}........{44}..............................................8",
                    //     "8{44}........{44}..............................................8",
                    //     "8{44}........{44}..............................................8",
                    //     "8{44}........{44}..............................................8",
                    //     "8{44}........{44}..............................................8",
                    //     "8............{44}..............................................8",
                    //     "8............{44}..............................................8",
                    //     "8..........[11]4}..............................................8",
                    //     "8..........{4444}..............................................8",
                    //     "8............{44}..............................................8",
                    //     "8P...........{44}..............................................8",
                    //     "8[11111111111{44}11111111111111111111111111111111111111111]8",
                    //     "8{44444444444444444444444444444444444444444444444444444444}8",
                    //     "888888888888888888888888888888888888888888888888888888888888",
                    //     "888888888888888888888888888888888888888888888888888888888888",
                    //     "888888888888888888888888888888888888888888888888888888888888",
                    //     "888888888888888888888888888888888888888888888888888888888888"
                    // };
                    // string[] past = new string[]
                    // {
                    //     "000000000000000000000000000000000000000000000000000000000000",
                    //     // "8..........................................................8",
                    //     // "8..........................................................8",
                    //     // "8..........................................................8",
                    //     // "8..........................................................8",
                    //     // "8..........................................................8",
                    //     "85555......................................................8",
                    //     "85555......................................................8",
                    //     "85555......................................................8",
                    //     "85555......................................................8",
                    //     "85555......................................................8",
                    //     "85555......................................................8",
                    //     "85555......................................................8",
                    //     "85555......................................................8",
                    //     "85555......................................................8",
                    //     "85555......................................................8",
                    //     "85555......................................................8",
                    //     "85555......................................................8",
                    //     "8..........................................................8",
                    //     "8..........................................................8",
                    //     "8..........................................................8",
                    //     "8..........................................................8",
                    //     "8..........................................................8",
                    //     "8..........................................................8",
                    //     "822222222222222222222222222222222222222222222222222222222228",
                    //     "855555555555555555555555555555555555555555555555555555555558",
                    //     "888888888888888888888888888888888888888888888888888888888888",
                    //     "888888888888888888888888888888888888888888888888888888888888",
                    //     "888888888888888888888888888888888888888888888888888888888888",
                    //     "888888888888888888888888888888888888888888888888888888888888"
                    // };
                    return (present, past);

                default:
                    // ด่าน 2 กันเหนียว (สามารถแก้เพิ่มเองได้เลย)
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
                    else if (tile == 'B') boxes.Add(new Box(new Vector2(x * tileSize, y * tileSize), boxTexture));
                    else if (tile == 'E') // Present Enemy
                    {
                        Vector2 enemyPos = new Vector2(x * tileSize, (y * tileSize) + 32);
                        enemies.Add(new Enemy(enemyPos, dummyTexture, TimeState.Present)); 
                    }
                    else if (tile == 'e') // Past Enemy
                    {
                        Vector2 enemyPos = new Vector2(x * tileSize, (y * tileSize) + 32);
                        enemies.Add(new Enemy(enemyPos, dummyTexture, TimeState.Past)); 
                    }
                    else if (tile == 'W')
                    {
                        // Normal floor hazard
                        hazards.Add(new Hazard(new Vector2(x * tileSize, y * tileSize), TimeState.Permanent, spearTexture));
                    }
                    else if (tile == 'M')
                    {
                        // Ceiling hazard (Upside Down)
                        var ceilingHazard = new Hazard(new Vector2(x * tileSize, y * tileSize), TimeState.Permanent, spearTexture);
                        ceilingHazard.IsUpsideDown = true;
                        hazards.Add(ceilingHazard);
                    }
                    else if (tile == 'A') plates.Add(new PressurePlate(rect, plateTexture, 1));
                    // else if (tile == '3') platforms.Add(new Platform(rect, TimeState.Permanent, dummyTexture, 1));
                    // else if (tile == 'A') plates.Add(new PressurePlate(rect, dummyTexture, 1));
                    else if (tile == 'H') itemManager.AddHeart(rect);
                    else if (tile == 'D')
                    {
                        exitDoor = new ExitDoor(rect, portalTexture);
                        hasExitDoor = true;
                    }
                    else if (tile == 'Z' || tile == 'z' || tile == 'C' || tile == 'c' || tile == 'V' || tile == 'v') // Handle both Normal and 360 modes
                    {
                        // 1. Calculate Positions
                        Vector2 anchorPos = new Vector2(rect.Center.X, rect.Top);
                        Rectangle platformRect = new Rectangle(rect.X, rect.Y, 32, 32); // Standard tile size
                        Rectangle sourceRect = new Rectangle(192, 16, 16, 16); // Your stone/block texture

                        TimeState hazardState;
                        switch (tile)
                        {
                            case 'Z' or 'z':
                                hazardState = TimeState.Present;
                                break;
                            case 'C' or 'c':
                                hazardState = TimeState.Past;
                                break;
                            default:
                                hazardState = TimeState.Permanent;
                                break;
                        }

                        // Z 180 present z 360 present
                        // C 180 past c 360 past
                        // V 180 permanent v 360 permanent
                        // 2. Create and add the Swinging Hazard
                        if (tile == 'Z' || tile == 'C' || tile == 'V') // Normal Mode
                        {
                            var mace = new SwingingHazard(anchorPos, 2.0f, 125f, maceTexture, dummyTexture, hazardState);
                            mace.IsFullRotation = false;
                            swingingHazards.Add(mace);
                        }
                        else if (tile == 'z' || tile == 'c' || tile == 'v') // 360 Mode
                        {
                            var mace = new SwingingHazard(anchorPos, 3.0f, 100f, maceTexture, dummyTexture, hazardState);
                            mace.IsFullRotation = true;
                            swingingHazards.Add(mace);
                        }
                        // 3. ADD THIS: Create a solid platform at the same spot
                        // This makes the block "platformable" using your existing logic
                        platforms.Add(new Platform(platformRect, hazardState, terrainTexture, sourceRect));
                    }
                    else if (tile == 'G') itemManager.AddPowerUp(rect, ItemManager.ItemType.DoubleJump);
                    else if (tile == 'F') itemManager.AddPowerUp(rect, ItemManager.ItemType.Dash);
                    else if (tile == 'J') itemManager.AddPowerUp(rect, ItemManager.ItemType.WallJump);
                }
            }
        }
        private void LoadLevel()
        {
            bool savedDJ = (player != null) ? player.CanDoubleJump : false;
            bool savedDash = (player != null) ? player.CanDash : false;
            bool savedWJ = (player != null) ? player.CanWallJump : false;

            // 1. ล้างข้อมูลเก่าทิ้ง
            platforms.Clear();
            spikes.Clear();
            boxes.Clear();
            enemies.Clear();
            swingingHazards.Clear();
            itemManager.hearts.Clear();
            hasExitDoor = false;
            exitDoor = null;
            plates.Clear();
            hazards.Clear();

            // 2. ดึงแผนที่ทั้ง 2 เลเยอร์มา
            var maps = GetLevelDesign(currentLevel);
            int tileSize = 32;
            MapWidth = maps.presentMap[0].Length * tileSize;
            MapHeight = maps.presentMap.Length * tileSize;

            // 3. สั่งโหลดแผนที่ทีละชั้นให้มันซ้อนกัน!
            LoadLayer(maps.presentMap, tileSize);
            LoadLayer(maps.pastMap, tileSize);


            player.CanDoubleJump = savedDJ;
            player.CanDash = savedDash;
            player.CanWallJump = savedWJ;
        }
    }
}