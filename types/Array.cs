function Array(%seq)
{
	%array = tempref(new ScriptObject()
	{
		class = "ArrayInstance";
		length = 0;
	} @ "\x08");

	if (%seq !$= "")
		%array.concat(%seq);

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

	%array = tempref(new ScriptObject()
	{
		class = "ArrayInstance";
		length = %length;
	} @ "\x08");

	for (%i = 0; %i < %length; %i++)
		%array.value[%i] = ref(%a[%i]);

	return %array;
}

function Array::split(%text, %separator)
{
	if (%separator $= "")
		%separator = " ";

	%array = tempref(new ScriptObject()
	{
		class = "ArrayInstance";
		length = 0;
	} @ "\x08");

	while (%text !$= "")
	{
		%text = nextToken(%text, "value", %separator);
		%array.append(%value);
	}

	return %array;
}

function ArrayInstance::onRemove(%this)
{
	for (%i = 0; %i < %this.length; %i++)
		unref(%this.value[%i]);
}

function ArrayInstance::__eq__(%this, %value)
{
	if (%value.class !$= %this.class || %value.length != %this.length)
		return 0;

	for (%i = 0; %i < %this.length; %i++)
	{
		if (!eq(%value.value[%i], %this.value[%i]))
			return 0;
	}

	return 1;
}

function ArrayInstance::__iter__(%this)
{
	%iter = ArrayIterator(%this.length);

	for (%i = 0; %i < %this.length; %i++)
		%iter.value[%i] = ref(%this.value[%i]);

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
	%array = Array();

	for (%i = 0; %i < %n; %i++)
		%array.concat();

	return %array;
}

function ArrayInstance::copy(%this)
{
	%array = tempref(new ScriptObject()
	{
		class = "ArrayInstance";
		length = %this.length;
	} @ "\x08");

	for (%i = 0; %i < %this.length; %i++)
		%array.value[%i] = ref(%this.value[%i]);

	return %array;
}

function ArrayInstance::clear(%this)
{
	for (%i = 0; %i < %this.length; %i++)
	{
		%this.value[%i] = unref(%this.value[%i]);
		%this.type[%i] = "";
	}

	%this.length = 0;
	return %this;
}

function ArrayInstance::append(%this, %value, %type)
{
	%this.value[%this.length] = ref(%value);
	%this.type[%this.length] = %type;
	%this.length++;

	return %this;
}

function ArrayInstance::insert(%this, %index, %value, %type)
{
	if (assert(%index >= 0 && %index < %this.length, "invalid array index"))
		return;

	for (%i = %this.length; %i > %index; %i--)
	{
		%this.value[%i] = %this.value[%i - 1];
		%this.type[%i] = %this.type[%i - 1];
	}

	%this.length = (%this.length + 1) | 0;
	%this.value[%index] = ref(%value);
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

	%value = passref(%this.value[%i]);

	for (0; %i < %this.length; %i++)
	{
		%this.value[%i] = %this.value[%i + 1];
		%this.type[%i] = %this.type[%i + 1];
	}

	%this.value[%this.length--] = "";
	%this.type[%this.length] = "";

	return %value;
}

function ArrayInstance::find(%this, %value)
{
	for (%i = 0; %i < %this.length; %i++)
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

	for (%i = 0; %i < %this.length; %i++)
	{
		if (eq(%value, %this.value[%i]))
			%n++;
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

	for (%i = 0; %i < %max; %i++)
		%this.swap(%i, %this.length - 1 - %i);

	return %this;
}

function ArrayInstance::shuffle(%this)
{
	%length = %this.length - 1;

	for (%i = 0; %i <= %length; %i++)
		%this.swap(getRandom(%i, %length), %i); // should use Random lib
}

// %key does not work correctly in some cases
// also seems to somehow destroy objects in the array at random
// do not use
function ArrayInstance::sort(%this, %key)
{
	_qsort(%this, 0, %this.length - 1, %key);
	return %this;
}

function ArrayInstance::map(%this, %target)
{
	for (%i = 0; %i < %this.length; %i++)
		%this.value[%i] = ref(dynCall(%target, passref(%this.value[%i])));

	return %this;
}

function ArrayInstance::filter(%this, %target)
{
	%result = Array();

	for (%i = 0; %i < %this.length; %i++)
	{
		if (dynCall(%target, %this.value[%i]))
			%result.append(%this.value[%i]);
	}

	return %result;
}

function ArrayInstance::search(%this, %value, %key)
{
	if (%key !$= "")
		%value = dynCall(%key, %value);
	return %this._binarySearch(%value, %key, 0, %this.length - 1);
}

function ArrayInstance::_binarySearch(%this, %value, %key, %low, %high)
{
	// this currently crashes the engine
	return 0;
	%mid = (((%high - %low) >> 1) + %low) | 0;
	%res = cmp(%key $= "" ? %this.value[%mid] : dynCall(%key, %this.value[%mid]), %value);

	if (%high < %low)
		return (%mid + 1) | 0;

	if (%res > 0)
		return %this._binarySearch(%this, %value, %key, %low, (%mid - 1) | 0);
	else if (%res < 0)
		return %this._binarySearch(%this, %value, %key, (%mid + 1) | 0, %high);
	else
		return %mid;
}

function ArrayInstance::appendSorted(%this, %value, %key)
{
	%index = %this.search(%value, %key);
	return %this.insert(%index, %value);
}