// TODO: implement str(...) using this resolution rule:
//   1. __str__
//   2. not an object? return unmodified
//   3. __repr__
// then replace all these calls with str(%x):
//   if (%x.class $= "String")
//     %x = %x.text;

class (String, TemporaryClass);
	String.definePrivateAttribute("str");

	function String::__construct__(%this, %str)
	{
		%this.str = str(%str);
	}

	function String::__str__(%this)
	{
		return %this.str;
	}

	function String::__repr__(%this)
	{
		return "\"" @ expandEscape(%this.str) @ "\"";
	}

	function String::__bool__(%this)
	{
		return %this.str !$= "";
	}

	function String::__len__(%this)
	{
		return strlen(%this.str);
	}

	function String::__cmp__(%this, %other)
	{
		if (isInstance(%other, String))
			return strcmp(%this.str, %other.str);
		else
			return stricmp(%this.str, %other);
	}

	function String::__iter__(%this)
	{
		%len = strlen(%this.text);
		%iter = ArrayIterator(%len);

		for (%i = 0; %i < %len; %i++)
			%iter.value[%i] = getSubStr(%this.text, %i, 1);

		return %iter;
	}

	function String::__add__(%this, %other)
	{
		return String(%this.str @ str(%other));
	}

	function String::__mul__(%this, %other)
	{
		if (isInstance(%other, Integer))
			%other = %other.i;
		else // bad, will cause unexpected behavior with Float/BigInteger value
			%other |= 0;

		for (%i = 0; %i < %other; %i++)
			%str = %str @ %this.str;

		return String(%str);
	}

	String.defineMethod("capitalize", "String", "",
		"Return a capitalized version of the string, i.e. make the first character" NL
		"have upper case and the rest lower case.");
	function String::capitalize(%this)
	{
		return String(
			strupr(getSubStr(%this.str, 0, 1)) @
			strlwr(getSubStr(%this.str, 1, strlen(%this.str))));
	}

	String.defineMethod("capitalize", "String", "int width, filler = \" \"",
		"Return the string centered in a string of length *width*. Padding is" NL
		"done using *filler* as the fill character (default is a space)." NL
		"Probably doesn't work right now. Not tested, at least.");
	function String::center(%this, %width, %filler)
	{
		%filler = str(%filler);

		if (%filler $= "")
			%filler = " ";

		%len = strlen(%this.text);
		%count = (%width >> 1) - (%len >> 1);

		for (%i = 0; %i < %count; %i++)
			%fill = %fill @ %filler;

		%result = %fill @ %this.text @ %fill;

		if (strlen(%result) == %width - 1)
			%result = %result @ %filler;

		return String(%result);
	}

	String.defineMethod("consistsOf", "bool", "string chars",
		"Test whether or not the string contains nothing but characteres from *chars*.");
	function String::consistsOf(%this, %chars)
	{
		%chars = str(%chars);
		%len = strlen(%this.text);

		for (%i = 0; %i < %len; %i++)
		{
			if (strpos(%chars, getSubStr(%this.text, %i, 1)) == -1)
				return 0;
		}

		return 1;
	}

	String.defineMethod("count", "int", "string text",
		"Count the number of occurrences of the substring *text* inside of this string.");
	function String::count(%this, %text)
	{
		%text = str(%text);

		if (%text $= "")
			return strlen(%this.str);

		%len1 = strlen(%this.str);
		%len2 = strlen(%text);

		%index = 0;
		%count = 0;

		while (%index < %len1)
		{
			%index = strpos(%this.str, %text, %index);

			if (%index == -1)
				break;

			%count = (%count + 1) | 0;
			%index = (%index + %len2) | 0;
		}

		return %count;
	}

	String.defineMethod("endsWith", "bool", "string suffix");
	function String::endsWith(%this, %suffix)
	{
		%suffix = str(%suffix);

		%len1 = strlen(%this.str);
		%len2 = strlen(%suffix);

		if (%len2 > %len1)
			return 0;

		return strcmp(%suffix, getSubStr(%this.str, %len1 - %len2, %len2)) == 0;
	}

	String.defineMethod("find", "int", "string text");
	function String::find(%this, %text)
	{
		return strpos(%this.str, str(%text));
	}

	String.defineMethod("format", "String", "...string args");
	function String::format(%this,
		%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
		%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
	{
		return String(format(%this.str,
			%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
			%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18));
	}

	String.defineMethod("isAlpha", "bool");
	function String::isAlpha(%this)
	{
		return %this._(consistsOf, stringlib.letters);
	}

	String.defineMethod("isAlphaNum", "bool");
	function String::isAlphaNum(%this)
	{
		return %this._(consistsOf, stringlib.letters @ stringlib.digits);
	}

	String.defineMethod("isLower", "bool");
	function String::isLower(%this)
	{
		return strcmp(%this.str, strlwr(%this.str)) == 0;
	}

	String.defineMethod("isNumeric", "bool");
	function String::isNumeric(%this)
	{
		return %this._(consistsOf, stringlib.digits);
	}

	String.defineMethod("isSpace", "bool");
	function String::isSpace(%this)
	{
		return %this._(consistsOf, stringlib.whitespace);
	}

	String.defineMethod("isUpper", "bool");
	function String::isUpper(%this)
	{
		return strcmp(%this.str, strupr(%this.str)) == 0;
	}

	String.defineMethod("join", "String", "Iterable seq");
	function String::join(%this, %seq)
	{
		if (assert(%iter = iter(%seq), "seq is not iterable"))
			return "";

		if (%iter.hasNext())
			%str = str(%iter.next());

		while (%iter.hasNext())
			%str = %str @ %this.str @ str(%iter.next());

		return %str;
	}

	String.defineMethod("lower", "String");
	function String::lower(%this)
	{
		return String(strlwr(%this.str));
	}

	String.defineMethod("replace", "String", "string search, string replace");
	function String::replace(%this, %search, %replace)
	{
		%search = str(%search);

		if (%search $= "")
			return String(%this.str);

		%replace = str(%replace);

		%len1 = strlen(%this.str);
		%len2 = strlen(%search);

		%index = 0;

		while (%index < %len1)
		{
			%start = %index;
			%index = strpos(%this.str, %search);

			if (%index == -1)
			{
				%text = %text @ getSubStr(%this.str, %start, %index - %start);
				break;
			}

			%text = %text @ getSubStr(%this.str, %start, %index - %start);
			%text = %text @ %replace;

			%index = (%index + %len2) | 0;
		}

		return String(%text);
	}

	String.defineMethod("split", "Array", "string sep");
	function String::split(%this, %sep)
	{
		return Array::split(%this.str, str(%sep));
	}

	String.defineMethod("reverse", "String");
	function String::reverse(%this)
	{
		%len = strlen(%this.text);

		for (%i = 0; %i < %len; %i++)
			%text = %text @ getSubStr(%this.text, %len - 1 - %i, 1);

		return String(%text);
	}

	String.defineMethod("startsWith", "bool", "String prefix");
	function String::startsWith(%this, %prefix)
	{
		%prefix = str(%prefix);

		%len1 = strlen(%this.str);
		%len2 = strlen(%prefix);

		if (%len1 < %len2)
			return 0;

		return strcmp(%prefix, getSubStr(%this.str, 0, %len2)) == 0;
	}

	String.defineMethod("strip", "String", "string chars = stringlib.whitespace");
	function String::strip(%this, %chars)
	{
		%chars = str(%chars);

		if (%chars $= "")
			//%chars = string.whitespace;
			return String(trim(%this.str));

		%len = strlen(%this.str);

		for (%i = 0; %i < %len; %i++)
		{
			if (strpos(%chars, getSubStr(%this.str, %i, 1)) == -1)
				break;
		}

		for (%j = strlen(%this.str) - 1; %j >= 0; %j--)
		{
			if (strpos(%chars, getSubStr(%this.str, %j, 1)) == -1)
				break;
		}

		return String(getSubStr(%this.str, %i, (%j + 1) - %i));
	}

	String.defineMethod("stripLeft", "String", "string chars = stringlib.whitespace");
	function String::stripLeft(%this, %chars)
	{
		%chars = str(%chars);

		if (%chars $= "")
			//%chars = string.whitespace;
			return String(ltrim(%this.str));

		%len = strlen(%this.str);

		for (%i = 0; %i < %len; %i++)
		{
			if (strpos(%chars, getSubStr(%this.str, %i, 1)) == -1)
				break;
		}

		return String(getSubStr(%this.str, %i, %len));
	}

	String.defineMethod("stripRight", "String", "string chars = stringlib.whitespace");
	function String::stripRight(%this, %chars)
	{
		%chars = str(%chars);

		if (%chars $= "")
			//%chars = string.whitespace;
			return String(rtrim(%this.str));

		for (%i = strlen(%this.str) - 1; %i >= 0; %i--)
		{
			if (strpos(%chars, getSubStr(%this.str, %i, 1)) == -1)
				break;
		}

		return String(getSubStr(%this.str, 0, %i + 1));
	}

	String.defineMethod("swapCase", "String");
	function String::swapCase(%this)
	{
		%len = strlen(%this.str);

		for (%i = 0; %i < %len; %i++)
		{
			%chr = getSubStr(%this.str, %i, 1);

			if (strcmp(%chr, strlwr(%chr)) == 0)
				%text = %text @ strupr(%chr);
			else
				%text = %text @ strlwr(%chr);
		}

		return String(%text);
	}

	String.defineMethod("upper", "String");
	function String::upper(%this)
	{
		return String(strupr(%this.text));
	}