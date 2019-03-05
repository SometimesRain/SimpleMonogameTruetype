using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SimpleMonogameTruetype.Example
{
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Effect effect;

		Font arial;
		Texture2D fontTexture;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			//Pixel shaders are only supported on HiDef profile
			graphics.GraphicsProfile = GraphicsProfile.HiDef;

			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);

			//Load pixel shader
			//  If this causes an error make sure the graphics profile is set to HiDef
			effect = Content.Load<Effect>("FontPixelShader");

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

			//Draw the font texture with our pixel shader
			//  If you need to draw something else as well, you might need another Begin() ... End() -block
			spriteBatch.Begin(effect: effect);
			spriteBatch.Draw(fontTexture, new Vector2(100, 100), new Color(0, 0, 255, 255));
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
