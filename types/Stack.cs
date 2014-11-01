function Stack(%seq)
{
	%stack = tempref(new ScriptObject()
	{
		class = "Stack";
		top = -1;
	} @ "\x08");

	if (%seq !$= "")
		%stack.concat(%seq);

	return %stack;
}

function Stack::onRemove(%this)
{
	for (%i = 0; %i < %this.length; %i = (%i + 1) | 0)
		unref(%this.value[%i]);
}

function Stack::__len__(%this)
{
	return %this.top + 1;
}

function Stack::__iter__(%this)
{
	%iter = ArrayIterator(%this.top + 1);

	for (%i = 0; %i <= %this.top; %i = (%i + 1) | 0)
		%iter.value[%i] = ref(%this.value[%i]);

	return %iter;
}

function Stack::__repr__(%this)
{
	return "Stack(" @ join(imap(repr, %this), ", ") @ ")";
}

function Stack::copy(%this)
{
	return Stack(%this);
}

function Stack::empty(%this)
{
	return %this.top < 0;
}

function Stack::clear(%this)
{
	for (%i = 0; %i <= %this.top; %i = (%i + 1) | 0)
		%this.value[%i] = unref(%this.value[%i]);

	%this.top = -1;
	return %this;
}

function Stack::push(%this, %value)
{
	%this.value[%this.top = (%this.top + 1) | 0] = ref(%value);
	return %this;
}

function Stack::pop(%this)
{
	if (%this.top < 0)
		return "";

	%value = %this.value[%this.top];

	%this.value[%this.top] = unref(%value);
	%this.top--;

	return %value;
}

function Stack::peek(%this)
{
	if (%this.top < 0)
		return "";

	return %this.value[%this.top];
}

function Stack::concat(%this, %seq)
{
	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return %this;

	while (%iter.hasNext())
		%this.push(%iter.next());

	%iter.delete();
	return %this;
}