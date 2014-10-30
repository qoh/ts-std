function Map(%refer)
{
	return new ScriptObject()
	{
		class = "MapInstance";
		__refer = %refer;
	};
}

function Map::fromPairs(%iterable)
{
	%iter = iter(%iterable);

	if (assert(%iter, "input not iterable"))
		return 0;

	%map = Map(%refer);

	while (%iter.hasNext())
	{
		%next = %iter.next();
		%map.set(%next.value[0], %next.value[1]);
	}

	%iter.delete();
	return %map;
}

function Map::fromKeys(%iterable, %value, %refer)
{
	%iter = iter(%iterable);

	if (assert(%iter, "input not iterable"))
		return 0;

	%map = Map(%refer);

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

function MapInstance::onAdd(%this)
{
	%this.__keys = Array();
}

function MapInstance::onRemove(%this)
{
	if (%this.__refer)
	{
		for (%i = 0; %i < %this.__keys.length; %i = (%i + 1) | 0)
			unref(%this.__value[%this.__keys.value[%i]]);
	}

	%this.__keys.delete();
}

function MapInstance::__repr__(%this)
{
	for (%i = 0; %i < %this.__keys.length; %i = (%i + 1) | 0)
	{
		if (%i)
			%text = %text @ ", ";

		%text = %text @ repr(%this.__keys.value[%i]) @ ": ";
		%text = %text @ repr(%this.__value[%this.__keys.value[%i]]);
	}

	return "{" @ %text @ "}";
}

function MapInstance::__eq__(%this, %other)
{
	if (%this.class != %other.class)
		return 0;

	if (%this.__keys.length != %other.__keys.length)
		return 0;

	for (%i = 0; %i < %this.__keys.length; %i = (%i + 1) | 0)
	{
		if (!%other.__keys.contains(%this.__keys.value[%i]))
			return 0;

		if (!eq(%this.__value[%this.__keys.value[%i]], %other.__value[%this.__keys.value[%i]]))
			return 0;
	}

	return 1;
}

function MapInstance::__iter__(%this)
{
	%iter = ArrayIterator(%this.__keys.length, 1);

	for (%i = 0; %i < %this.__keys.length; %i = (%i + 1) | 0)
		%iter.value[%i] = Tuple::fromArgs(%this.__keys.value[%i],
							%this.__value[%this.__keys.value[%i]]);

	return %iter;
}

function MapInstance::copy(%this)
{
	%map = Map(%this.__refer);

	for (%i = 0; %i < %this.__keys.length; %i = (%i + 1) | 0)
		%map.set(%this.__keys.value[%i], %this.__value[%this.__keys.value[%i]], %this.__type[%this.__keys.value[%i]]);

	return %map;
}

function MapInstance::clear(%this)
{
	for (%i = 0; %i < %this.__keys.length; %i = (%i + 1) | 0)
	{
		%key = %this.__keys.value[%i];
		
		if (%this.__refer)
			unref(%this.__value[%key]);
		
		if (Map::__isSafe(%key))
			%this.setAttribute(%key, "");

		%this.__value[%key] = "";
		%this.__type[%key] = "";
	}

	%this.__keys.clear();
}

function MapInstance::get(%this, %key, %default)
{
	if (%this.__keys.contains(%key))
		return %this.__value[%key];

	return %default;
}

function MapInstance::getType(%this, %key)
{
	return %this.__type[%key];
}

function MapInstance::set(%this, %key, %value, %type)
{
	if (!%this.__keys.contains(%key))
		%this.__keys.append(%key);
	else if (%this.__refer)
		unref(%this.__value[%key]);

	if (%this.__refer)
		ref(%value);

	%this.__value[%key] = %value;
	%this.__type[%key] = %value;

	if (Map::__isSafe(%key))
		setattr(%this, %key, %value);

	return %value;
}

function MapInstance::setDefault(%this, %key, %value, %type)
{
	if (!%this.__keys.contains(%key))
		%this.set(%key, %value, %type);

	return %this.__value[%key];
}

function MapInstance::pop(%this, %key, %default)
{
	%index = %this.__keys.find(%key);

	if (%index != -1)
	{
		%value = %this.__value[%key];
		%this.__value[%key] = "";
		%this.__type[%key] = "";

		if (Map::__isSafe(%key))
			setattr(%this, %key, "");

		%this.__keys.pop(%index);
		return %value;
	}

	return %default;
}

function MapInstance::patch(%this, %map)
{
	for (%i = 0; %i < %map.__keys.length; %i = (%i + 1) | 0)
		%this.set(%map.__keys.value[%i], %map.__value[%map.__keys.value[%i]], %map.__type[%map.__keys.value[%i]]);
}

function MapInstance::keys(%this)
{
	return %this.__keys.copy();
}

function MapInstance::exists(%this, %key)
{
	return %this.__keys.contains(%key);
}
