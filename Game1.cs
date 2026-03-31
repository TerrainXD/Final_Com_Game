using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
namespace FinalProject;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private SpriteFont font;

    private Player player;
    private List<Platform> platforms;
    private List<Spike> spikes;
    private TimeState currentTime;
    private KeyboardState previousKey;
    private Texture2D dummyTexture;
    private Camera camera;
    private List<Box> boxes;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;

        graphics.ApplyChanges();
    }

    private void LoadLevel()
    {
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
        "0........................1...2.........0", // โซนกำแพงสลับเวลา!
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
                else if (tile == 'B')
                {
                    boxes.Add(new Box(new Vector2(x * tileSize, y * tileSize), dummyTexture));
                }
            }
        }
    }

    protected override void Initialize()
    {
        currentTime = TimeState.Present;
        platforms = new List<Platform>();
        spikes = new List<Spike>();
        boxes = new List<Box>(); // <-- Add this
        camera = new Camera();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        // สร้าง Texture สีขาวชั่วคราวสำหรับแพลตฟอร์มและตัวละคร
        dummyTexture = new Texture2D(GraphicsDevice, 1, 1);
        dummyTexture.SetData(new[] { Color.White });

        font = Content.Load<SpriteFont>("GameFont");

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
        // Inside Update():
        foreach (var platform in platforms) platform.Update(currentTime);
        foreach (var spike in spikes) spike.Update(currentTime);


        // Give the box a reference to the player so it can push them if needed!
        foreach (var box in boxes)
        {
            box.Update(platforms, player);
        }

        // อัปเดตตัวละคร
        player.Update(platforms, boxes);
        player.CheckSpikeCollision(spikes);



        previousKey = currentKey;
        camera.Follow(player, GraphicsDevice);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // เปลี่ยนสีพื้นหลังตามกาลเวลาเพื่อบอกผู้เล่น
        GraphicsDevice.Clear(currentTime == TimeState.Present ? Color.CornflowerBlue : Color.DarkSlateBlue);
        spriteBatch.Begin(transformMatrix: camera.Transform);

        // วาดแพลตฟอร์ม
        foreach (var platform in platforms)
        {
            platform.Draw(spriteBatch);
        }

        foreach (var spike in spikes)
        {
            spike.Draw(spriteBatch);
        }
        foreach (var box in boxes)
        {
            box.Draw(spriteBatch);
        }

        player.Draw(spriteBatch);
        spriteBatch.End();

        spriteBatch.Begin();
        spriteBatch.DrawString(font, "Double Jump Used " + player.isDoubleJump.ToString(),
        new Vector2(20, 20),
        Color.Black
        );



        spriteBatch.End();

        base.Draw(gameTime);
    }
}