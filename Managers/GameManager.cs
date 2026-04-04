using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
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
        public List<SawHazard> saws;
        public TimeState currentTime;
        private Texture2D dummyTexture;
        private float bgScrollX = 0f;
        private float bgScrollY = 0f;

        public ItemManager itemManager;
        public UIManager uiManager;
        public ParticleManager particleManager;
        public VFXManager vfxManager;

        private Dictionary<Player.PlayerState, Animation> playerAnimations;

        public int currentLevel = 1;
        public int maxLevel = 5;

        public int MapWidth { get; private set; }
        public int MapHeight { get; private set; }

        public ExitDoor exitDoor;
        public bool hasExitDoor;


        public enum GameState { MainMenu, Playing, GameWon }
        public GameState State = GameState.MainMenu;
        public int totalFailed = 0;
        public float totalPlayTime = 0f;
        public bool IsExiting = false;

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
            swingingHazards = new List<SwingingHazard>();
            saws = new List<SawHazard>();
            currentTime = TimeState.Present;
        }

        public void LoadContent(ContentManager content, Texture2D dummy)
        {
            dummyTexture = dummy;
            AssetManager.LoadContent(content);

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.2f;
            MediaPlayer.Play(AssetManager.Bgm);

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

            itemManager = new ItemManager(AssetManager.PickupSound);
            uiManager = new UIManager(AssetManager.Font, content, dummyTexture.GraphicsDevice); particleManager = new ParticleManager();
            vfxManager = new VFXManager(particleManager, dummyTexture);
            itemManager.powerUpTextures[ItemManager.ItemType.DoubleJump] = AssetManager.DoubleJumpTexture;
            itemManager.powerUpTextures[ItemManager.ItemType.Dash] = AssetManager.DashTexture;
            itemManager.powerUpTextures[ItemManager.ItemType.WallJump] = AssetManager.WallJumpTexture;

            LoadLevel();
        }

        public void Update(GameTime gameTime)
        {

            if (MediaPlayer.State == MediaState.Playing)
            {
                if (MediaPlayer.PlayPosition.TotalSeconds >= 36.0)
                {
                    MediaPlayer.Play(AssetManager.Bgm);
                }
            }

            MouseState mouseState = Mouse.GetState();
            bgScrollX = 0f;
            bgScrollY += 0.5f;

            if (State == GameState.MainMenu)
            {
                if (uiManager.IsStartClicked(mouseState))
                {
                    totalFailed = 0;
                    totalPlayTime = 0f;
                    currentLevel = 1;
                    LoadLevel();
                    State = GameState.Playing;
                }
                if (uiManager.IsExitClicked(mouseState))
                {
                    IsExiting = true;
                }
                return;
            }
            else if (State == GameState.GameWon)
            {
                if (uiManager.IsRestartClicked(mouseState))
                {
                    totalFailed = 0;
                    totalPlayTime = 0f;
                    currentLevel = 1;
                    LoadLevel();
                    State = GameState.Playing;
                }
                if (uiManager.IsBackClicked(mouseState))
                {
                    State = GameState.MainMenu;
                }
                return;
            }
            if (State == GameState.Playing && player != null && !player.IsDead)
            {
                totalPlayTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }



            if (InputManager.IsKeyPressed(Keys.LeftControl))
            {
                currentTime = (currentTime == TimeState.Present) ? TimeState.Past : TimeState.Present;
                AssetManager.ShiftSound.Play(0.5f, 0f, 0f);
            }

            foreach (var platform in platforms) platform.Update(currentTime, plates, gameTime); ;
            foreach (var spike in spikes) spike.Update(currentTime);
            foreach (var hazard in hazards) hazard.Update(currentTime, gameTime);
            foreach (var box in boxes) box.Update(platforms, player);
            foreach (var enemy in enemies) enemy.Update(gameTime, platforms, boxes, currentTime);
            foreach (var plate in plates) plate.Update(player, boxes);

            player.Update(platforms, boxes, particleManager);
            player.CheckSpikeCollision(spikes);
            itemManager.Update(player, vfxManager);
            particleManager.Update();


            if (hasExitDoor)
            {
                exitDoor.Update(gameTime);
            }

            foreach (var enemy in enemies)
            {
                if (enemy.IsDangerous && player.Hitbox.Intersects(enemy.Hitbox))
                {
                    int pushDir = (player.Position.X < enemy.Position.X) ? -1 : 1;
                    player.TakeDamage(pushDir);
                }
            }
            foreach (var saw in saws)
            {
                saw.Update(currentTime, gameTime);
                if (saw.IsDangerous && player.Hitbox.Intersects(saw.Hitbox))
                {
                    int pushDir = (player.Position.X < saw.Position.X) ? -1 : 1;
                    player.TakeDamage(pushDir);
                }
            }

            foreach (var hazard in hazards)
            {
                if (hazard.IsDangerous && player.Hitbox.Intersects(hazard.Hitbox))
                {
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

            if (player.Position.Y > MapHeight + 200)
            {
                player.Die();
            }

            if (player.IsDead)
            {
                if (InputManager.IsKeyPressed(Keys.Enter))
                {
                    totalFailed++;
                    LoadLevel();
                }
                return;
            }

            if (InputManager.IsKeyPressed(Keys.R) && State == GameState.Playing)
            {
                totalFailed++;
                LoadLevel();
                return;
            }


            if (hasExitDoor && player.Hitbox.Intersects(exitDoor.Hitbox))
            {

                if (currentLevel < maxLevel)
                {
                    AssetManager.ExitSound.Play(0.2f, 0f, 0f);
                    currentLevel++;
                    LoadLevel();
                }
                else
                {
                    AssetManager.WinSound.Play(0.5f, 0f, 0f);
                    State = GameState.GameWon;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var platform in platforms) platform.Draw(spriteBatch);
            foreach (var spike in spikes) spike.Draw(spriteBatch);
            foreach (var box in boxes) box.Draw(spriteBatch);
            foreach (var enemy in enemies) enemy.Draw(spriteBatch, currentTime);
            foreach (var hazard in hazards) hazard.Draw(spriteBatch);
            foreach (var mace in swingingHazards) mace.Draw(spriteBatch);
            foreach (var plate in plates) plate.Draw(spriteBatch);
            foreach (var mace in swingingHazards) mace.Draw(spriteBatch);
            foreach (var saw in saws) saw.Draw(spriteBatch);
            if (hasExitDoor) exitDoor.Draw(spriteBatch);
            itemManager.Draw(spriteBatch);
            particleManager.Draw(spriteBatch);
            player.Draw(spriteBatch);
        }

        public void DrawBackground(SpriteBatch spriteBatch)
        {
            Texture2D currentBg;
            if (State == GameState.MainMenu)
            {
                currentBg = AssetManager.BgBrown;
            }
            else
            {
                currentBg = (currentTime == TimeState.Present) ? AssetManager.BgGray : AssetManager.BgBrown;
            }

            int bgScale = 2;
            int drawWidth = currentBg.Width * bgScale;
            int drawHeight = currentBg.Height * bgScale;

            int offsetX = (int)(Math.Abs(bgScrollX) % drawWidth) * Math.Sign(bgScrollX);
            int offsetY = (int)(Math.Abs(bgScrollY) % drawHeight) * Math.Sign(bgScrollY);

            for (int y = -drawHeight; y < MapHeight + 1000; y += drawHeight)
            {
                for (int x = -drawWidth; x < MapWidth + 2000; x += drawWidth)
                {
                    spriteBatch.Draw(currentBg, new Rectangle(x + offsetX, y + offsetY, drawWidth, drawHeight), Color.White);
                }
            }
        }


        private void LoadLayer(string[] levelLayer, int tileSize)
        {
            for (int y = 0; y < levelLayer.Length; y++)
            {
                for (int x = 0; x < levelLayer[y].Length; x++)
                {
                    char tile = levelLayer[y][x];
                    if (tile == '.') continue;

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

                    Rectangle copperPlate = new Rectangle(192, 145, 16, 16);
                    Rectangle goldPlate = new Rectangle(272, 145, 16, 16);

                    if (tile == '0') platforms.Add(new Platform(rect, TimeState.Permanent, AssetManager.TerrainTexture, stoneTop));
                    else if (tile == '8') platforms.Add(new Platform(rect, TimeState.Permanent, AssetManager.TerrainTexture, stoneCenter));

                    else if (tile == '[') platforms.Add(new Platform(rect, TimeState.Present, AssetManager.TerrainTexture, grassLeft));
                    else if (tile == '1') platforms.Add(new Platform(rect, TimeState.Present, AssetManager.TerrainTexture, grassMid));
                    else if (tile == ']') platforms.Add(new Platform(rect, TimeState.Present, AssetManager.TerrainTexture, grassRight));
                    else if (tile == '{') platforms.Add(new Platform(rect, TimeState.Present, AssetManager.TerrainTexture, dirtLeft));
                    else if (tile == '4') platforms.Add(new Platform(rect, TimeState.Present, AssetManager.TerrainTexture, dirtCenter));
                    else if (tile == '}') platforms.Add(new Platform(rect, TimeState.Present, AssetManager.TerrainTexture, dirtRight));

                    else if (tile == '2') platforms.Add(new Platform(rect, TimeState.Past, AssetManager.TerrainTexture, woodTop));
                    else if (tile == '5') platforms.Add(new Platform(rect, TimeState.Past, AssetManager.TerrainTexture, woodCenter));
                    else if (tile == 'X' || tile == 'T' || tile == 't')
                    {
                        int drawWidth = 32;
                        int drawHeight = 64;

                        Rectangle drawRect = new Rectangle(
                            x * tileSize + 4,
                            y * tileSize + (tileSize - drawHeight),
                            drawWidth,
                            drawHeight);

                        int hitboxWidth = 32;
                        int hitboxHeight = 32;
                        Rectangle spikeHitbox = new Rectangle(
                            (x * tileSize) + 4,
                            (y * tileSize) + (tileSize - hitboxHeight),
                            hitboxWidth, hitboxHeight);

                        if (tile == 'X') spikes.Add(new Spike(spikeHitbox, drawRect, TimeState.Permanent, AssetManager.SpikesTexture));
                        else if (tile == 'T') spikes.Add(new Spike(spikeHitbox, drawRect, TimeState.Present, AssetManager.SpikesTexture));
                        else if (tile == 't') spikes.Add(new Spike(spikeHitbox, drawRect, TimeState.Past, AssetManager.SpikesTexture));
                    }
                    else if (tile == '<' || tile == '>' || tile == '^')
                    {
                        int drawWidth = 32;
                        int drawHeight = 32;

                        int hitboxWidth = 24;
                        int hitboxHeight = 18;

                        Rectangle spikeHitbox;
                        float rotation = 0f;

                        Vector2 origin = new Vector2(AssetManager.SpikesTexture.Width / 2f, AssetManager.SpikesTexture.Height / 2f);

                        Rectangle drawRect = new Rectangle(
                            (x * tileSize) + (tileSize / 2),
                            (y * tileSize) + (tileSize / 2),
                            drawWidth,
                            drawHeight);

                        if (tile == '>')
                        {
                            rotation = MathHelper.PiOver2;
                            spikeHitbox = new Rectangle((x * tileSize), (y * tileSize) + (tileSize - hitboxHeight) / 2, hitboxWidth, hitboxHeight);
                        }
                        else
                        {
                            rotation = -MathHelper.PiOver2;
                            spikeHitbox = new Rectangle((x * tileSize) + (tileSize - hitboxWidth), (y * tileSize) + (tileSize - hitboxHeight) / 2, hitboxWidth, hitboxHeight);
                        }

                        TimeState state = (tile == '^') ? TimeState.Past : TimeState.Permanent;

                        spikes.Add(new Spike(spikeHitbox, drawRect, state, AssetManager.SpikesTexture, rotation, origin));
                    }
                    else if (tile == '=' || tile == '+' || tile == '-' || tile == '*')
                    {
                        int platWidth = 40 * 3;
                        Rectangle movingRect = new Rectangle(x * tileSize, y * tileSize + (tileSize - 10), platWidth, 20);
                        Rectangle sourceRect = new Rectangle(0, 0, 32, 10);

                        TimeState platState = tile switch
                        {
                            '=' or '-' => TimeState.Present,
                            '+' or '*' => TimeState.Past,
                            _ => TimeState.Permanent
                        };

                        float distanceX = tile switch
                        {
                            '=' or '+' => 8 * tileSize,
                            '-' or '*' => -8 * tileSize,
                            _ => 0
                        };

                        float speed = 3f;

                        Platform movingPlat = new Platform(movingRect, platState, AssetManager.MovingPlatformTexture, sourceRect);
                        movingPlat.SetMoving(distanceX, speed);
                        movingPlat.SetAnimation(4, 32, 10, 0.15f);

                        platforms.Add(movingPlat);
                    }
                    else if (tile == 'P') player = new Player(new Vector2(x * tileSize, y * tileSize), playerAnimations, AssetManager.JumpSound, AssetManager.DeadSound);
                    else if (tile == 'B') boxes.Add(new Box(new Vector2(x * tileSize, y * tileSize), AssetManager.BoxTexture));
                    else if (tile == 'E')
                    {
                        Vector2 enemyPos = new Vector2(x * tileSize, (y * tileSize) + 32);
                        enemies.Add(new Enemy(enemyPos, AssetManager.SlimeTexture, TimeState.Present));
                    }
                    else if (tile == 'e')
                    {
                        Vector2 enemyPos = new Vector2(x * tileSize, (y * tileSize) + 32);
                        enemies.Add(new Enemy(enemyPos, AssetManager.SlimeTexture, TimeState.Past));
                    }
                    else if (tile == 'W')
                    {
                        hazards.Add(new Hazard(new Vector2(x * tileSize, y * tileSize), TimeState.Permanent, AssetManager.SpearTexture));
                    }
                    else if (tile == 'M')
                    {
                        var ceilingHazard = new Hazard(new Vector2(x * tileSize, y * tileSize), TimeState.Permanent, AssetManager.SpearTexture);
                        ceilingHazard.IsUpsideDown = true;
                        hazards.Add(ceilingHazard);
                    }
                    else if (tile == 'A') plates.Add(new PressurePlate(rect, AssetManager.PlateTexture, 1));
                    else if (tile == 'a') platforms.Add(new Platform(rect, TimeState.Permanent, AssetManager.TerrainTexture, copperPlate, 1));
                    else if (tile == 'Q') plates.Add(new PressurePlate(rect, AssetManager.PlateTexture, 2));
                    else if (tile == 'q') platforms.Add(new Platform(rect, TimeState.Permanent, AssetManager.TerrainTexture, goldPlate, 2));
                    else if (tile == 'D')
                    {
                        exitDoor = new ExitDoor(rect, AssetManager.PortalTexture);
                        hasExitDoor = true;
                    }
                    else if (tile == 'Z' || tile == 'z' || tile == 'C' || tile == 'c' || tile == 'V' || tile == 'v') // Handle both Normal and 360 modes
                    {
                        Vector2 anchorPos = new Vector2(rect.Center.X, rect.Top);
                        Rectangle platformRect = new Rectangle(rect.X, rect.Y, 32, 32);
                        Rectangle sourceRect = new Rectangle(192, 16, 16, 16);

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
                        if (tile == 'Z' || tile == 'C' || tile == 'V') // 180 
                        {
                            var mace = new SwingingHazard(anchorPos, 2.0f, 125f, AssetManager.MaceTexture, dummyTexture, hazardState);
                            mace.IsFullRotation = false;
                            swingingHazards.Add(mace);
                        }
                        else if (tile == 'z' || tile == 'c' || tile == 'v') // 360 
                        {
                            var mace = new SwingingHazard(anchorPos, 3.5f, 120f, AssetManager.MaceTexture, dummyTexture, hazardState);
                            mace.IsFullRotation = true;
                            swingingHazards.Add(mace);
                        }
                        platforms.Add(new Platform(platformRect, hazardState, AssetManager.TerrainTexture, sourceRect));
                    }

                    else if (tile == 'S' || tile == 's' || tile == 'H' || tile == 'h' || tile == 'G' || tile == 'g' || tile == 'L' || tile == 'l')
                    {
                        Vector2 startPos = new Vector2(x * tileSize - 20, y * tileSize - 20);

                        TimeState hazardState = tile switch
                        {
                            'S' or 'H' or 'G' or 'L' => TimeState.Present,
                            's' or 'h' or 'g' or 'l' => TimeState.Past,
                            _ => TimeState.Permanent
                        };

                        SawHazard saw = new SawHazard(startPos, hazardState, AssetManager.SawTexture, AssetManager.ChainTexture);

                        if (tile == 'H' || tile == 'h')
                        {
                            saw.SetMoving(new Vector2(startPos.X + (6 * tileSize), startPos.Y), 2f);
                        }
                        else if (tile == 'G' || tile == 'g')
                        {
                            saw.SetMoving(new Vector2(startPos.X, startPos.Y + (5 * tileSize)), 2f);
                        }
                        else if (tile == 'L' || tile == 'l')
                        {
                            saw.SetMoving(new Vector2(startPos.X + (8 * tileSize), startPos.Y), 2f);
                        }


                        saws.Add(saw);
                    }
                    else if (tile == 'Y') itemManager.AddPowerUp(rect, ItemManager.ItemType.DoubleJump);
                    else if (tile == 'F') itemManager.AddPowerUp(rect, ItemManager.ItemType.Dash);
                    else if (tile == 'J') itemManager.AddPowerUp(rect, ItemManager.ItemType.WallJump);
                }
            }
        }
        private void LoadLevel()
        {
            platforms.Clear();
            spikes.Clear();
            boxes.Clear();
            enemies.Clear();
            swingingHazards.Clear();
            hasExitDoor = false;
            exitDoor = null;
            plates.Clear();
            hazards.Clear();
            enemies.Clear();
            swingingHazards.Clear();
            saws.Clear();
            itemManager.powerUps.Clear();

            var maps = LevelManager.GetLevelDesign(currentLevel);
            int tileSize = 32;
            MapWidth = maps.presentMap[0].Length * tileSize;
            MapHeight = maps.presentMap.Length * tileSize;

            LoadLayer(maps.presentMap, tileSize);
            LoadLayer(maps.pastMap, tileSize);

            if (player != null)
            {
                player.CanDoubleJump = false;
                player.CanDash = false;
                player.CanWallJump = false;

                if (currentLevel >= 4)
                {
                    player.CanDoubleJump = true;
                }

                if (currentLevel >= 5)
                {
                    player.CanWallJump = true;
                }

            }
        }
    }
}