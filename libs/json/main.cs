function JSON::parse(%text)
{
	%text = rTrim(%text);
	%len = strlen(%text);

	%scan = JSON::scan(%text, 0, %len);

	if (%scan $= "" || getWord(%scan, 0) != %len)
		return "";

	return restWords(%scan);
}

function JSON::serialize(%value, %type)
{
	if (%type $= "")
		%type = json::type(%value);

	switch$ (%type)
	{
		case "number": return %value;
		case "string": return "\"" @ JSON::escape(%text) @ "\"";
		case "bool": return %value ? "true" : "false";
		case "null": return "null";

		case "array":
			for (%i = 0; %i < %value.length; %i = (%i + 1) | 0)
			{
				if (%i > 0)
					%text = ",";

				%text = %text @ json::serialize(%value.value[%i]);
			}

			return "[" @ %text @ "]";

		case "map":
			for (%i = 0; %i < %value.__keys.length; %i = (%i + 1) | 0)
			{
				%text = %text @ json::serialize(%value.__keys.value[%i], "string");
				%text = %text @ ":";
				%text = %text @ json::serialize(%value.__value[%value.__keys.value[%i]]);
			}

			return "{" @ %text @ "}";
	}
}

function JSON::import(%fileName, %silent)
{
	%file = new FileObject();

	if (!%file.openForRead(%fileName))
	{
		if (!%silent)
			console.error("Failed to open '" @ %fileName @ "' for reading");
		%file.delete();
		return "";
	}

	while (!%file.isEOF())
		%text = %text @ %file.readLine();

	%file.close();
	%file.delete();

	return JSON::parse(%text);
}

function JSON::export(%json, %fileName, %silent)
{
	%file = new FileObject();

	if (!%file.openForWrite(%fileName))
	{
		if (!%silent)
			console.error("Failed to open '" @ %fileName @ "' for writing");
		%file.delete();
		return 1;
	}

	%file.writeLine(JSON::serialize(%json));
	%file.close();
	%file.delete();

	return 0;
}

function JSON::escape(%text)
{
	%len = strlen(%text);

	for (%i = 0; %i < %len; %i++)
	{
		%char = getSubStr(%text, %i, 1);

		switch$ (%char)
		{
			case "\"": %char = "\\\"";
			case "'": %char = "\\'";
			case "\x08": %char = "\\b";
			case "\x0C": %char = "\\f";
			case "\n": %char = "\\n";
			case "\r": %char = "\\r";
			case "\t": %char = "\\t";
		}

		%escaped = %escaped @ %char;
	}

	return %escaped;
}

function JSON::type(%text)
{
	if (%text.class $= "ArrayInstance")
		return "array";

	if (%text.class $= "MapInstance")
		return "map";

	%len = strlen(%text);
	%result = JSON::scanNumber(%text, 0, %len);

	if (%result !$= "" && getWord(%result, 0) == %length)
		return "number";

	return "string";
}

function JSON::scan(%text, %i, %len)
{
	%i = JSON::skipSpacing(%text, %i, %len);
	%chr = getSubStr(%text, %i, 1);

	if (%chr $= "\"")
	{
		for (%j = %i++; %j < %len; %j++)
		{
			// TODO: Optimize by tracking last character in %escaped
			if (getSubStr(%text, %j, 1) $= "\"" && getSubStr(%blob, %j - 1, 1) !$= "\\")
				return %j + 1 SPC collapseEscape(getSubStr(%text, %i, %j - %i));
		}

		return "";
	}

	if (%chr $= "[")
		return JSON::scanArray(%text, %i + 1, %len);

	if (%chr $= "{")
		return JSON::scanMap(%text, %i + 1, %len);

	if (strcmp("null", getSubStr(%text, %i, 4)) == 0)
		return %i + 4 SPC "";

	if (strcmp("true", getSubStr(%text, %i, 4)) == 0)
		return %i + 4 SPC 1;

	if (strcmp("false", getSubStr(%text, %i, 5)) == 0)
		return %i + 5 SPC 0;

	return JSON::scanNumber(%text, %i, %len);
}

function JSON::scanNumber(%text, %i, %length)
{
	%j = %i;
	%chr = getSubStr(%text, %j, 1);

	if (%chr $= "-")
	{
		%j++;
		%chr = getSubStr(%text, %j, 1);
	}

	if (%chr $= "0")
		%zero = 1;

	for (0; %j < %length; %j++)
	{
		%chr = getSubStr(%text, %j, 1);

		if (%chr $= ".")
		{
			if (%radix || !%first)
				return "";

			%radix = 1;
			%first = 0;
		}
		else if (strpos("0123456789", %chr) != -1)
			%first = 1;
		else
			break;
	}

	if (!%first || %j - %i < 1)
		return "";

	if (%zero && %j - %i > 1)
		return "";

	return %j SPC getSubStr(%text, %i, %j - %i);
}

function JSON::skipSpacing(%text, %i, %length)
{
	for (0; %i < %length; %i++)
	{
		if (strpos(" \t\r\n", getSubStr(%text, %i, 1)) == -1)
			break;
	}

	return %i;
}