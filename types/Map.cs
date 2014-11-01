function Map()
{
	return tempref(new ScriptObject()
	{
		class = "Map";
	} @ "\x08");
}

function Map::fromPairs(%seq)
{
	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return 0;

	%map = Map();

	while (%iter.hasNext())
	{
		%next = %iter.next();
		%map.set(%next.value0, %next.value1);
	}

	%iter.delete();
	return %map;
}

function Map::fromKeys(%seq, %value)
{
	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return 0;

	%map = Map();

	// maybe should concat %iter to %map.__keys instead?
	while (%iter.hasNext())
		%map.set(%iter.next(), %value);
	
	%iter.delete();
	return %map;
}

function Map::__isSafe(%key)
{
	return !(
		%key $= "class" || %key $= "superClass" ||
		%key $= "__refer" || %key $= "__keys" ||
		%key $= "___ref" || %key $= "___ref_sched" ||
		getSubStr(%key, 0, 7) $= "__value" ||
		getSubStr(%key, 0, 6) $= "__type"
	);
}

function Map::onAdd(%this)
{
	%this.__keys = ref(Array());
}

function Map::onRemove(%this)
{
	for (%i = 0; %i < %this.__keys.length; %i = (%i + 1) | 0)
		unref(%this.__value[%this.__keys.value[%i]]);

	unref(%this.__keys);
}

function Map::__repr__(%this)
{
	%length = (%keys = %this.__keys).length;

	for (%i = 0; %i < %length; %i = (%i + 1) | 0)
	{
		if (%i)
			%text = %text @ ", ";

		if (_safeid(%key = %keys.value[%i]))
			%text = %text @ %key;
		else
			%text = %text @ repr(%key);

		%text = %text @ ": " @ repr(%this.__value[%key]);
	}

	return "{" @ %text @ "}";
}

function Map::__eq__(%this, %other)
{
	if (%this.class !$= %other.class || %this.__keys.length != %other.__keys.length)
		return 0;

	for (%i = 0; %i < %this.__keys.length; %i = (%i + 1) | 0)
	{
		if (!%other.__keys.contains(%key = %this.__keys.value[%i]))
			return 0;

		if (!eq(%this.__value[%key], %other.__value[%key]))
			return 0;
	}

	return 1;
}

function Map::__iter__(%this)
{
	%iter = ArrayIterator(%length = (%keys = %this.__keys).length);

	for (%i = 0; %i < %length; %i = (%i + 1) | 0)
		%iter.value[%i] = ref(Tuple::fromArgs(%key = %keys.value[%i], %this.__value[%key]));

	return %iter;
}

function Map::copy(%this)
{
	%map = Map();
	%length = (%keys = %this.__keys).length;

	for (%i = 0; %i < %length; %i = (%i + 1) | 0)
		%map.set(%key = %keys.value[%i], %this.__value[%key], %this.__type[%key]);

	return %map;
}

function Map::clear(%this)
{
	%length = (%keys = %this.__keys).length;

	for (%i = 0; %i < %length; %i = (%i + 1) | 0)
	{
		if (Map::__isSafe(%key = %keys.value[%i]))
			%this.setAttribute(%key, "");

		%this.__value[%key] = unref(%this.__value[%key]);
		%this.__type[%key] = "";
	}

	%keys.clear();
	return %this;
}

function Map::get(%this, %key, %default)
{
	if (%this.__keys.contains(%key))
		return %this.__value[%key];

	return %default;
}

function Map::getKeyType(%this, %key)
{
	return %this.__type[%key];
}

function Map::set(%this, %key, %value, %type)
{
	if (!%this.__keys.contains(%key))
		%this.__keys.append(%key);
	else
		unref(%this.__value[%key]);

	%this.__value[%key] = ref(%value);
	%this.__type[%key] = %type;

	if (Map::__isSafe(%key))
		%this.setAttribute(%key, %value);

	return %this;
}

function Map::setDefault(%this, %key, %value, %type)
{
	if (!%this.__keys.contains(%key))
		%this.set(%key, %value, %type);

	return %this.__value[%key];
}

function Map::pop(%this, %key, %default)
{
	%index = %this.__keys.find(%key);

	if (%index != -1)
	{
		%this.__value[%key] = unref(%value = %this.__value[%key]);
		%this.__type[%key] = "";

		if (Map::__isSafe(%key))
			%this.setAttribute(%key, "");

		%this.__keys.pop(%index);
		return %value;
	}

	return %default;
}

function Map::patch(%this, %map)
{
	%length = (%keys = %map.__keys).length;

	for (%i = 0; %i < %length; %i = (%i + 1) | 0)
		%this.set(%key = %keys.value[%i], %map.__value[%key], %map.__type[%key]);

	return %this;
}

function Map::keys(%this)
{
	return %this.__keys.copy();
}

function Map::exists(%this, %key)
{
	return %this.__keys.contains(%key);
}
