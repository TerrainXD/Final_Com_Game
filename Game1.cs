using FinalProject.Managers;
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
    private Texture2D dummyTexture;
    private Camera camera;

    private GameManager gameManager;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
        graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        gameManager = new GameManager();
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
        gameManager.LoadContent(Content, dummyTexture);
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState currentKey = Keyboard.GetState();
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || currentKey.IsKeyDown(Keys.Escape))
            Exit();

        InputManager.Update();
        gameManager.Update(gameTime);
        camera.Follow(gameManager.player, GraphicsDevice);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // เปลี่ยนสีพื้นหลังตามกาลเวลาเพื่อบอกผู้เล่น
        GraphicsDevice.Clear(gameManager.currentTime == TimeState.Present ? Color.CornflowerBlue : Color.DarkSlateBlue);

        spriteBatch.Begin(transformMatrix: camera.Transform);
        gameManager.Draw(spriteBatch);
        spriteBatch.End();

        spriteBatch.Begin();
        gameManager.uiManager.Draw(spriteBatch, gameManager.player, gameManager.currentTime);
        spriteBatch.End();

        base.Draw(gameTime);
    }
}