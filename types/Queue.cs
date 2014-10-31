function Queue(%seq, %refer)
{
	%queue = tempref(new ScriptObject()
	{
		class = "QueueInstance";
		index = 0;
		count = 0;
		_refer = %refer;
	} @ "\x08");

	if (%seq !$= "")
		%queue.push_all(%seq);

	return %queue;
}

function QueueInstance::onRemove(%this)
{
	if (%this._refer)
	{
		for (%i = 0; %i < %this.count; %i = (%i + 1) | 0)
			unref(%this.value[(%this.index + %i) | 0]);
	}
}

function QueueInstance::__len__(%this)
{
	return %this.count;
}

function QueueInstance::__iter__(%this)
{
	%iter = ArrayIterator(%this.count, %this._refer);

	for (%i = 0; %i < %this.count; %i = (%i + 1) | 0)
	{
		%index = (%this.index + %i) | 0;

		if (%this._refer)
			ref(%this.value[%index]);

		%iter.value[%index] = %this.value[%index];
	}

	return %iter;
}

function QueueInstance::__repr__(%this)
{
	return "Queue(" @ join(imap(%this, repr), ", ") @ ")";
}

function QueueInstance::copy(%this)
{
	return Queue(%this, %this._refer);
}

function QueueInstance::empty(%this)
{
	return %this.count == 0;
}

function QueueInstance::clear(%this)
{
	for (%i = 0; %i < %this.count; %i = (%i + 1) | 0)
	{
		%index = (%this.index + %i) | 0;

		if (%this._refer)
			unref(%this.value[%index]);

		%this.value[%index] = "";
	}

	%this.count = 0;
	%this.index = 0;

	return %this;
}

function QueueInstance::push(%this, %value)
{
	if (%this._refer)
		ref(%value);

	%this.value[%this.count] = %value;
	%this.count = (%this.count + 1) | 0;

	return %this;
}

function QueueInstance::push_all(%this, %seq)
{
	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return %this;

	while (%iter.hasNext())
		%this.push(%iter.next());

	%iter.delete();
	return %this;
}

function QueueInstance::pop(%this)
{
	if (%this.count < 1)
		return "";

	%value = %this.value[%this.index];

	if (%this._refer)
		unref(%value);

	%this.value[%this.index] = "";

	%this.index = (%this.index + 1) | 0;
	%this.count = (%this.count - 1) | 0;

	if (%this.count < 1)
		%this.index = 0;

	return %value;
}

function QueueInstance::peek(%this)
{
	if (%this.count < 1)
		return "";

	return %this.value[%this.index];
}