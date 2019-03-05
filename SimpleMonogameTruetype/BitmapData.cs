using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMonogameTruetype
{
	/// <summary>
	/// An object containing size and alpha values for a bitmap.
	/// </summary>
	public struct BitmapData
	{
		/// <summary>
		/// Width of the bitmap.
		/// </summary>
		public int Width;
		/// <summary>
		/// Height of the bitmap
		/// </summary>
		public int Height;
		/// <summary>
		/// Stores offset of the top-most row in pixels. Useful for input fields or other applications 
		/// where you don't want the text to jump if the top border is exceeded.
		/// </summary>
		public int YOffset;
		/// <summary>
		/// Alpha values for every pixel.
		/// </summary>
		public byte[] Alphas;

		/// <summary>
		/// Creates a new <see cref="BitmapData"/> object.
		/// </summary>
		/// <param name="width">Width of the bitmap.</param>
		/// <param name="height">Height of the bitmap.</param>
		/// <param name="yOffset">Offset of the top-most row.</param>
		/// <param name="alphas">Alpha values for every pixel.</param>
		public BitmapData(int width, int height, int yOffset, byte[] alphas)
		{
			Width = width;
			Height = height;
			YOffset = yOffset;
			Alphas = alphas;
		}

		/// <summary>
		/// Expand from alpha only to RGBA.
		/// </summary>
		/// <param name="hexColor">Bitmap color in hexadecimal.</param>
		/// <returns></returns>
		public byte[] ExpandToRGBA(int hexColor)
		{
			return ExpandToRGBA((byte)(hexColor >> 16), (byte)(hexColor >> 8), (byte)hexColor);
		}

		/// <summary>
		/// Expand from alpha only to RGBA.
		/// </summary>
		/// <param name="r">Red value.</param>
		/// <param name="g">Green value.</param>
		/// <param name="b">Blue value.</param>
		/// <returns></returns>
		public byte[] ExpandToRGBA(byte r, byte g, byte b)
		{
			byte[] pixels = new byte[Alphas.Length * 4];
			for (int i = 0; i < Alphas.Length; i++)
			{
				int i4 = i * 4;
				pixels[i4 + 0] = r;
				pixels[i4 + 1] = g;
				pixels[i4 + 2] = b;
				pixels[i4 + 3] = Alphas[i];
			}

			return pixels;
		}
	}
}
