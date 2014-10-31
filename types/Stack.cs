function Stack(%seq)
{
	%stack = tempref(new ScriptObject()
	{
		class = "StackInstance";
		top = -1;
	} @ "\x08");

	if (%seq !$= "")
		%stack.push_all(%seq);

	return %stack;
}

function StackInstance::onRemove(%this)
{
	for (%i = 0; %i < %this.length; %i = (%i + 1) | 0)
		unref(%this.value[%i]);
}

function StackInstance::__len__(%this)
{
	return %this.top + 1;
}

function StackInstance::__iter__(%this)
{
	%iter = ArrayIterator(%this.top + 1, %this._refer);

	for (%i = 0; %i <= %this.top; %i = (%i + 1) | 0)
		%iter.value[%i] = ref(%this.value[%i]);

	return %iter;
}

function StackInstance::__repr__(%this)
{
	return "Stack(" @ join(imap(%this, repr), ", ") @ ")";
}

function StackInstance::copy(%this)
{
	return Stack(%this);
}

function StackInstance::empty(%this)
{
	return %this.top < 0;
}

function StackInstance::clear(%this)
{
	for (%i = 0; %i <= %this.top; %i = (%i + 1) | 0)
		%this.value[%i] = unref(%this.value[%i]);

	%this.top = -1;
	return %this;
}

function StackInstance::push(%this, %value)
{
	%this.value[%this.top = (%this.top + 1) | 0] = ref(%value);
	return %this;
}

function StackInstance::push_all(%this, %seq)
{
	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return %this;

	while (%iter.hasNext())
		%this.push(%iter.next());

	%iter.delete();
	return %this;
}

function StackInstance::pop(%this)
{
	if (%this.top < 0)
		return "";

	%value = %this.value[%this.top];

	%this.value[%this.top] = unref(%value);
	%this.top--;

	return %value;
}

function StackInstance::peek(%this)
{
	if (%this.top < 0)
		return "";

	return %this.value[%this.top];
}