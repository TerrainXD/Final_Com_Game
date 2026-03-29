using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
namespace FinalProject;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Player player;
    private List<Platform> platforms;
    private List<Spike> spikes;
    private TimeState currentTime;
    private KeyboardState previousKey;
    private Texture2D dummyTexture; // ไว้สร้างภาพกล่องสี่เหลี่ยมสีขาวชั่วคราว
    private Camera camera;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 720;

        _graphics.ApplyChanges();
    }

    private void LoadLevel()
    {
        string[] levelDesign = new string[]
        {
        "..................................................",
        "..................................................",
        "..................................................",
        "...............................2222...............",
        "..................................................",
        "....P...............S...................111.......",
        "..................2222..........2222..............",
        "00000000...22222.........1111.................1111",
        "00000000...........SSS............SSSS............",
        "00000000000000000000000000000000000000000000000000"
    };

        int tileSize = 64;

        for (int y = 0; y < levelDesign.Length; y++)
        {
            for (int x = 0; x < levelDesign[y].Length; x++)
            {
                char tile = levelDesign[y][x];
                if (tile == '0')
                {
                    platforms.Add(new Platform(new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), TimeState.Permanent, dummyTexture));
                }
                else if (tile == '1')
                {
                    platforms.Add(new Platform(new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), TimeState.Present, dummyTexture));
                }
                else if (tile == '2')
                {
                    platforms.Add(new Platform(new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), TimeState.Past, dummyTexture));
                }
                else if (tile == 'S')
                {
                    spikes.Add(new Spike(new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize), TimeState.Present, dummyTexture));
                }
                else if (tile == 'P')
                {
                    player = new Player(new Vector2(x * tileSize, y * tileSize), dummyTexture);
                }
            }
        }
    }

    protected override void Initialize()
    {
        currentTime = TimeState.Present;
        platforms = new List<Platform>();
        spikes = new List<Spike>();
        camera = new Camera();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // สร้าง Texture สีขาวชั่วคราวสำหรับแพลตฟอร์มและตัวละคร
        dummyTexture = new Texture2D(GraphicsDevice, 1, 1);
        dummyTexture.SetData(new[] { Color.White });

        // สร้างตัวละคร
        // player = new Player(new Vector2(100, 100), dummyTexture);

        // // สร้างฉากตามคอนเซปต์ที่ให้มา (บางอันอยู่อดีต บางอันอยู่ปัจจุบัน)
        // // พื้นดินหลัก (อยู่ตลอดเวลา)
        // platforms.Add(new Platform(new Rectangle(0, 400, 800, 80), TimeState.Present, dummyTexture));
        // platforms.Add(new Platform(new Rectangle(0, 400, 800, 80), TimeState.Past, dummyTexture));

        // // แพลตฟอร์มลอยฟ้าแบบพัซเซิล (อยู่คนละเวลา)
        // platforms.Add(new Platform(new Rectangle(200, 300, 100, 20), TimeState.Present, dummyTexture)); // ปัจจุบัน
        // platforms.Add(new Platform(new Rectangle(350, 200, 100, 20), TimeState.Past, dummyTexture));    // อดีต

        LoadLevel();
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState currentKey = Keyboard.GetState();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || currentKey.IsKeyDown(Keys.Escape))
            Exit();

        // กด Shift ซ้ายเพื่อสลับเวลา (Time Shift Mechanic)
        if (currentKey.IsKeyDown(Keys.LeftShift) && previousKey.IsKeyUp(Keys.LeftShift))
        {
            currentTime = (currentTime == TimeState.Present) ? TimeState.Past : TimeState.Present;
        }

        // อัปเดตสถานะของแพลตฟอร์มตามเวลาปัจจุบัน
        foreach (var platform in platforms)
        {
            platform.Update(currentTime);
        }

        foreach (var spike in spikes)
        {
            spike.Update(currentTime);
        }

        // อัปเดตตัวละคร
        player.Update(platforms);
        player.CheckSpikeCollision(spikes);

        previousKey = currentKey;
        camera.Follow(player, GraphicsDevice);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // เปลี่ยนสีพื้นหลังตามกาลเวลาเพื่อบอกผู้เล่น
        GraphicsDevice.Clear(currentTime == TimeState.Present ? Color.CornflowerBlue : Color.DarkSlateBlue);

        _spriteBatch.Begin(transformMatrix: camera.Transform);

        // วาดแพลตฟอร์ม
        foreach (var platform in platforms)
        {
            platform.Draw(_spriteBatch);
        }

        foreach (var spike in spikes)
        {
            spike.Draw(_spriteBatch);
        }

        // วาดตัวละคร
        player.Draw(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}