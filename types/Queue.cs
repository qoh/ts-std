function Queue(%seq)
{
	%queue = tempref(new ScriptObject()
	{
		class = "Queue";
		index = 0;
		count = 0;
	} @ "\x08");

	if (%seq !$= "")
		%queue.concat(%seq);

	return %queue;
}

function Queue::onRemove(%this)
{
	for (%i = 0; %i < %this.count; %i = (%i + 1) | 0)
		unref(%this.value[(%this.index + %i) | 0]);
}

function Queue::__len__(%this)
{
	return %this.count;
}

function Queue::__iter__(%this)
{
	%iter = ArrayIterator(%this.count);

	for (%i = 0; %i < %this.count; %i = (%i + 1) | 0)
	{
		%index = (%this.index + %i) | 0;
		%iter.value[%i] = ref(%this.value[%index]);
	}

	return %iter;
}

function Queue::__repr__(%this)
{
	return "Queue(" @ join(imap(repr, %this), ", ") @ ")";
}

function Queue::copy(%this)
{
	return Queue(%this);
}

function Queue::empty(%this)
{
	return %this.count == 0;
}

function Queue::clear(%this)
{
	for (%i = 0; %i < %this.count; %i = (%i + 1) | 0)
	{
		%index = (%this.index + %i) | 0;
		%this.value[%index] = unref(%this.value[%index]);
	}

	%this.count = 0;
	%this.index = 0;

	return %this;
}

function Queue::push(%this, %value)
{
	%this.value[%this.count] = ref(%value);
	%this.count = (%this.count + 1) | 0;

	return %this;
}

function Queue::concat(%this, %seq)
{
	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return %this;

	while (%iter.hasNext())
		%this.push(%iter.next());

	%iter.delete();
	return %this;
}

function Queue::pop(%this)
{
	if (%this.count < 1)
		return "";

	%value = %this.value[%this.index];
	%this.value[%this.index] = unref(%value);

	%this.index = (%this.index + 1) | 0;
	%this.count = (%this.count - 1) | 0;

	if (%this.count < 1)
		%this.index = 0;

	return %value;
}

function Queue::peek(%this)
{
	if (%this.count < 1)
		return "";

	return %this.value[%this.index];
}