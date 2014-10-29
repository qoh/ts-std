function RedisReplyParser(%client)
{
	return new ScriptObject()
	{
		class = "RedisReplyParser";
		client = %client;
	};
}

function RedisReplyParser::onAdd(%this)
{
	%this.lines = Queue();
}

function RedisReplyParser::onRemove(%this)
{
	%this.lines.delete();
}

function RedisReplyParser::onLine(%this, %line)
{
	if (strpos(%line, "\r\n") != -1)
	{
		while (%line !$= "")
		{
			%line = nextToken(%line, "value", "\r\n");
			%this.lines.push(%value);
		}
	}
	else
		%this.lines.push(%line);

	while (!%this.lines.empty())
	{
		%read = %this.readValue(0);

		if (%read $= "")
			break;

		%this.lines.shift(firstWord(%read));
		%this.client.onReply(restWords(%read));
	}
}

function RedisReplyParser::readValue(%this, %index)
{
	if (%index >= %this.lines.getCount())
		return "";

	%line = %this.lines.value[%index];
	%index++;

	%type = getSubStr(%line, 0, 1);
	%data = getSubStr(%line, 1, strLen(%line));

	if (%type $= "+" || %type $= "-" || %type $= ":")
	{
		if (%type $= ":")
			%data |= 0;

		if (%type $= "-")
			%data = "\c0" @ %data;

		return %index SPC %data;
	}

	if (%type $= "*")
	{
		if (%this.lines.getCount() - %index < %size)
			return "";

		%array = Array();

		for (%i = 0; %i < %data; %i++)
		{
			%read = %this.readValue(%index);

			if (%read $= "")
			{
				%array.delete();
				return "";
			}

			%index = firstWord(%read);
			%array.append(restWords(%read));
		}

		return %index SPC tempref(%array);
	}

	if (%type $= "$")
	{
		if (%data < 0)
			return %index SPC "";

		// now, we basically need to iterate through the upcoming lines until
		// reaching the intended size (concatenating them with \r\n), HACKHACK.
		%count = %this.lines.getCount();

		while (%index < %count && strlen(%bulk) < %data)
		{
			if (%bulk !$= "")
				%bulk = %bulk @ "\r\n";

			%bulk = %bulk @ %this.lines.value[%index];
			%index++;
		}

		if (strlen(%bulk) < %data)
			return "";

		return %index SPC getSubStr(%bulk, 0, %data);
	}

	%this.lines.clear();
	return "";
}