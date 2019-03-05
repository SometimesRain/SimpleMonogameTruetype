using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMonogameTruetype
{
	/// <summary>
	/// A TrueType or OpenType font object.
	/// </summary>
    public class Font
    {
		private int handle;

		/// <summary>
		/// Creates a TrueType or OpenType font.
		/// </summary>
		/// <param name="font">Path to the font file, or name of an installed font.</param>
		public Font(string font)
		{
			if (font.Substring(font.Length - 4, 4) == ".ttf" || font.Substring(font.Length - 4, 4) == ".otf")
				handle = LoadFont(font);
			else
				handle = LoadFontByName(font);

			if (handle == -1) throw new Exception("File not found");
			if (handle == -2) throw new Exception(font + " is not a valid font");
			if (handle == -3) throw new NotImplementedException("Installed fonts can only be accessed on Windows");
		}

		/// <summary>
		/// Generates bitmap data for the desired string using the font.<para>If you want to force the width of the bitmap, use <see cref="GenerateBitmapDataForceWidth(string, int, int, float)"/>.</para>
		/// </summary>
		/// <param name="text">The text to be rendered.</param>
		/// <param name="fontSize">Font size in pixels. To convert from pt units use <see cref="PointsToPixels(int)"/>.</param>
		/// <param name="maxWidth">Break the line is this width is exceeded. Resulting width may be smaller than this.</param>
		/// <param name="lineSpacing">Space between the lines.</param>
		/// <returns>A <see cref="BitmapData"/> object containing the size and alpha values for the bitmap.</returns>
		public BitmapData GenerateBitmapData(string text, int fontSize, int maxWidth, float lineSpacing)
		{
			MeasureBitmap(handle, text, fontSize, out int width, out int height, out int yOffset, maxWidth, lineSpacing);
			byte[] data = new byte[width * height];
			GenerateBitmap(handle, data, width);

			return new BitmapData(width, height, yOffset, data);
		}

		/// <summary>
		/// Generates bitmap data for the desired string using the font.
		/// </summary>
		/// <param name="text">The text to be rendered.</param>
		/// <param name="fontSize">Font size in pixels. To convert from pt units use <see cref="PointsToPixels(int)"/>.</param>
		/// <param name="lineSpacing">Space between the lines.</param>
		/// <returns>A <see cref="BitmapData"/> object containing the size and alpha values for the bitmap.</returns>
		public BitmapData GenerateBitmapData(string text, int fontSize, float lineSpacing)
		{
			return GenerateBitmapData(text, fontSize, 0, lineSpacing);
		}

		/// <summary>
		/// Generates bitmap data for the desired string using the font.<para>If you want to force the width of the bitmap, use <see cref="GenerateBitmapDataForceWidth(string, int, int, float)"/>.</para>
		/// </summary>
		/// <param name="text">The text to be rendered.</param>
		/// <param name="fontSize">Font size in pixels. To convert from pt units use <see cref="PointsToPixels(int)"/>.</param>
		/// <param name="maxWidth">Break the line is this width is exceeded. </param>
		/// <returns>A <see cref="BitmapData"/> object containing the size and alpha values for the bitmap.</returns>
		public BitmapData GenerateBitmapData(string text, int fontSize, int maxWidth)
		{
			return GenerateBitmapData(text, fontSize, maxWidth, 1.5f);
		}

		/// <summary>
		/// Generates bitmap data for the desired string using the font.
		/// </summary>
		/// <param name="text">The text to be rendered.</param>
		/// <param name="fontSize">Font size in pixels. To convert from pt units use <see cref="PointsToPixels(int)"/>.</param>
		/// <returns>A <see cref="BitmapData"/> object containing the size and alpha values for the bitmap.</returns>
		public BitmapData GenerateBitmapData(string text, int fontSize)
		{
			return GenerateBitmapData(text, fontSize, 0, 1.5f);
		}

		/// <summary>
		/// Generates bitmap data for the desired string using the font.
		/// </summary>
		/// <param name="text">The text to be rendered.</param>
		/// <param name="fontSize">Font size in pixels. To convert from pt units use <see cref="PointsToPixels(int)"/>.</param>
		/// <param name="maxWidth">Break the line is this width is exceeded. Sets the width of the bitmap.</param>
		/// <param name="lineSpacing">Space between the lines.</param>
		/// <returns>A <see cref="BitmapData"/> object containing the size and alpha values for the bitmap.</returns>
		public BitmapData GenerateBitmapDataForceWidth(string text, int fontSize, int maxWidth, float lineSpacing)
		{
			MeasureBitmap(handle, text, fontSize, out int width, out int height, out int yOffset, maxWidth, lineSpacing);
			if (width < maxWidth)
				width = maxWidth;
			byte[] data = new byte[width * height];
			GenerateBitmap(handle, data, width);

			return new BitmapData(width, height, yOffset, data);
		}

		/// <summary>
		/// Generates bitmap data for the desired string using the font.
		/// </summary>
		/// <param name="text">The text to be rendered.</param>
		/// <param name="fontSize">Font size in pixels. To convert from pt units use <see cref="PointsToPixels(int)"/>.</param>
		/// <param name="maxWidth">Break the line is this width is exceeded. Sets the width of the bitmap.</param>
		/// <returns>A <see cref="BitmapData"/> object containing the size and alpha values for the bitmap.</returns>
		public BitmapData GenerateBitmapDataForceWidth(string text, int fontSize, int maxWidth)
		{
			return GenerateBitmapDataForceWidth(text, fontSize, maxWidth, 1.5f);
		}

		/// <summary>
		/// Saves <see cref="BitmapData"/> to a file.
		/// </summary>
		/// <param name="path">Output file path.</param>
		/// <param name="bitmapData">Bitmap data to be saved.</param>
		/// <param name="hexColor">Text color in hexadecimal.</param>
		public void DrawBitmapDataToFile(string path, BitmapData bitmapData, int hexColor = 0)
		{
			Bitmap bitmap = new Bitmap(bitmapData.Width, bitmapData.Height);
			for (int i = 0; i < bitmapData.Alphas.Length; i++)
				bitmap.SetPixel(i % bitmapData.Width, i / bitmapData.Width, Color.FromArgb(bitmapData.Alphas[i] << 24 | hexColor));

			bitmap.Save(path);
		}

		/// <summary>
		/// Convert from pt units to pixels.
		/// </summary>
		/// <param name="pt">Font size in points.</param>
		/// <returns></returns>
		public static int PointsToPixels(int pt)
		{
			return pt * 4 / 3;
		}

		[DllImport("simple-font-lib.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int LoadFont([MarshalAs(UnmanagedType.LPWStr)]string filename);

		[DllImport("simple-font-lib.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int LoadFontByName([MarshalAs(UnmanagedType.LPWStr)]string fontname);

		[DllImport("simple-font-lib.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void FreeFont(int handle);

		[DllImport("simple-font-lib.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void MeasureBitmap(int handle, [MarshalAs(UnmanagedType.LPWStr)]string text, int fontSize,
			out int width, out int height, out int yOffset, int maxWidth, float lineSpacing);

		[DllImport("simple-font-lib.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void GenerateBitmap(int handle, byte[] emptyBitmap, int width);

		/// <summary>
		/// Free all unmanaged resources.
		/// </summary>
		[DllImport("simple-font-lib.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void FreeAllResources();
	}
}
