if (!isObject(stringlib))
{
	new ScriptObject(stringlib)
	{
		class = "Module";
		
		bindigits = "01";
		colors = "\c0\c1\c2\c3\c4\c5\c6\c7\c8\c9\cp\co\cr";
		digits = "0123456789";
		hexdigits = "0123456789abcdefABCDEF";
		lowercase = "abcdefghijklmnopqrstuvwxyz";
		octdigits = "01234567";
		punctuation = ".!?:,;-*\"()[]{}<>&";
		whitespace = " \f\t\r\n";
	};

	stringlib.uppercase = strupr(stringlib.lowercase);
	stringlib.letters = stringlib.lowercase @ stringlib.uppercase;
	stringlib.printable = stringlib.letters @ stringlib.punctuation @ stringlib.whitespace;
}

function stripColorChars(%string)
{
	return stripChars(%string, stringlib.colors);
}

function _iterformat_num(%format, %i, %len)
{
	while (%i < %len)
	{
		%chr = getSubStr(%format, %i, 1);

		if (strpos("0123456789", %chr) == -1)
			break;

		%out = %out @ %chr;
		%i++;
	}

	return %i SPC %out;
}

function loc(%str, %i, %desc)
{
	echo(%str, "\n", repeat(" ", %i), "^ ", %desc);
}

function iterformat(%format, %iter)
{
	%i = 0;
	%len = strlen(%format);

	while (%i < %len)
	{
		// search for a format
		%j = strpos(%format, "%", %i);

		if (%j == -1)
		{
			%out = %out @ getSubStr(%format, %i, %len);
			break;
		}

		if (%i != %j)
			%out = %out @ getSubStr(%format, %i, %j - %i);

		%j++; // go past the actual % sign

		if (%j >= %len) // trailing % produces raw %
		{
			%out = %out @ "%";
			break;
		}

		if ((%chr = getSubStr(%format, %j, 1)) $= "%")
		{
			%out = %out @ "%";
			%i = %j++;
			continue;
		}

		if (!%iter.hasNext())
		{
			%out = %out @ getSubStr(%format, %j - 1, %len);
			break;
		}

		// read flags
		//     +: prefix positive numbers with +
		// space: prefix positive numbers with space
		//     -: pad width on the right (left-align) instead
		//     #: use alternate form
		//     0: use 0 instead of spaces to pad width
		%flag_always_sign = 0;
		%flag_space_sign = 0;
		%flag_alternate = 0;
		%padside = 1;
		%padchar = " ";

		while (%j < %len)
		{
			switch$ (%chr)
			{
				case "+": %flag_always_sign = 1;
				case " ": %flag_space_sign = 1;
				case "-": %padside = 0;
				case "#": %flag_alternate = 1;
				case "0": %padchar = "0";
				default: break; // break; in switch in TS breaks outer loop
			}

			%chr = getSubStr(%format, %j++, 1);
		}

		// read width
		%scan = _iterformat_num(%format, %j, %len);

		%j = getWord(%scan, 0);
		%padsize = getWord(%scan, 1);

		if (%padsize !$= "")
		{
			%chr = getSubStr(%format, %j, 1);
			%j++;
		}

		// read precision
		if (%chr $= ".")
		{
			%scan = _iterformat_num(%format, %j, %len);

			%j = getWord(%scan, 0);
			%precision = getWord(%scan, 1);

			if (%precision $= "") // invalid format, add and move along
			{
				%out = %out @ getSubStr(%format, %i, %j - %i);
				%i = %j;
				continue;
			}

			if (%precision !$= "")
			{
				%chr = getSubStr(%format, %j, 1);
				%j++;
			}

			if (%precision == 0)
				%precision = "";
		}
		else
			%precision = "";

		%value = %iter.next();
		%text = "";

		switch$ (%chr)
		{
			case "d" or "i":
				%value |= 0;

				if (%flag_always_sign && %value >= 0)
				{
					echo(%value @ " is >= 0");
					%text = "+" @ %value;
				}
				else if (%flag_space_sign && %value >= 0)
					%text = " " @ %value;
				else
					%text = %value;

			case "u":
				%value |= 0;

				if (%value < 0)
					%text = -%value;
				else
				{
					echo(%value SPC "is not < 0");
					%text = %value;
				}

			case "f":
				%value = mFloatLength(%value, %precision $= "" ? 6 : %precision);

				if (%flag_always_sign)
				{
					if (%value < 0)
						%text = "+" @ %value;
				}
				else if (%flag_space_sign && %value >= 0)
					%text = " " @ %value;
				else
					%text = %value;

			case "e": %text = "<exponent form goes here>";
			case "g": %text = "<dynamic float form goes here>";
			case "x": %text = hex(%value, strcmp(%chr, "X") == 0);
			case "o": %text = ord(%value);
			case "b": %text = bin(%value);
			case "s": %text = %value;
			case "c": %text = isInteger(%value) ? ord(%value) : getSubStr(%value, 0, 1);
			case "p": %text = repr(%value);
			case "n": // consume a value

			default:
				%text = %text @ "%" @ %curr;
		}

		if (%padsize !$= "")
		{
			if (%padside)
				%text = %text @ repeat(%padchar, %padsize - strlen(stripColorChars(%text)));
			else
				%text = repeat(%padchar, %padsize - strlen(stripColorChars(%text))) @ %text;
		}

		%out = %out @ %text;
		%i = %j + 1;
	}

	return %out;
}

function format(%format,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	if (%a0.superClass $= "Iterator" && %a1 $= "")
		return iterformat(%format, %a0);

	for (%count = 19; %count > 0; %count--)
	{
		if (%a[%count - 1] !$= "")
			break;
	}

	%args = ArrayIterator(%count, 1);

	for (%i = 0; %i < %count; %i++)
		%args.value[%i] = %a[%i];
	
	return iterformat(%format, %args);
}

function formatn(
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18, %a19)
{
	if (%a0.superClass $= "Iterator" && %a1 $= "")
	{
		%args = %a0;
		%given = 1;
	}
	else
	{
		for (%count = 20; %count > 0; %count--)
		{
			if (%a[%count - 1] !$= "")
				break;
		}

		%args = ArrayIterator(%count, 1);

		for (%i = 0; %i < %count; %i++)
			%args.value[%i] = %a[%i];
	}

	while (%args.hasNext())
		%text = %text @ iterformat(%args.next(), %args);
	
	if (!%given)
		%args.delete();

	return %text;
}

function repeat(%text, %n)
{
	for (%i = 0; %i < %n; %i++)
		%result = %result @ %text;

	return %result;
}

function chr(%ord)
{
	if ((%ord != %ord | 0) || %ord < 0 || %ord >= 256)
		return "";

	if (!$_ascii_cache)
		_ascii_cache();

	return $_ascii_value[%ord];
}

function ord(%chr)
{
	if (%chr $= "")
		return 0;

	if (!$_ascii_cache)
		_ascii_cache();

	// need case-sensitivity. [] isn't.
	%pos = stripos("abcdefghijklmnopqrstuvwxyz", %chr = getSubStr(%chr, 0, 1));

	if (%pos != -1)
		return (strcmp(%chr, strupr(%chr)) == 0 ? 65 : 97) + %pos;

	return $_ascii_index[%chr];
}

function _ascii_cache()
{
	%hex = "0123456789abcdef";

	for (%i = 0; %i < 16; %i++)
	{
		for (%j = 0; %j < 16; %j++)
		{
			%value = collapseEscape("\\x" @ getSubStr(%hex, %i, 1) @ getSubStr(%hex, %j, 1));
			$_ascii_value[%i * 16 + %j] = %value;
			$_ascii_index[%value] = %i * 16 + %j;
		}
	}

	$_ascii_cache = 1;
}