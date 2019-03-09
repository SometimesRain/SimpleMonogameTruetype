#ifndef INSTALLEDFONTS_H
#define INSTALLEDFONTS_H

#include "levenshtein.h"
#include "wcsutil.h"

#include <stdlib.h>

typedef struct {
	wchar_t* name;
	int fontIndex;
	wchar_t* filename;
} installedfont_t;

installedfont_t* instFonts = NULL;
size_t numInstFonts;

#ifdef _WIN32
#include <Windows.h>

//Loop through all font registry keys and collect truetype fonts to instFonts array
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

	//Guess the size of the array (later fixed with a realloc)
	size_t allocSize = 512;
	instFonts = malloc(sizeof(installedfont_t) * allocSize);

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
			//Discard " (TrueType)" suffix
			nameLen -= 10;
			valueName[nameLen - 1] = 0;

			if (wcscmp(data + (dataLen / 2 - 5), L".ttc") == 0)
			{
				wchar_t* pos;
				wchar_t* token = wcstok_full_delimit(valueName, L" & ", &pos);
				int i = 0;
				while (token != NULL)
				{
					//Add font
					size_t tokenSize = (wcslen(token) + 1) * sizeof(wchar_t);
					instFonts[numInstFonts].name = memcpy(malloc(tokenSize), token, tokenSize);
					instFonts[numInstFonts].filename = memcpy(malloc(dataLen), data, dataLen);
					instFonts[numInstFonts].fontIndex = i;

					++numInstFonts, ++i;

					token = wcstok_full_delimit(NULL, L" & ", &pos);
				}
			}
			else
			{
				//Add font
				instFonts[numInstFonts].name = memcpy(malloc(sizeof(wchar_t) * nameLen), valueName, sizeof(wchar_t) * nameLen);
				instFonts[numInstFonts].filename = memcpy(malloc(dataLen), data, dataLen);
				instFonts[numInstFonts].fontIndex = 0;

				++numInstFonts;
			}
		}
		if (numInstFonts > allocSize)
		{
			allocSize += 512;
			instFonts = realloc(instFonts, sizeof(installedfont_t) * allocSize);
		}
	}

	status = RegCloseKey(hKey);

	//Resize array to exactly the size it needs to be
	instFonts = realloc(instFonts, sizeof(installedfont_t) * numInstFonts);
	free(valueName);
	free(data);

	return ERROR_SUCCESS;
}

#else

//No implementation for non-Windows operating systems
void LoadInstalledFonts()
{

}

#endif

installedfont_t* GetFontByName(const wchar_t* name)
{
	//Load installed fonts only once
	if (instFonts == NULL)
		if (LoadInstalledFonts() != ERROR_SUCCESS)
			return NULL;

	installedfont_t* font = NULL;
	size_t minDistance = SIZE_MAX;

	//Find font with lowest levenshtein distance to parameter name
	for (size_t i = 0; i < numInstFonts; i++)
	{
		size_t distance = levenshtein(name, instFonts[i].name);

		//Exact match
		if (distance == 0)
			return instFonts + i;

		if (distance < minDistance)
		{
			font = instFonts + i;
			minDistance = distance;
		}
	}

	return font;
}

__declspec(dllexport) void PrintInstalledFonts()
{
	//Load installed fonts only once
	if (instFonts == NULL)
		if (LoadInstalledFonts() != ERROR_SUCCESS)
			return;

	for (size_t i = 0; i < numInstFonts; i++)
		wprintf(L"%s (%s)\n", instFonts[i].name, instFonts[i].filename);
}

/*__declspec(dllexport) wchar_t** GetNClosestMatches(const wchar_t* name, size_t n)
{
	//Load installed fonts only once
	if (instFonts == NULL)
		if (LoadInstalledFonts() != ERROR_SUCCESS)
			return NULL;

	wchar_t** matches = realloc(matches, sizeof(wchar_t*) * n);
	size_t* minDistance = malloc(sizeof(size_t*) * n);
	memset(minDistance, SIZE_MAX, sizeof(size_t*) * n);

	//Find font with lowest levenshtein distance to parameter name
	for (size_t i = 0; i < numInstFonts; i++)
	{
		size_t distance = levenshtein(name, instFonts[i].name);

		for (size_t j = 0; j < n; j++)
		{
			if (distance < minDistance[j])
			{
				matches[j] = instFonts[i].name;
				minDistance[j] = distance;
				break;
			}
		}
	}

	return matches;
}*/

#endif