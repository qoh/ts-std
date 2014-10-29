function EngineSet(%seq)
{
	%set = new ScriptObject()
	{
		class = "EngineSet";
	};

	if (%seq !$= "")
		%set.addAll(%seq);

	return %set;
}

function EngineSet::onAdd(%this)
{
	%this.set = new SimSet();
}

function EngineSet::onRemove(%this)
{
	%count = %this.set.getCount();

	for (%i = 0; %i < %count; %i++)
		unref(%this.set.getObject(%i));

	%this.set.delete();
}

function EngineSet::__len__(%this)
{
	return %this.set.getCount();
}

function EngineSet::__iter__(%this)
{
	%count = %this.set.getCount();
	%iter = ArrayIterator(%count);

	for (%i = 0; %i < %count; %i++)
		%iter.value[%i] = %this.set.getObject(%i);

	return %iter;
}

function EngineSet::add(%this, %obj)
{
	if (isObject(%obj) && !%this.set.isMember(%obj))
		%this.set.add(ref(%obj));

	return %this;
}

function EngineSet::addAll(%this, %seq)
{
	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return %this;

	while (%iter.hasNext())
		%this.add(%iter.next());

	return %this;
}

function EngineSet::remove(%this, %obj)
{
	if (isObject(%obj) && %this.set.isMember(%obj))
	{
		unref(%obj);
		%this.set.remove(%obj);
	}

	return %this;
}

function EngineSet::contains(%this, %obj)
{
	return %this.set.isMember(%obj);
}