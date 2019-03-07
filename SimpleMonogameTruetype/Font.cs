using System;
using System.Runtime.InteropServices;

namespace SimpleMonogameTruetype
{
	/// <summary>
	/// A TrueType or OpenType font object.
	/// </summary>
    public unsafe class Font
    {
		private int handle;

		/// <summary>
		/// Name of the font. If the font was loaded by name (which gives the closest match), this can be used to verify the correct font was loaded.
		/// </summary>
		public string Name
		{
			get; private set;
		}

		/// <summary>
		/// Creates a TrueType or OpenType font.
		/// </summary>
		/// <param name="font">Path to the font file, or name of an installed font.</param>
		public Font(string font)
		{
			IntPtr actualName;
			if (font.Substring(font.Length - 4, 4) == ".ttf" || font.Substring(font.Length - 4, 4) == ".otf")
				handle = LoadFont(font, 0, out actualName);
			else
				handle = LoadFontByName(font, out actualName);

			if (handle == -1) throw new Exception("File not found");
			if (handle == -2) throw new Exception(font + " is not a valid font");
			if (handle == -3) throw new NotImplementedException("Installed fonts can only be accessed on Windows");

			Name = Marshal.PtrToStringUni(actualName);
		}

		/// <summary>
		/// Creates a TrueType or OpenType font from .ttc/.otc font collection file.
		/// </summary>
		/// <param name="path">Path to the font collection file.</param>
		/// <param name="index">Index of font in the collection.</param>
		public Font(string path, int index)
		{
			IntPtr actualName;
			if (path.Substring(path.Length - 4, 4) == ".ttc" || path.Substring(path.Length - 4, 4) == ".otc")
				handle = LoadFont(path, index, out actualName);
			else
				throw new Exception("File is not a TrueType or OpenType font collection (.ttc/.otc)");

			if (handle == -1) throw new Exception("File not found");
			if (handle == -2) throw new Exception(path + " is not a valid font");
			if (handle == -3) throw new NotImplementedException("Installed fonts can only be accessed on Windows");

			Name = Marshal.PtrToStringUni(actualName);
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
			fixed (byte* p = data)
			{
				GenerateBitmap(handle, p, width);
			}

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
			fixed (byte* p = data)
			{
				GenerateBitmap(handle, p, width);
			}
			//GenerateBitmap(handle, data, width);

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
		/// Convert from pt units to pixels.
		/// </summary>
		/// <param name="pt">Font size in points.</param>
		/// <returns></returns>
		public static int PointsToPixels(int pt)
		{
			return pt * 4 / 3;
		}

		[DllImport("simple-font-lib.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int LoadFont([MarshalAs(UnmanagedType.LPWStr)]string filename, int index, out IntPtr actualName);

		[DllImport("simple-font-lib.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int LoadFontByName([MarshalAs(UnmanagedType.LPWStr)]string fontname, out IntPtr actualName);

		[DllImport("simple-font-lib.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void MeasureBitmap(int handle, [MarshalAs(UnmanagedType.LPWStr)]string text, int fontSize,
			out int width, out int height, out int yOffset, int maxWidth, float lineSpacing);

		[DllImport("simple-font-lib.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void GenerateBitmap(int handle, byte* emptyBitmap, int width);

		/// <summary>
		/// Prints all installed fonts.
		/// </summary>
		[DllImport("simple-font-lib.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void PrintInstalledFonts();

		/// <summary>
		/// Free all unmanaged resources.
		/// </summary>
		[DllImport("simple-font-lib.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void FreeAllResources();
	}
}
