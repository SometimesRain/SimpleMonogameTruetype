#ifndef WCSUTIL_H
#define WCSUTIL_H

#include <string.h>

// Implementation of wcstok that matches the delimiter string as a whole
// instead of any character within the delimiter string
wchar_t* wcstok_full_delimit(wchar_t* string, wchar_t* delimiter, wchar_t** context)
{
	size_t i = 0, j = 0, delimiterLen = wcslen(delimiter);

	if (string == NULL)
		string = *context;

	while (string[i] != L'\0')
	{
		//Delimiter fully matched
		if (delimiter[j] == L'\0')
		{
			string[i - delimiterLen] = 0;
			*context = string + i;
			return string;
		}
		if (string[i] == delimiter[j])
			++j;
		++i;
	}

	//End of string reached
	if (i == 0)
		return NULL;

	*context = string + i;
	return string;
}

#endif