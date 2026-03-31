using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class ItemManager
{
    public List<Item> hearts;
    private Texture2D heartTexture;

    public ItemManager(Texture2D heartTex)
    {
        hearts = new List<Item>();
        heartTexture = heartTex;
    }
    public void AddHeart(Rectangle rec)
    {
        hearts.Add(new Item(rec, heartTexture));
    }

    public void Update(Player player)
    {
        for (int i = hearts.Count - 1; i >= 0; i--)
        {
            if (hearts[i].Hitbox.Intersects(player.Hitbox))
            {
                player.Heal(1);
                hearts.RemoveAt(i);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var heart in hearts)
        {
            heart.Draw(spriteBatch);
        }
    }

}