// note: sets currently index by case-insensitive raw string value; does not use eq()
// raw index lookup is much faster than eq() loop and would be preferred

function Set(%seq)
{
	%set = tempref(new ScriptObject()
	{
		class = "Set";
		left = 0;
		right = -1;
	} @ "\x08");

	if (%seq !$= "")
		%set.concat(%seq);

	return %set;
}

function Set::onRemove(%this)
{
	for (%i = %this.left; %i <= %this.right; %i++)
		unref(%this.value[%i]);
}

function Set::__len__(%this)
{
	return (%this.right - %this.left) + 1;
}

function Set::__repr__(%this)
{
	return "{" @ join(imap(repr, %this), ", ") @ "}";
}

function Set::__eq__(%this, %other)
{
	if (%this.class !$= %other.class || len(%this) != len(%other))
		return 0;

	for (%i = %this.left; %i <= %this.right; %i++)
	{
		if (!%other.contains(%this.value[%i]))
			return 0;
	}

	return 1;
}

function Set::__iter__(%this)
{
	%iter = ArrayIterator((%this.right - %this.left) + 1);

	for (%i = %this.left; %i <= %this.right; %i++)
		%iter.value[%i - %this.left] = ref(%this.value[%i]);

	return %iter;
}

function Set::copy(%this)
{
	%pivot = (%this.right - %this.left) >> 1;

	%set = tempref(new ScriptObject()
	{
		class = "Set";
		left = %this.left - %pivot;
		right = %this.right - %pivot;
	} @ "\x08");

	for (%i = %this.left; %i <= %this.right; %i++)
	{
		%set.value[%i - %pivot] = ref(%this.value[%i]);
		%set.index[%this.value[%i]] = %i - %pivot;
	}

	return %set;
}

function Set::clear(%this)
{
	for (%i = %this.left; %i <= %this.right; %i++)
	{
		%this.index[%value = %this.value[%i]] = "";
		%this.value[%i] = unref(%value);
	}

	%this.left = 0;
	%this.right = -1;

	return %this;
}

function Set::add(%this, %value)
{
	if (%this.index[%value] !$= "")
		return %this;

	%index = mAbs(%this.left) < %this.right ? %this.left-- : %this.right++;

	%this.value[%index] = ref(%value);
	%this.index[%value] = %index;

	return %this;
}

function Set::remove(%this, %value)
{
	if (%this.index[%value] $= "")
		return %this;

	unref(%value);

	%left = %this.index[%value] - %this.left;
	%right = %this.right - %this.index[%value];

	%dir = %left < %right ? -1 : 1;
	%end = (%left < %right ? %this.left : %this.right) + %dir;

	for (%i = %this.index[%value]; %i != %end; %i += %dir)
	{
		%this.value[%i] = %this.value[%i + %dir];
		%this.index[%this.value[%i]] = %i;
	}

	%this.value[%i] = "";
	%this.index[%value] = "";

	if (%left < %right)
		%this.left++;
	else
		%this.right--;

	return %this;
}

function Set::pop(%this, %default)
{
	if (%this.right - %this.left < 0)
		return %default;

	// should consider optimal-pop consideration for when left/right is
	// pushed off it's respective side of 0 by non-optimal usage
	if (%this.right > mAbs(%this.left))
		%index = %this.right-- + 1;
	else
		%index = %this.left++ - 1;

	%value = passref(%this.value[%index]);

	%this.index[%value] = "";
	%this.value[%index] = "";

	return %value;
}

function Set::concat(%this, %seq)
{
	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return %this;

	while (%iter.hasNext())
		%this.add(%iter.next());

	%iter.delete();
	return %this;
}

function Set::contains(%this, %value)
{
	return %this.index[%value] !$= "";
}

// TODO:
// isdisjoint
// issubset
// issuperset
// union
// intersection
// difference
// symmetric_difference
// intersection_update
// difference_update
// symmetric_difference_update