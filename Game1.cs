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
    private TimeState currentTime;
    private KeyboardState previousKey;
    private Texture2D dummyTexture; // ไว้สร้างภาพกล่องสี่เหลี่ยมสีขาวชั่วคราว

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        currentTime = TimeState.Present;
        platforms = new List<Platform>();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // สร้าง Texture สีขาวชั่วคราวสำหรับแพลตฟอร์มและตัวละคร
        dummyTexture = new Texture2D(GraphicsDevice, 1, 1);
        dummyTexture.SetData(new[] { Color.White });

        // สร้างตัวละคร
        player = new Player(new Vector2(100, 100), dummyTexture);

        // สร้างฉากตามคอนเซปต์ที่ให้มา (บางอันอยู่อดีต บางอันอยู่ปัจจุบัน)
        // พื้นดินหลัก (อยู่ตลอดเวลา)
        platforms.Add(new Platform(new Rectangle(0, 400, 800, 80), TimeState.Present, dummyTexture));
        platforms.Add(new Platform(new Rectangle(0, 400, 800, 80), TimeState.Past, dummyTexture));

        // แพลตฟอร์มลอยฟ้าแบบพัซเซิล (อยู่คนละเวลา)
        platforms.Add(new Platform(new Rectangle(200, 300, 100, 20), TimeState.Present, dummyTexture)); // ปัจจุบัน
        platforms.Add(new Platform(new Rectangle(350, 200, 100, 20), TimeState.Past, dummyTexture));    // อดีต
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

        // อัปเดตตัวละคร
        player.Update(platforms);

        previousKey = currentKey;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // เปลี่ยนสีพื้นหลังตามกาลเวลาเพื่อบอกผู้เล่น
        GraphicsDevice.Clear(currentTime == TimeState.Present ? Color.CornflowerBlue : Color.DarkSlateBlue);

        _spriteBatch.Begin();

        // วาดแพลตฟอร์ม
        foreach (var platform in platforms)
        {
            platform.Draw(_spriteBatch);
        }

        // วาดตัวละคร
        player.Draw(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}