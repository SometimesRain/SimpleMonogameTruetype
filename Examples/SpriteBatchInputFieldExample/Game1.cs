using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SimpleMonogameTruetype.Example
{
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		Font candara;
		BitmapData data;
		string input = "";
		Texture2D fontTexture;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			//Set input handle
			Window.TextInput += TextInputHandler;

			IsMouseVisible = true;
		}

		protected void TextInputHandler(object sender, TextInputEventArgs args)
		{
			char c = args.Character;

			//Backspace
			if (c == 8)
				input = input.Substring(0, MathHelper.Max(input.Length - 1, 0));
			//Enter
			else if (c == 13)
				input += '\n';
			//Problem character
			else if (c == 49)
				input += 'Å';
			//Visible character
			else if (!char.IsControl(c))
				input += c;

			RenderText();
		}

		//Render again only when text is changed
		private void RenderText()
		{
			//Dispose the old texture
			if (fontTexture != null)
				fontTexture.Dispose();

			//No text to render
			if (input == "")
				return;

			//Rasterize the font at size 48pt
			data = candara.GenerateBitmapData(input, Font.PointsToPixels(48));

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

			//Load font
			//  Font can be loaded from a file path (supports .ttf and .otf formats)
			//  or if it's an installed font, by specifying its name
			candara = new Font("Candara");

			RenderInstructions();
			RenderText();
		}

		protected override void UnloadContent()
		{
			if (fontTexture != null)
				fontTexture.Dispose();
			instructions.Dispose();

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

			spriteBatch.Begin();
			spriteBatch.Draw(instructions, new Vector2(50, 10), new Color(0, 0, 0, 255));
			if (fontTexture != null)
			{
				//Draw the font texture
				//This is where YOffset really comes into play, otherwise the text could jump up when the next character is inputed
				spriteBatch.Draw(fontTexture, new Vector2(50, 70 + data.YOffset), new Color(0, 0, 0, 255));
				spriteBatch.Draw(fontTexture, new Vector2(50, 250), new Color(0, 0, 0, 255));
			}
			spriteBatch.End();

			base.Draw(gameTime);
		}

		#region Instructions

		Texture2D instructions;
		private void RenderInstructions()
		{
			string instructionString =
				"Use your keyboard to write something. Press 1 to insert 'Å' character that causes the offset to change.\n"+
				"The upper display shows proper way of handling offsets, the lower displays what happens when you don't.";
			BitmapData data = candara.GenerateBitmapData(instructionString, 16);

			instructions = new Texture2D(GraphicsDevice, data.Width, data.Height, false, SurfaceFormat.Alpha8);
			instructions.SetData(data.Alphas);
		}
		#endregion
	}
}
