function INI::import(%fileName, %silent)
{
	%data = INIData();
	%read = new FileObject();

	if (!%read.openForRead(%fileName))
	{
		if (!%silent)
			console.error("Cannot open '" @ %fileName @ "' for reading");
		%read.delete();
		return %data;
	}

	%section = %data;

	while (!%read.isEOF())
	{
		%line = ltrim(%read.readLine());

		if (%line $= "")
			continue;

		%first = getSubStr(%line, 0, 1);

		if (%first $= ";" || %first $= "#")
			continue;

		if (%first $= "[")
		{
			%name = trim(getSubStr(%line, 1, strlen(rtrim(%line) - 1)));
			%len = strlen(%name);

			if (getSubStr(%name, %len - 1, 1) !$= "]")
				continue;

			%name = getSubStr(%name, 0, %len - 1);

			if (%section != %data)
				%section.delete();
			
			if (%name $= "")
				%section = %data;
			else
				%section = %data.section(%name);
		}

		%pos = strpos(%line, "=");

		if (%pos == -1)
			continue;

		%key = getSubStr(%line, 0, %pos);
		%value = getSubStr(%line, %pos + 1, strlen(%line));

		if (%section.exists(%key))
		{
			%curr = %section.get(%key);

			if (%curr.class $= "ArrayInstance")
				%curr.append(%value);
			else
				%section.set(%key, Array::fromArgs(%curr, %value));
		}
		else
			%section.set(%key, %value);
	}

	if (%section != %data)
		%section.delete();

	%read.close();
	%read.delete();

	return %data;
}

function INI::export(%data, %fileName, %silent)
{
	%write = new FileObject();

	if (!%write.openForWrite(%fileName))
	{
		if (!%silent)
			console.error("Cannot open '" @ %fileName @ "' for writing");
		%write.delete();
		return 0;
	}

	for (%i = 0; %i < %data.items.__keys.length; %i++)
		_ini_write(%write, %data.items.__keys.value[%i],
			%data.items.__value[%data.items.__keys.value[%i]]);

	for (%i = 0; %i < %data.sections.__keys.length; %i++)
	{
		%name = %data.sections.__keys.value[%i];
		%section = %data.sections.__value[%name];

		%write.writeLine("[" @ %name @ "]");

		for (%i = 0; %i < %section.__keys.length; %i++)
			_ini_write(%write, %section.__keys.value[%i],
				%section.__value[%section.__keys.value[%i]]);
	}

	%write.close();
	%write.delete();

	return 1;
}

function INIData()
{
	return new ScriptObject()
	{
		class = "INIData";

		items = Map("", 1);
		sections = Map("", 1);
	};
}

function INIData::fromFile(%fileName)
{
	%data = INIData();
	%data.import(%fileName);
	return %data;
}

function INIData::onRemove(%this)
{
	%this.items.delete();
	%this.sections.delete();
}

function INIData::copy(%this)
{
	return new ScriptObject()
	{
		class = "INIData";

		// Map::copy needs deepcopy
		items = %this.items.copy();
		sections = %this.sections.copy();
	};
}

function INIData::root(%this)
{
	return %this;
}

function INIData::section(%this, %name)
{
	return tempref(new ScriptObject()
	{
		class = "INISectionProxy";
		data = %this;
		name = %name;
	});
}

function INIData::get(%this, %key, %default)
{
	return %this.items.get(%key, %default);
}

function INIData::set(%this, %key, %value, %default)
{
	if (%default)
		return %this.items.setDefault(%key, %value);

	%this.items.set(%key, %value);
	return %this;
}

function INIData::exists(%this, %key)
{
	return %this.items.exists(%key);
}

function INIData::remove(%this, %key)
{
	return %this.items.remove(%key);
}

function INISectionProxy::root(%this)
{
	return %this.data;
}

function INISectionProxy::empty(%this)
{
	return isObject(%this.data) && %this.data.sections.exists(%this.name);
}

function INISectionProxy::get(%this, %key, %default)
{
	if (%this.empty())
		return "";

	return %this.data.sections.get(%this.name).get(%key, %default);
}

function INISectionProxy::set(%this, %key, %value, %default)
{
	if (!isObject(%this.data))
		return "";

	if (%this.data.sections.exists(%this.name))
		%section = %this.data.sections.get(%this.name);
	else
		%section = %this.data.sections.set(%this.name, Map("", 1));

	if (%default)
		return %section.setDefault(%key, %value);

	%section.set(%key, %value);
	return %this;
}

function INISectionProxy::remove(%this, %key)
{
	if (!%this.exists())
		return %this;

	%section = %this.data.sections.get(%this.name);
	%section.remove(%key);

	if (%section.__keys.length == 0)
		%this.data.sections.remove(%this.name);

	return %this;
}

function INISectionProxy::exists(%this, %key)
{
	return %this.exists() && %this.data.sections.get(%this.name).exists(%key);
}

function _ini_write(%write, %key, %value)
{
	if (%value.class $= "ArrayInstance")
	{
		for (%i = 0; %i < %value.length; %i++)
			%write.writeLine(%key @ "=" @ %value.value[%i]);
	}
	else
		%write.writeLine(%key @ "=" @ %value);
}