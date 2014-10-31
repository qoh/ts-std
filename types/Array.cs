function Array(%iterable, %refer)
{
	%array = new ScriptObject()
	{
		class = "ArrayInstance";
		length = 0;
		_refer = %refer;
	};

	if (%iterable !$= "")
		%array.concat(%iterable);

	return %array;
}

function Array::fromArgs(
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18, %a19)
{
	for (%length = 20; %length > 0; %length--)
	{
		if (%a[%length - 1] !$= "")
			break;
	}

	%array = new ScriptObject()
	{
		class = "ArrayInstance";
		length = %length;
		_refer = 1;
	};

	for (%i = 0; %i < %length; %i++)
		%array.value[%i] = ref(%a[%i]);

	return %array;
}

function Array::split(%text, %separator, %refer)
{
	if (%separator $= "")
		%separator = " ";

	%array = new ScriptObject()
	{
		class = "ArrayInstance";
		length = 0;
		_refer = %refer;
	};

	while (%text !$= "")
	{
		%text = nextToken(%text, "value", %separator);
		%array.append(%value);
	}

	return %array;
}

function ArrayInstance::onRemove(%this)
{
	if (%this._refer)
	{
		for (%i = 0; %i < %this.length; %i = (%i + 1) | 0)
			unref(%this.value[%i]);
	}
}

function ArrayInstance::__eq__(%this, %value)
{
	if (%value.class !$= %this.class || %value.length != %this.length)
		return 0;

	for (%i = 0; %i < %this.length; %i = (%i + 1) | 0)
	{
		if (!eq(%value.value[%i], %this.value[%i]))
			return 0;
	}

	return 1;
}

function ArrayInstance::__iter__(%this)
{
	%iter = ArrayIterator(%this.length, %this._refer);

	for (%i = 0; %i < %this.length; %i = (%i + 1) | 0)
	{
		if (%this._refer)
			ref(%this.value[%i]);

		%iter.value[%i] = %this.value[%i];
	}

	return %iter;
}

function ArrayInstance::__len__(%this)
{
	return %this.length;
}

function ArrayInstance::__repr__(%this)
{
	return "[" @ join(imap(repr, %this), ", ") @ "]";
}

function ArrayInstance::__add__(%this, %other)
{
	if (assert(%other.class == %this.class, "other must be Array for Array + other"))
		return "";

	%array = %this.copy();
	%array.concat(%other);
	
	return %array;
}

function ArrayInstance::__mul__(%this, %n)
{
	%array = Array("", %this._refer);

	for (%i = 0; %i < %n; %i = (%i + 1) | 0)
		%array.concat();

	return %array;
}

function ArrayInstance::copy(%this)
{
	return Array(%this, %this._refer);
}

function ArrayInstance::clear(%this)
{
	for (%i = 0; %i < %this.length; %i = (%i + 1) | 0)
	{
		if (%this._refer)
			unref(%this.value[%i]);

		%this.value[%i] = "";
		%this.type[%i] = "";
	}

	%this.length = 0;
	return %this;
}

function ArrayInstance::append(%this, %value, %type)
{
	if (%this._refer)
		ref(%value);

	%this.value[%this.length] = %value;
	%this.type[%this.length] = %type;
	%this.length = (%this.length + 1) | 0;

	return %this;
}

function ArrayInstance::insert(%this, %index, %value, %type)
{
	if (assert(%index >= 0 && %index < %this.length, "invalid array index"))
		return;

	if (%this._refer)
		ref(%value);

	for (%i = %this.length; %i > %index; %i = (%i - 1) | 0)
	{
		%this.value[%i] = %this.value[(%i - 1) | 0];
		%this.type[%i] = %this.type[(%i - 1) | 0];
	}

	%this.length = (%this.length + 1) | 0;
	%this.value[%index] = %value;
	%this.type[%index] = %type;

	return %this;
}

function ArrayInstance::pop(%this, %i)
{
	if (%i $= "")
	{
		if (%this.length < 1)
			return "";

		%i = %this.length - 1;
	}

	if (assert(%i >= 0 && %i <= %this.length, "Invalid array index"))
		return "";

	%value = %this.value[%i];

	if (%this._refer)
		unref(%value);

	for (0; %i < %this.length; %i = (%i + 1) | 0)
	{
		%this.value[%i] = %this.value[(%i + 1) | 0];
		%this.type[%i] = %this.type[(%i + 1) | 0];
	}

	%this.length = (%this.length - 1) | 0;
	%this.value[%this.length] = "";
	%this.type[%this.length] = "";

	return %value;
}

function ArrayInstance::find(%this, %value)
{
	for (%i = 0; %i < %this.length; %i = (%i + 1) | 0)
	{
		if (eq(%value, %this.value[%i]))
			return %i;
	}

	return -1;
}

function ArrayInstance::contains(%this, %value)
{
	return %this.find(%value) != -1;
}

function ArrayInstance::count(%this, %value)
{
	%n = 0;

	for (%i = 0; %i < %this.length; %i = (%i + 1) | 0)
	{
		if (eq(%value, %this.value[%i]))
			%n = (%n + 1) | 0;
	}

	return %n;
}

function ArrayInstance::remove(%this, %value)
{
	%index = %this.find(%value);

	if (%index != -1)
		%this.pop(%index);

	return %this;
}

function ArrayInstance::swap(%this, %i, %j)
{
	%temp = %this.value[%i];
	%this.value[%i] = %this.value[%j];
	%this.value[%j] = %temp;
	
	%temp = %this.type[%i];
	%this.type[%i] = %this.type[%j];
	%this.type[%j] = %temp;

	return %this;
}

function ArrayInstance::concat(%this, %seq)
{
	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return %this;

	while (%iter.hasNext())
		%this.append(%iter.next());

	%iter.delete();
	return %this;
}

function ArrayInstance::reverse(%this)
{
	%max = (%this.length - 2) >> 1;

	for (%i = 0; %i < %max; %i = (%i + 1) | 0)
		%this.swap(%i, (%this.length - 1 - %i) | 0);

	return %this;
}

function ArrayInstance::map(%this, %target)
{
	for (%i = 0; %i < %this.length; %i = (%i + 1) | 0)
	{
		if (%this._refer)
			unref(%this.value[%i]);

		%this.value[%i] = dynCall(%target, %this.value[%i]);

		if (%this._refer)
			ref(%this.value[%i]);
	}

	return %this;
}

function ArrayInstance::filter(%this, %target)
{
	%result = Array("", %this._refer);

	for (%i = 0; %i < %this.length; %i = (%i + 1) | 0)
	{
		if (dynCall(%target, %this.value[%i]))
			%result.append(%this.value[%i]);
	}

	return %result;
}

function ArrayInstance::search(%this, %value, %comparator)
{
	return %this._binarySearch(%value, %comparator, 0, %this.length - 1);
}

function ArrayInstance::_binarySearch(%this, %value, %comparator, %low, %high)
{
	%mid = ((%high - %low) >> 1) + %low;
	%res = call(%comparator, %this.value[%mid], %value);

	if (%high < %low)
		return %mid + 1;

	if (%res > 0)
		return %this._binarySearch(%This, %value, %comparator, %low, %mid - 1);
	else if (%res < 0)
		return %this._binarySearch(%This, %value, %comparator, %mid + 1, %high);
	else
		return %mid;
}

function ArrayInstance::sort(%this, %cmp)
{
	if (%cmp $= "")
		%cmp = "cmp";

	_qsort(%this, 0, %this.length);
}

function ArrayInstance::appendSorted(%this, %value, %comparator)
{
	%index = %this.search(%value, %comparator);
	return %this.insert(%index, %value);
}
