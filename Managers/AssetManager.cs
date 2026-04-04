using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace FinalProject.Managers
{
    public static class AssetManager
    {
        // ==========================================
        // 1. Fonts & UI
        // ==========================================
        public static SpriteFont Font;
        public static Texture2D PlayButtonTex;
        public static Texture2D CloseButtonTex;
        public static Texture2D RestartButtonTex;

        // ==========================================
        // 2. Environment & Terrain
        // ==========================================
        public static Texture2D TerrainTexture;
        public static Texture2D BgBrown;
        public static Texture2D BgGray;

        // ==========================================
        // 3. Hazards & Enemies
        // ==========================================
        public static Texture2D SpikesTexture;
        public static Texture2D SlimeTexture;
        public static Texture2D SpearTexture;
        public static Texture2D MaceTexture;
        public static Texture2D MaceBlockTexture;
        public static Texture2D SawTexture;
        public static Texture2D ChainTexture;

        // ==========================================
        // 4. Items & Objects
        // ==========================================
        public static Texture2D DoubleJumpTexture;
        public static Texture2D DashTexture;
        public static Texture2D WallJumpTexture;
        public static Texture2D BoxTexture;
        public static Texture2D PlateTexture;
        public static Texture2D PortalTexture;
        public static Texture2D MovingPlatformTexture;

        // ==========================================
        // 5. Audio (SFX & BGM)
        // ==========================================
        public static SoundEffect PickupSound;
        public static SoundEffect JumpSound;
        public static SoundEffect ShiftSound;
        public static SoundEffect ExitSound;
        public static SoundEffect DeadSound;
        public static SoundEffect WinSound;
        public static Song Bgm;

        public static void LoadContent(ContentManager content)
        {
            // Fonts
            Font = content.Load<SpriteFont>("GameFont");

            PlayButtonTex = content.Load<Texture2D>("UI/Play");
            CloseButtonTex = content.Load<Texture2D>("UI/Close");
            RestartButtonTex = content.Load<Texture2D>("UI/Restart");

            // Sceern
            TerrainTexture = content.Load<Texture2D>("Terrain/Terrain");
            BgBrown = content.Load<Texture2D>("Terrain/Brown");
            BgGray = content.Load<Texture2D>("Terrain/Gray");

            // Trap
            SpikesTexture = content.Load<Texture2D>("Spike/Idle");
            MaceTexture = content.Load<Texture2D>("Spike/Spiked Ball");
            MaceBlockTexture = content.Load<Texture2D>("Spike/Block");
            SlimeTexture = content.Load<Texture2D>("Enemy/Slime");
            SawTexture = content.Load<Texture2D>("Enemy/Saw");
            ChainTexture = content.Load<Texture2D>("Enemy/Chain");

            // Item
            DoubleJumpTexture = content.Load<Texture2D>("Item/wing");
            DashTexture = content.Load<Texture2D>("Item/Veil");
            WallJumpTexture = content.Load<Texture2D>("Item/shoe");
            BoxTexture = content.Load<Texture2D>("Item/Box");
            PlateTexture = content.Load<Texture2D>("Item/plate");
            PortalTexture = content.Load<Texture2D>("Item/exit_door");
            SpearTexture = content.Load<Texture2D>("Item/Spear");
            MovingPlatformTexture = content.Load<Texture2D>("Item/movingPlatform");

            // Sound
            PickupSound = content.Load<SoundEffect>("Sound/pickup");
            JumpSound = content.Load<SoundEffect>("Sound/jump");
            ShiftSound = content.Load<SoundEffect>("Sound/swift");
            ExitSound = content.Load<SoundEffect>("Sound/enterExit");
            DeadSound = content.Load<SoundEffect>("Sound/dead");
            WinSound = content.Load<SoundEffect>("Sound/win");
            Bgm = content.Load<Song>("Sound/bgm");
        }
    }
}