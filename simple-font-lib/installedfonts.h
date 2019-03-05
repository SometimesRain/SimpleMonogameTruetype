#ifndef INSTALLEDFONTS_H
#define INSTALLEDFONTS_H
#ifdef _WIN32

#include "levenshtein.h"

#include <stdlib.h>
#include <Windows.h>

typedef struct {
	wchar_t* name;
	wchar_t* filename;
} installedfont_t;

installedfont_t* installedFonts = NULL;
size_t numInstalledFonts;

//Loop through all font registry keys and collect truetype fonts to installedFonts array
int LoadInstalledFonts()
{
	HKEY hKey;
	DWORD numValues;
	DWORD maxNameLen;
	DWORD maxDataLen;

	//Get registry key
	LSTATUS status = RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Fonts", 0, KEY_QUERY_VALUE, &hKey);
	if (status != ERROR_SUCCESS)
		return status;

	//Get number of values in this key and the maximum lengths for the name and data fields
	status = RegQueryInfoKeyW(hKey, NULL, NULL, NULL, NULL, NULL, NULL, &numValues, &maxNameLen, &maxDataLen, NULL, NULL);
	if (status != ERROR_SUCCESS)
		return status;

	//Some of the values need to be discarded so the array will be larger than needed (later fixed with a realloc)
	installedFonts = malloc(sizeof(installedfont_t) * numValues);

	//Allocate memory according to the max values returned from query
	LPWSTR valueName = malloc(sizeof(wchar_t) * maxNameLen);
	LPWSTR data = malloc(sizeof(wchar_t) * maxDataLen);

	DWORD dwIndex = 0;
	while (status != ERROR_NO_MORE_ITEMS)
	{
		//Copy nameLen and dataLen because they get overwritten by RegEnumValueW to the actual lengths for that registry value
		//NOTE: nameLen is in characters and dataLen is in bytes
		DWORD nameLen = maxNameLen;
		DWORD dataLen = maxDataLen;
		status = RegEnumValueW(hKey, dwIndex++, valueName, &nameLen, NULL, NULL, (LPBYTE)data, &dataLen);

		//Discard raster and vector fonts
		if (wcscmp(valueName + nameLen - 11, L" (TrueType)") == 0)
		{
			//stb_truetype doesn't support TTC
			if (wcscmp(data + (dataLen / 2 - 5), L".ttc") == 0) continue;

			//Discard " (TrueType)" suffix
			valueName[nameLen - 11] = 0;

			//Add font
			size_t nameShortLen = sizeof(wchar_t) * (nameLen - 10);
			installedFonts[numInstalledFonts].name = malloc(nameShortLen);
			memcpy(installedFonts[numInstalledFonts].name, valueName, nameShortLen);

			installedFonts[numInstalledFonts].filename = malloc(dataLen);
			memcpy(installedFonts[numInstalledFonts].filename, data, dataLen);

			++numInstalledFonts;
		}
	}

	status = RegCloseKey(hKey);

	//Resize array to exactly the size it needs to be
	realloc(installedFonts, sizeof(installedfont_t) * numInstalledFonts);
	free(valueName);
	free(data);

	return ERROR_SUCCESS;
}

installedfont_t* GetFontByName(const wchar_t* name)
{
	//Load installed fonts only once
	if (installedFonts == NULL)
		if (LoadInstalledFonts() != ERROR_SUCCESS)
			return NULL;

	installedfont_t* font = NULL;
	size_t minDistance = SIZE_MAX;

	//Find font with lowest levenshtein distance to parameter name
	for (size_t i = 0; i < numInstalledFonts; i++)
	{
		size_t distance = levenshtein(name, installedFonts[i].name);

		//Exact match
		if (distance == 0)
			return installedFonts + i;

		if (distance < minDistance)
		{
			font = installedFonts + i;
			minDistance = distance;
		}
	}

	return font;
}

#else

//No implementation for non-Windows operating systems
void LoadInstalledFonts()
{

}

installedfont_t* GetFontByName(const wchar_t* name)
{
	return NULL;
}

#endif
#endif