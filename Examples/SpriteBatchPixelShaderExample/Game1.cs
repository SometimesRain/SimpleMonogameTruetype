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

		Font font;
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
			font = new Font("Arial");
			Console.WriteLine("Loaded "+font.Name);

			//Rasterize the font
			BitmapData data = font.GenerateBitmapData("Hello, MonoGame!", 64);

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

			//-------------------- SINGLE COLOR TEXT ---------------------
			//Draw the text
			spriteBatch.Draw(fontTexture, new Vector2(100, 100), new Color(0, 0, 255, 255));

			//-------------------- RAINBOW COLOR TEXT --------------------
			//Phase for the outline is half a second ahead of current game time
			float phase = ((float)gameTime.TotalGameTime.TotalSeconds + 0.5f) / 6 % 1;
			//When color alpha is 0, the shader uses the color's red value as the phase of the rainbow
			//  Rainbow is generated on the GPU, see the shader source for implementation
			DrawOutline(spriteBatch, fontTexture, new Vector2(100, 200), new Color(phase, 0, 0, 0));

			//Set phase back to current game time
			phase = (float)gameTime.TotalGameTime.TotalSeconds / 6 % 1;
			//Draw the text
			spriteBatch.Draw(fontTexture, new Vector2(100, 200), new Color(phase, 0, 0, 0));

			//-------------------- FADING COLOR TEXT ---------------------
			//Outline is darker (lower HSV v-component)
			DrawOutline(spriteBatch, fontTexture, new Vector2(100, 300), GetColorFromHSV(phase * 360, 255, 190, 255));
			//Draw the text
			spriteBatch.Draw(fontTexture, new Vector2(100, 300), GetColorFromHSV(phase * 360, 255, 255, 255));

			spriteBatch.End();

			base.Draw(gameTime);
		}

		private void DrawOutline(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Color color)
		{
			const int margin = 2;

			//Create an outline by drawing the texture 8 times
			spriteBatch.Draw(texture, new Vector2(position.X - margin, position.Y - margin), color);
			spriteBatch.Draw(texture, new Vector2(position.X + margin, position.Y - margin), color);
			spriteBatch.Draw(texture, new Vector2(position.X - margin, position.Y + margin), color);
			spriteBatch.Draw(texture, new Vector2(position.X + margin, position.Y + margin), color);
			spriteBatch.Draw(texture, new Vector2(position.X - margin, position.Y         ), color);
			spriteBatch.Draw(texture, new Vector2(position.X + margin, position.Y         ), color);
			spriteBatch.Draw(texture, new Vector2(position.X         , position.Y - margin), color);
			spriteBatch.Draw(texture, new Vector2(position.X         , position.Y + margin), color);
		}

		/// <summary>
		/// Gets a color from hue component.
		/// </summary>
		/// <param name="h">Hue component, between 0 and 1.</param>
		/// <returns></returns>
		private Color GetColorFromHue(float h)
		{
			float r = Math.Abs(h * 6 - 3) - 1;
			float g = 2 - Math.Abs(h * 6 - 2);
			float b = 2 - Math.Abs(h * 6 - 4);
			return new Color(r, g, b);
		}

		/// <summary>
		/// Gets a color from hue, saturation, value and alpha components.
		/// </summary>
		/// <param name="h">Hue component, between 0 and 360.</param>
		/// <param name="s">Saturation component, between 0 and 255.</param>
		/// <param name="v">Value component, between 0 and 255.</param>
		/// <param name="a">Alpha component, between 0 and 255.</param>
		/// <returns></returns>
		private Color GetColorFromHSV(float h, float s, byte v, byte a)
		{
			s /= 255;
			if (s == 0) //achromatic
				return new Color(v, v, v, a);

			h /= 60; //divide to sectors 0-5

			int i = (int)h;
			float f = h - i; //decimal part of h

			byte p = (byte)(v * (1 - s));
			byte q = (byte)(v * (1 - s * f));
			byte t = (byte)(v * (1 - s * (1- f)));

			switch (i)
			{
				case 0:  return new Color(v, t, p, a); //red     to yellow
				case 1:  return new Color(q, v, p, a); //yellow  to green
				case 2:  return new Color(p, v, t, a); //green   to teal
				case 3:  return new Color(p, q, v, a); //teal    to blue
				case 4:  return new Color(t, p, v, a); //blue    to magenta
				default: return new Color(v, p, q, a); //magenta to red
			}
		}
	}
}
