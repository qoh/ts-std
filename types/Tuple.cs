// Tuple Tuple(Iterable seq)
function Tuple(%seq)
{
	if (%seq !$= "" && assert(%iter = iter(%seq), "seq is not iterable"))
		return 0;

	%tuple = new ScriptObject()
	{
		class = "TupleInstance";
		length = 0;
	};

	if (%seq !$= "")
	{
		while (%iter.hasNext())
		{
			%tuple.value[%tuple.length] = ref(%iter.next());
			%tuple.length = (%tuple.length + 1) | 0;
		}
	}

	%iter.delete();
	return tempref(%tuple);
}

function Tuple::fromArgs(
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18, %a19)
{
	for (%length = 20; %length > 0; %length--)
	{
		if (%a[%length - 1] !$= "")
			break;
	}

	%tuple = new ScriptObject()
	{
		class = "TupleInstance";
		length = %length;
	};

	for (%i = 0; %i < %length; %i++)
		%tuple.value[%i] = ref(%a[%i]);

	return tempref(%tuple);
}

function TupleInstance::onRemove(%this)
{
	for (%i = 0; %i < %this.length; %i = (%i + 1) | 0)
		unref(%this.value[%i]);
}

function TupleInstance::__eq__(%this, %other)
{
	if (%other.class !$= "TupleInstance" || %other.length != %this.length)
		return 0;

	for (%i = 0; %i < %this.length; %i = (%i + 1) | 0)
	{
		if (!eq(%other.value[%i], %this.value[%i]))
			return 0;
	}

	return 1;
}

function TupleInstance::__iter__(%this)
{
	%iter = ArrayIterator(%this.length, 1);

	for (%i = 0; %i < %this.length; %i = (%i + 1) | 0)
		%iter.value[%i] = ref(%this.value[%i]);

	return %iter;
}

function TupleInstance::__len__(%this)
{
	return %this.length;
}

function TupleInstance::__repr__(%this)
{
	return "(" @ join(imap(repr, %this), ", ") @ ")";
}

function TupleInstance::__add__(%this, %other)
{
	if (assert(%iter = iter(%other), "other is not iterable for Tuple add"))
		return "";

	%tuple = %this.copy();
	
	while (%iter.hasNext())
	{
		%tuple.value[%tuple.length] = ref(%iter.next());
		%tuple.length = (%tuple.length + 1) | 0;
	}
	
	%iter.delete();
	return %tuple;
}

function TupleInstance::__mul__(%this, %n)
{
	%n |= 0;

	if (assert(%n >= 0, "other cannot be negative for Tuple mul"))
		return 0;

	%tuple = new ScriptObject()
	{
		class = "Tuple";
		length = %this.length * %n;
	};

	for (%i = 0; %i < %n; %i = (%i + 1) | 0)
	{
		%j = %this.length * %i;

		for (%k = 0; %k < %this.length; %k = (%k + 1) | 0)
			%tuple.value[%j + %k] = %this.value[%k];
	}

	return %tuple;
}

function TupleInstance::copy(%this)
{
	return Tuple(%this);
}

function TupleInstance::find(%this, %value)
{
	for (%i = 0; %i < %this.length; %i = (%i + 1) | 0)
	{
		if (eq(%value, %this.value[%i]))
			return %value;
	}

	return -1;
}

function TupleInstance::contains(%this, %value)
{
	return %this.find(%value) != -1;
}

function TupleInstance::count(%this, %value)
{
	%n = 0;

	for (%i = 0; %i < %this.length; %i = (%i + 1) | 0)
	{
		if (eq(%value, %this.value[%i]))
			%n = (%n + 1) | 0;
	}

	return %n;
}