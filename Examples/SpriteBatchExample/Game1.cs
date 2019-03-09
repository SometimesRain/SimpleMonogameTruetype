using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace SimpleMonogameTruetype.Example
{
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		Font font;
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
			font = new Font("Arial");
			Console.WriteLine("Loaded "+font.Name);

			//Rasterize the font
			BitmapData data = font.GenerateBitmapData("Hello, MonoGame!", 64);

			//Create a texture to hold the rendered string
			//  SurfaceFormat must be Alpha8
			fontTexture = new Texture2D(GraphicsDevice, data.Width, data.Height, false, SurfaceFormat.Alpha8);

			//Set texture data
			fontTexture.SetData(data.Alphas);

			JapaneseExample();
		}

		protected override void UnloadContent()
		{
			fontTexture?.Dispose();
			unicode?.Dispose();

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

			spriteBatch.Draw(unicode, new Vector2(150, 400), new Color(0, 0, 0, 255));
			spriteBatch.End();

			base.Draw(gameTime);
		}

		Texture2D unicode;
		private void JapaneseExample()
		{
			Font yuMincho = new Font("Yu Mincho Demibold");

			BitmapData data = yuMincho.GenerateBitmapData("Unicodeサポート含まれて!", 36);
			unicode = new Texture2D(GraphicsDevice, data.Width, data.Height, false, SurfaceFormat.Alpha8);
			unicode.SetData(data.Alphas);
		}
	}
}
