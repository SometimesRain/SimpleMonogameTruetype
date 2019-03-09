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
		Effect effect;

		const string englishLoremIpsum =
			"But I must explain to you how all this mistaken idea of denouncing pleasure and praising pain was born " +
			"and I will give you a complete account of the system, and expound the actual teachings of the great explorer " +
			"of the truth, the master-builder of human happiness. No one rejects, dislikes, or avoids pleasure itself, " +
			"because it is pleasure, but because those who do not know how to pursue pleasure rationally encounter " +
			"consequences that are extremely painful. Nor again is there anyone who loves or pursues or desires to obtain " +
			"pain of itself, because it is pain, but because occasionally circumstances occur in which toil and pain can " +
			"procure him some great pleasure. To take a trivial example, which of us ever undertakes laborious physical " +
			"exercise, except to obtain some advantage from it? But who has any right to find fault with a man who chooses " +
			"to enjoy a pleasure that has no annoying consequences, or one who avoids a pain that produces no resultant pleasure?";

		Font font;
		BitmapData data;
		Texture2D fontTexture;

		public Game1(int width, int height)
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = width;
			graphics.PreferredBackBufferWidth = height;
			Content.RootDirectory = "Content";

			//Pixel shaders are only supported on HiDef profile
			graphics.GraphicsProfile = GraphicsProfile.HiDef;

			//Allow resizing and set the resize handle
			Window.AllowUserResizing = true;
			Window.ClientSizeChanged += new EventHandler<EventArgs>(OnResize);

			Window.Title = "Try resizing the window";
			IsMouseVisible = true;
		}

		protected void OnResize(object sender, EventArgs e)
		{
			int width = Window.ClientBounds.Width;
			int height = Window.ClientBounds.Height;

			if (width == 0 || height == 0)
				return;

			graphics.PreferredBackBufferWidth = width;
			graphics.PreferredBackBufferHeight = height;
			graphics.ApplyChanges();

			RenderText(width);
		}

		//Render again only when size is changed
		private void RenderText(int width)
		{
			//Rasterize the font at size 12pt
			data = font.GenerateBitmapData(englishLoremIpsum, Font.PointsToPixels(12), width - 10);

			//Dispose the old texture
			if (fontTexture != null)
				fontTexture.Dispose();

			//Create a texture to hold the rendered string
			//  SurfaceFormat must be Alpha8
			fontTexture = new Texture2D(GraphicsDevice, data.Width, data.Height, false, SurfaceFormat.Alpha8);

			//Set texture data
			fontTexture.SetData(data.Alphas);
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
			font = new Font("Arial");
			Console.WriteLine("Loaded " + font.Name);
			RenderText(Window.ClientBounds.Width);
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
			GraphicsDevice.Clear(Color.White);

			//Draw the font texture with our pixel shader
			//  If you need to draw something else as well, you might need another Begin() ... End() -block
			spriteBatch.Begin(effect: effect);
			spriteBatch.Draw(fontTexture, new Vector2(5, 5), new Color(0, 0, 0, 255));
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
