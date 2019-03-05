using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SimpleMonogameTruetype.Example
{
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		Font arial;
		Texture2D fontTexture;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);

			//Load font
			//  Font can be loaded from a file path (supports .ttf and .otf formats)
			//  or if it's an installed font, by specifying its name
			arial = new Font("Arial");

			//Rasterize the font
			BitmapData data = arial.GenerateBitmapData("Hello, MonoGame!", 64);

			//Create a texture to hold the rendered string
			//  SurfaceFormat must be Alpha8
			fontTexture = new Texture2D(GraphicsDevice, data.Width, data.Height, false, SurfaceFormat.Alpha8);

			//Set texture data
			fontTexture.SetData(data.Alphas);
		}

		protected override void UnloadContent()
		{
			if (fontTexture != null)
				fontTexture.Dispose();

			//Free all unmanaged resources
			Font.FreeAllResources();
		}

		protected override void Update(GameTime gameTime)
		{
			if (Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			//Draw the font texture
			//  Note that only the alpha component of the color value is taken into account
			//  If you want to change the color of the text you need to write your own pixel shader
			spriteBatch.Begin();
			spriteBatch.Draw(fontTexture, new Vector2(100, 100), new Color(0, 0, 0, 255));
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
