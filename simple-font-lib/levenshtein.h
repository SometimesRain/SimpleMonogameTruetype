#ifndef LEVENSHTEIN_H
#define LEVENSHTEIN_H

#include <stdlib.h>
#include <string.h>

size_t levenshtein_n(const wchar_t* a, const size_t length, const wchar_t* b, const size_t bLength)
{
	size_t* cache = malloc(sizeof(size_t) * length);
	size_t index = 0;
	size_t bIndex = 0;
	size_t distance;
	size_t bDistance;
	size_t result;
	wchar_t code;

	//Shortcut optimizations / degenerate cases
	if (a == b)
		return 0;

	if (length == 0)
		return bLength;

	if (bLength == 0)
		return length;

	//Initialize the array
	while (index < length)
	{
		cache[index] = index + 1;
		index++;
	}

	while (bIndex < bLength)
	{
		code = b[bIndex];
		result = distance = bIndex++;
		index = SIZE_MAX;

		while (++index < length)
		{
			bDistance = code == a[index] ? distance : distance + 1;
			distance = cache[index];

			cache[index] = result = distance > result
				? (bDistance > result ? result + 1 : bDistance)
				: (bDistance > distance ? distance + 1 : bDistance);
		}
	}

	free(cache);
	return result;
}

size_t levenshtein(const wchar_t* a, const wchar_t* b)
{
	return levenshtein_n(a, wcslen(a), b, wcslen(b));
}

#endif