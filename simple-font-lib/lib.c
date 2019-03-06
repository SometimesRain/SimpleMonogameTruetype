#define _CRT_SECURE_NO_WARNINGS
#include <stdio.h>
#include <stdlib.h>

#define STB_TRUETYPE_IMPLEMENTATION 
#include "stb_truetype.h"

#include "installedfonts.h"

//---------------------------------- DATA TYPES -----------------------------------
typedef struct
{
	stbtt_fontinfo info;
	wchar_t* filename;
	int ascent;
	int descent;
	int lineGap;
} font_t;

typedef struct
{
	int codepoint;
	int offsetX;
	int offsetY;
	int width;
	int height;
} glyph_t;

enum
{
	FILE_NOT_FOUND = -1,
	INVALID_FONT = -2,
	WINDOWS_ONLY = -3
};

//------------------------------ LOADING AND FREEING ------------------------------
font_t** fonts = NULL;
size_t capacity = 0;

//Returns a handle to the loaded font
__declspec(dllexport) int LoadFont(wchar_t* filename)
{
	//Check if font is already loaded
	for (size_t i = 0; i < capacity; i++)
		if (fonts[i] != NULL && wcscmp(fonts[i]->filename, filename) == 0)
			return i;

	//Open for reading
	FILE* fontFile = _wfopen(filename, L"rb");
	if (fontFile == NULL)
		return FILE_NOT_FOUND;

	//Get length
	fseek(fontFile, 0, SEEK_END);
	size_t size = ftell(fontFile);
	fseek(fontFile, 0, SEEK_SET);

	//Allocate, read and close
	unsigned char* fontBuffer = malloc(size);
	fread(fontBuffer, size, 1, fontFile);
	fclose(fontFile);

	//Initialize font
	font_t* font = malloc(sizeof(font_t));
	if (!stbtt_InitFont(&font->info, fontBuffer, 0))
	{
		//Invalid font
		free(font);
		return INVALID_FONT;
	}

	//Get vertical metrics and set filename
	stbtt_GetFontVMetrics(&font->info, &font->ascent, &font->descent, &font->lineGap);
	size = sizeof(wchar_t) * (wcslen(filename) + 1);
	font->filename = malloc(size);
	memcpy(font->filename, filename, size);

	//Add font to fonts
	//First see if there are free slots on the array as it is (marked NULL, left by freed fonts)
	for (size_t i = 0; i < capacity; i++)
	{
		if (fonts[i] == NULL)
		{
			fonts[i] = font;
			return i;
		}
	}

	//Array needs to be extended
	fonts = realloc(fonts, sizeof(font_t*) * ++capacity);
	fonts[capacity - 1] = font;
	return capacity - 1;
}

__declspec(dllexport) int LoadFontByName(wchar_t* fontname)
{
	//Use winapi to find the correct font file
	installedfont_t* font = GetFontByName(fontname);
	if (font == NULL)
		return WINDOWS_ONLY;

	//Create path
	wchar_t path[MAX_PATH];
	GetWindowsDirectoryW(path, MAX_PATH);
	wcscat(path, L"\\Fonts\\");
	wcscat(path, font->filename);

	return LoadFont(path);
}

//Don't call this to take advantage of caching
__declspec(dllexport) void FreeFont(int handle)
{
	//Free memory and erase the pointer
	free(fonts[handle]->info.data);
	free(fonts[handle]->filename);
	free(fonts[handle]);
	fonts[handle] = NULL;
}

//Included for good practice, in real world cases you can rely on Windows cleaning up
__declspec(dllexport) void FreeAllResources()
{
	//lib.c
	for (size_t i = 0; i < capacity; i++)
	{
		if (fonts[i] != NULL)
		{
			free(fonts[i]->info.data);
			free(fonts[i]->filename);
			free(fonts[i]);
		}
	}
	free(fonts);

	//installedfonts.h
	for (size_t i = 0; i < numInstalledFonts; i++)
	{
		free(installedFonts[i].name);
		free(installedFonts[i].filename);
	}
	free(installedFonts);
}

//------------------------------- GENERATING BITMAP -------------------------------
glyph_t* glyphs = NULL;
size_t numGlyphs;
int extraYOffset;
float scale;

__declspec(dllexport) void MeasureBitmap(int handle, wchar_t* text, int fontSize, int* width, int* height, int* yOffset, int maxWidth, float lineSpacing)
{
	size_t lastSpaceAt = 0;
	int lineMaxXAtSpace = 0;
	if (maxWidth == 0)
		maxWidth = INT_MAX;

	numGlyphs = wcslen(text);
	glyphs = malloc(sizeof(glyph_t) * numGlyphs);
	memset(glyphs, 0, sizeof(glyph_t) * numGlyphs);

	stbtt_fontinfo* info = &fonts[handle]->info;
	scale = stbtt_ScaleForPixelHeight(info, (float)fontSize);
	extraYOffset = 0;

	float x = 0, y = 0, maxX = 0, maxY = 0;
	float lineYIncrement = fontSize / 2 + fontSize / 2 * lineSpacing;
	float ascent = fonts[handle]->ascent * scale;
	int advanceWidth = 0, x1 = 0, lineMaxX = 0;
	for (size_t i = 0; i < numGlyphs + 1; i++)
	{
		if (text[i] == L'\r') continue;
		if (text[i] == L'\n' || text[i] == L'\0')
		{
			maxX = max(lineMaxX, maxX);
			x = 0;
			y += lineYIncrement;
			continue;
		}

		int kern = stbtt_GetCodepointKernAdvance(info, text[i], text[i + 1]);

		int leftSideBearing;
		stbtt_GetCodepointHMetrics(info, text[i], &advanceWidth, &leftSideBearing);

		int x0, y0, y1;
		stbtt_GetCodepointBitmapBox(info, text[i], scale, scale, &x0, &y0, &x1, &y1);

		glyphs[i].codepoint = text[i];
		glyphs[i].width = x1 - x0;
		glyphs[i].height = y1 - y0;
		glyphs[i].offsetY = y0 + (int)(y + ascent);

		//If the a character on the first line exceeds top of the bitmap, bring all characters down by extraYOffset
		if (glyphs[i].offsetY < 0 && extraYOffset < -glyphs[i].offsetY)
			extraYOffset = -glyphs[i].offsetY;

		int lastX = (int)x;
		if (x == 0 && leftSideBearing < 0)
		{
			//Glyph is the first character of a line with a negative left side bearing
			glyphs[i].offsetX = (int)x;
			x += (advanceWidth + kern - leftSideBearing) * scale;
		}
		else
		{
			glyphs[i].offsetX = (int)(x + leftSideBearing * scale);
			x += (advanceWidth + kern) * scale;
		}
		x = (float)(int)(x + 0.5f);

		lineMaxX = (int)x - ((int)(advanceWidth * scale) - x1);
		maxY = max((int)(y + ascent) + y1, maxY);

		if (text[i] == L' ')
		{
			lineMaxXAtSpace = min(lineMaxX, maxWidth);
			lastSpaceAt = i;
		}
		if (lineMaxX > maxWidth)
		{
			maxX = max(lineMaxXAtSpace, maxX);
			x = 0;
			y += lineYIncrement;
			if (lastX != 0)
				i = lastSpaceAt;
			continue;
		}
	}

	*width = (int)maxX;
	*height = (int)maxY + extraYOffset;
	*yOffset = -extraYOffset;
}

__declspec(dllexport) void GenerateBitmap(int handle, unsigned char* emptyBitmap, int width)
{
	for (size_t i = 0; i < numGlyphs; i++)
	{
		if (glyphs[i].codepoint != 0)
		{
			int offset = (glyphs[i].offsetY + extraYOffset) * width + glyphs[i].offsetX;
			stbtt_MakeCodepointBitmap(&fonts[handle]->info, emptyBitmap + offset, glyphs[i].width, glyphs[i].height, width, scale, scale, glyphs[i].codepoint);
		}
	}
	free(glyphs);
}