using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

public class ItemManager
{
    public List<Item> hearts;
    public List<PowerUpItem> powerUps = new List<PowerUpItem>();
    public Dictionary<ItemType, Texture2D> powerUpTextures = new Dictionary<ItemType, Texture2D>();
    private Texture2D heartTexture;
    private SoundEffect pickupSound;
    public enum ItemType { Heart, DoubleJump, Dash, WallJump }

    public class PowerUpItem
    {
        public Rectangle Hitbox;
        public ItemType Type;
        public bool IsCollected = false;

        public PowerUpItem(Rectangle hitbox, ItemType type)
        {
            Hitbox = hitbox;
            Type = type;
        }


    }
    public ItemManager(Texture2D heartTex, SoundEffect sound)
    {
        hearts = new List<Item>();
        heartTexture = heartTex;
        pickupSound = sound;
    }

    public void AddHeart(Rectangle rec)
    {
        hearts.Add(new Item(rec, heartTexture));
    }

    public void Update(Player player, FinalProject.Managers.VFXManager vfxManager)
    {
        for (int i = hearts.Count - 1; i >= 0; i--)
        {
            if (hearts[i].Hitbox.Intersects(player.Hitbox))
            {
                player.Heal(1);

                vfxManager.CreateItemPickupEffect(hearts[i].Hitbox, Color.Red);
                pickupSound.Play();

                hearts.RemoveAt(i);
            }
        }
        foreach (var item in powerUps)
        {
            if (!item.IsCollected && player.Hitbox.Intersects(item.Hitbox))
            {
                item.IsCollected = true;

                vfxManager.CreateItemPickupEffect(item.Hitbox, Color.Gold);
                pickupSound.Play(0.2f, 0f, 0f);

                if (item.Type == ItemType.DoubleJump) player.CanDoubleJump = true;
                if (item.Type == ItemType.Dash) player.CanDash = true;
                if (item.Type == ItemType.WallJump) player.CanWallJump = true;
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var heart in hearts)
        {
            heart.Draw(spriteBatch);
        }

        foreach (var item in powerUps)
        {
            if (!item.IsCollected)
            {
                if (powerUpTextures.ContainsKey(item.Type))
                    spriteBatch.Draw(powerUpTextures[item.Type], item.Hitbox, Color.White);
                else
                    spriteBatch.Draw(heartTexture, item.Hitbox, Color.Gold);
            }
        }
    }
    public void AddPowerUp(Rectangle rect, ItemType type)
    {
        powerUps.Add(new PowerUpItem(rect, type));
    }

}