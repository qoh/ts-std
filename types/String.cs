// TODO: implement str(...) using this resolution rule:
//   1. __str__
//   2. not an object? return unmodified
//   3. __repr__
// then replace all these calls with str(%x):
//   if (%x.class $= "String")
//     %x = %x.text;

function String(%text)
{
	if (%text.class $= "Class")
	{
		return tempref(new ScriptObject()
		{
			class = "String";
			text = %text.text;
		} @ "\x08");
	}

	// %text = str(%text);

	return tempref(new ScriptObject()
	{
		class = "String";
		text = %text;
	} @ "\x08");
}

function String::__cmp__(%this, %other)
{
	if (%other.class $= "String")
		return strcmp(%this.text, %other.text);
	else
		return stricmp(%this.text);
}

function String::__str__(%this)
{
	return %this.text;
}

function String::__add__(%this, %other)
{
	if (assert(%other.class $= "String", "other must be String for String + other"))
		return "";

	return String(%this.text @ %other.text);
}

function String::__mul__(%this, %other)
{
	return String(repeat(%this.text, %other));
}

function String::__repr__(%this)
{
	return "\"" @ expandEscape(%this.text) @ "\"";
}

function String::__len__(%this)
{
	return strlen(%this.text);
}

function String::__bool__(%this)
{
	return %this.text !$= "";
}

function String::__iter__(%this)
{
	%len = strlen(%this.text);
	%iter = ArrayIterator(%len);

	for (%i = 0; %i < %len; %i++)
		%iter.value[%i] = getSubStr(%this.text, %i, 1);

	return %iter;
}

function String::capitalize(%this)
{
	return String(
		strupr(getSubStr(%this.text, 0, 1)) @
		strlwr(getSubStr(%this.text, 1, strlen(%this.text))));
}

// probably doesn't work
function String::center(%this, %width, %filler)
{
	if (%filler.class $= "String")
		%filler = %filler.text;

	if (%filler $= "")
		%filler = " ";

	%len = strlen(%this.text);
	%count = (%width >> 1) - (%len >> 1);

	%fill = repeat(%fillter, %count);
	%result = %fill @ %this.text @ %fill;

	if (strlen(%result) == %width - 1)
		%result = %result @ %filler;

	return String(%result);
}

function String::consistsOf(%this, %chars)
{
	if (%chars.class $= "String")
		%chars = %chars.text;

	%len = strlen(%this.text);

	for (%i = 0; %i < %len; %i++)
	{
		if (strpos(%chars, getSubStr(%this.text, %i, 1)) == -1)
			return 0;
	}

	return 1;
}

function String::count(%this, %text)
{
	if (%text.class $= "String")
		%text = %text.text;

	%len1 = strlen(%this.text);
	%len2 = strlen(%text);

	if (%len2 < 1)
		return %len1;

	%index = 0;
	%count = 0;

	while (%index < %len1)
	{
		%index = strpos(%this.text, %text, %index);

		if (%index == -1)
			break;

		%count = (%count + 1) | 0;
		%index = (%index + %len2) | 0;
	}

	return %count;
}

function String::endsWith(%this, %suffix)
{
	if (%suffix.class $= "String")
		%suffix.class = %suffix.text;

	%len1 = strlen(%this.text);
	%len2 = strlen(%suffix);

	if (%len2 > %len1)
		return 0;

	return strcmp(%suffix, getSubStr(%this.text, %len1 - %len2, %len2)) == 0;
}

function String::find(%this, %text)
{
	if (%text.class $= "String")
		%text = %text.text;

	return strpos(%this.text, %text);
}

function String::format(%this,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	return String(format(%this.text,
		%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
		%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18));
}

function String::isAlpha(%this)
{
	return %this.consistsOf(stringlib.letters);
}

function String::isAlphaNum(%this)
{
	return %this.consistsOf(stringlib.letters @ stringlib.digits);
}

function String::isLower(%this)
{
	return strcmp(%this.text, strlwr(%this.text)) == 0;
}

function String::isNumeric(%this)
{
	return %this.consistsOf(stringlib.digits);
}

function String::isSpace(%this)
{
	return %this.consistsOf(stringlib.whitespace);
}

function String::isUpper(%this)
{
	return strcmp(%this.text, strupr(%this.text)) == 0;
}

function String::join(%this, %seq)
{
	return join(%seq, %this.text);
}

function String::lower(%this)
{
	return String(strlwr(%this.text));
}

function String::replace(%this, %search, %replace)
{
	if (%search.class $= "String")
		%search = %search.text;

	if (%replace.class $= "String")
		%replace = %replace.text;

	%len1 = strlen(%this.text);
	%len2 = strlen(%search);

	if (%len2 < 1)
		return String(%this);

	%index = 0;

	while (%index < %len1)
	{
		%start = %index;
		%index = strpos(%this.text, %search);

		if (%index == -1)
		{
			%text = %text @ getSubStr(%this.text, %start, %index - %start);
			break;
		}

		%text = %text @ getSubStr(%this.text, %start, %index - %start);
		%text = %text @ %replace;

		%index = (%index + %len2) | 0;
	}

	return String(%text);
}

function String::split(%this, %text)
{
	if (%text.class $= "String")
		%text = %text.text;

	return Array::split(%this.text, %text);
}

function String::reverse(%this)
{
	%len = strlen(%this.text);

	for (%i = 0; %i < %len; %i++)
		%text = %text @ getSubStr(%this.text, %len - 1 - %i, 1);

	return String(%text);
}

function String::startswith(%this, %prefix)
{
	if (%prefix.class $= "String")
		%prefix = %prefix.text;

	%len1 = strlen(%this.text);
	%len2 = strlen(%prefix);

	if (%len1 < %len2)
		return 0;

	return strcmp(%prefix, getSubStr(%this.text, 0, %len2)) == 0;
}

function String::strip(%this, %chars)
{
	if (%chars.class $= "String")
		%chars = %chars.text;

	if (%chars $= "")
		%chars = string.whitespace;

	%len = strlen(%this.text);

	for (%i = 0; %i < %len; %i++)
	{
		if (strpos(%chars, getSubStr(%this.text, %i, 1)) == -1)
			break;
	}

	for (%j = strlen(%this.text) - 1; %j >= 0; %j--)
	{
		if (strpos(%chars, getSubStr(%this.text, %j, 1)) == -1)
			break;
	}

	return String(getSubStr(%this.text, %i, (%j + 1) - %i));
}

function String::stripLeft(%this, %chars)
{
	if (%chars.class $= "String")
		%chars = %chars.text;

	if (%chars $= "")
		%chars = string.whitespace;

	%len = strlen(%this.text);

	for (%i = 0; %i < %len; %i++)
	{
		if (strpos(%chars, getSubStr(%this.text, %i, 1)) == -1)
			break;
	}

	return String(getSubStr(%this.text, %i, %len));
}

function String::stripRight(%this, %chars)
{
	if (%chars.class $= "String")
		%chars = %chars.text;

	if (%chars $= "")
		%chars = string.whitespace;

	for (%i = strlen(%this.text) - 1; %i >= 0; %i--)
	{
		if (strpos(%chars, getSubStr(%this.text, %i, 1)) == -1)
			break;
	}

	return String(getSubStr(%this.text, 0, %i + 1));
}

function String::swapCase(%this)
{
	%len = strlen(%this.text);

	for (%i = 0; %i < %len; %i++)
	{
		%chr = getSubStr(%this.text, %i, 1);

		if (strcmp(%chr, strlwr(%chr)) == 0)
			%text = %text @ strupr(%chr);
		else
			%text = %text @ strlwr(%chr);
	}

	return %text;
}

function String::upper(%this)
{
	return String(strupr(%this.text));
}