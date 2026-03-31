using Microsoft.Xna.Framework.Input;

namespace FinalProject.Managers
{
    public class InputManager
    {
        private static KeyboardState currentKey;
        private static KeyboardState previousKey;

        public static void Update()
        {
            previousKey = currentKey;
            currentKey = Keyboard.GetState();
        }

        public static bool IsKeyDown(Keys key)
        {
            return currentKey.IsKeyDown(key);
        }

        public static bool IsKeyPressed(Keys key)
        {
            return currentKey.IsKeyDown(key) && !previousKey.IsKeyDown(key);
        }
    }
}