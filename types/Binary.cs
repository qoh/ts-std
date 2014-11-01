function ByteMap()
{
	return tempref(new ScriptObject()
	{
		class = "ByteMap";
	} @ "\x08");
}

function ByteMap::get(%this, %i)
{
	return %this.int[%i >> 2] >> ((%i & 3) << 3) & 255;
}

function ByteMap::set(%this, %i, %v)
{
	if (%this.int[%i >> 2] != 0)
		%this.int[%i >> 2] &= ~(0xFF << ((%i & 3) << 3));

	%this.int[%i >> 2] |= (%v & 255) << ((%i & 3) << 3);
	return %v;
}

function ByteArray(%init)
{
	%array = tempref(new ScriptObject()
	{
		class = "ByteArray";
		superClass = "ByteMap";
		size = 0;
	} @ "\x08");

	if (%init !$= "")
		%array.concat(%init);

	return %array;
}

function ByteArray::fromString(%str)
{
	%len = strlen(%str);

	%array = tempref(new ScriptObject()
	{
		class = "ByteArray";
		superClass = "ByteMap";
		size = %len;
	} @ "\x08");

	for (%i = 0; %i < %len; %i = (%i + 1) | 0)
		%array.set(%i, ord(getSubStr(%str, %i, 1)));

	return %array;
}

function ByteArray::__len__(%this)
{
	return %this.size;
}

function ByteArray::__iter__(%this)
{
	%iter = ArrayIterator(%this.size);

	for (%i = 0; %i < %this.size; %i = (%i + 1) | 0)
		%iter.value[%i] = %this.int[%i >> 2] >> ((%i & 3) << 3) & 255;

	return %iter;
}

function ByteArray::__repr__(%this)
{
	return "ByteArray[" @ join(imap(hex, %this), ", ") @ "]";
}

function ByteArray::copy(%this)
{
	%array = tempref(new ScriptObject()
	{
		class = "ByteArray";
		superClass = "ByteMap";
		size = %this.size;
	} @ "\x08");

	%ints = (((%this.size - 1) >> 2) + 1) | 0;
	for (%i = 0; %i < %ints; %i = (%i + 1) | 0)
		%array.int[%i] = %this.int[%i];

	return %array;
}

function ByteArray::clear(%this)
{
	if (%this.size > 0)
	{
		%ints = (((%this.size - 1) >> 2) + 1) | 0;
		for (%i = 0; %i < %ints; %i = (%i + 1) | 0)
			%this.int[%i] = "";
	}

	%this.size = 0;
	return %this;
}

function ByteArray::append(%this, %value)
{
	if (assert((%value | 0) $= %value && %value >= 0 && %value < 256, "value must integer where be 0 <= value < 256"))
		return 0;

	%this.size = ((%i = %this.size) + 1) | 0;
	%this.set(%i, %value);

	return 1;
}

function ByteArray::appendChar(%this, %chr)
{
	return %this.append(ord(%chr));
}

function ByteArray::concat(%this, %seq)
{
	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return 0;

	while (%iter.hasNext())
	{
		if (!%this.append(%iter.next()))
		{
			%iter.delete();
			return 0;
		}
	}

	%iter.delete();
	return 1;
}

function ByteArray::concatString(%this, %str)
{
	%i = %this.size;
	%this.size = (%this.size + (%len = strlen(%str))) | 0;

	for (%j = 0; %j < %len; %j = (%j + 1) | 0)
		%this.set((%i + %j) | 0, ord(getSubStr(%str, %j, 1)));
	
	return 1;
}

function ByteArray::toBase16(%this)
{
	for (%i = %this.size - 1; %i >= 0; %i--)
	{
		%value = %this.get(%i);

		%str =
			getSubStr("0123456789abcdef", %value >> 4, 1) @
			getSubStr("0123456789abcdef", %value  & 3, 1) @
			%str;
	}

	return %str;
}

// ByteArray::toBase64 should be implemented too

function ByteArray::toBase256(%this, %mode)
{
	if (%mode $= "")
		%mode = "trim";
	else if (%mode !$= "trim" && %mode !$= "skip" && %mode !$= "replace" && %mode !$= "fail")
	{
		console.error("Unknown NULL handling mode '" @ %mode @ "'.");
		return "";
	}

	for (%i = %this.size - 1; %i >= 0; %i--)
	{
		%value = %this.get(%i);

		if (%value == 0)
		{
			switch$ (%mode)
			{
				case "trim": %str = "";
				case "skip": continue;
				case "replace": %str = "<NULL>" @ %str;
				case "fail": return "";
			}
		}
		else
			%str = chr(%value) @ %str;
	}

	return %str;
}

function ByteArray::get(%this, %i)
{
	if (%i < 0 || %i >= %this.size)
		return "";

	return Parent::get(%this, %i);
}

function ByteArray::set(%this, %i, %v)
{
	if (%i < 0 || %i >= %this.size)
		return "";

	return Parent::set(%this, %i, %v);
}

function BitMap()
{
	return tempref(new ScriptObject()
	{
		class = "BitMap";
	} @ "\x08");
}

function BitMap::get(%this, %i)
{
	return %this.int[%i >> 5] >> %i & 1;
}

function BitMap::set(%this, %i, %v)
{
	if (%this.int[%i >> 5] != 0)
		%this.int[%i >> 5] &= ~(1 << %i);

	%this.int[%i >> 5] |= (%v & 1) << %i;
	return %v;
}

function BitArray(%init)
{
	%array = tempref(new ScriptObject()
	{
		class = "BitArray";
		superClass = "BitMap";
		size = 0;
	} @ "\x08");

	if (%init !$= "")
	{
		if (%init.class $= "ByteArray")
		{
			for (%i = 0; %i < %init.size; %i = (%i + 1) | 0)
				%array.appendByte(%init.get(%i));
		}
		else
			%array.concat(%init);
	}

	return %array;
}

function BitArray::fromString(%str)
{
	%len = strlen(%str);

	%array = tempref(new ScriptObject()
	{
		class = "BitArray";
		superClass = "BitMap";
		size = 0;
	} @ "\x08");

	// this could be improved by a lot
	for (%i = 0; %i < %len; %i = (%i + 1) | 0)
		%array.appendByte(ord(getSubStr(%str, %i, 1)));

	return %array;
}

function BitArray::__len__(%this)
{
	return %this.size;
}

function BitArray::__iter__(%this)
{
	%iter = ArrayIterator(%this.size);

	for (%i = 0; %i < %this.size; %i = (%i + 1) | 0)
		%iter.value[%i] = %this.int[%i >> 5] >> %i & 1;

	return %iter;
}

function BitArray::__repr__(%this)
{
	for (%i = 0; %i < %this.size; %i = (%i + 1) | 0)
	{
		if (%i && (%i & 7) == 0)
			%str = %str @ " ";

		%str = %str @ %this.get(%i);
	}
	return "BitArray[" @ %str @ "]";
}

function BitArray::clear(%this)
{
	if (%this.size > 0)
	{
		%ints = (((%this.size - 1) >> 5) + 1) | 0;
		for (%i = 0; %i < %ints; %i = (%i + 1) | 0)
			%this.int[%i] = "";
	}

	%this.size = 0;
	return %this;
}

function BitArray::append(%this, %value)
{
	%this.size = ((%i = %this.size) + 1) | 0;
	%this.set(%i, %value);

	return 1;
}

function BitArray::appendByte(%this, %value)
{
	%value &= 0xFF;
	%this.size = ((%i = %this.size) + 8) | 0;

	for (%j = 0; %j < 8; %j++)
	{
		%idx = (%i + (7 - %j)) | 0;
		%bit = (%value >> %j) & 1;

		%this.set(%idx, %bit);
	}
	
	return 1;
}

function BitArray::appendInt(%this, %value)
{
	%this.size = ((%i = %this.size) + 32) | 0;

	for (%j = 0; %j < 32; %j++)
	{
		%idx = (%i + (31 - %j)) | 0;
		%bit = (%value >> %j) & 1;

		%this.set(%idx, %bit);
	}
	
	return 1;
}

function BitArray::concat(%this, %seq)
{
	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return 0;

	while (%iter.hasNext())
		%this.append(%iter.next());

	%iter.delete();
	return 1;
}

function BitArray::toBase16(%this)
{
	for (%i = %this.size - 1; %i >= 0; %i -= 4)
	{
		%value = 0;

		for (%j = 0; %j < 4; %j++)
			%value |= %this.get((%i - %j) | 0) << %j;
		
		%str = getSubStr("0123456789abcdef", %value, 1) @ %str;
	}

	return %str;
}

function BitArray::toBase64(%this, %extra)
{
	%map = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
	%map = %map @ (strlen(%extra) >= 2 ? %extra : "+/");

	for (%i = 0; %i < %this.size; %i = (%i + 4) | 0)
	{
		%value = 0;

		for (%j = 0; %j < 4; %j++)
			%value |= %this.get((%i + (3 - %j)) | 0) << %j;

		%str = %str @ getSubStr(%map, %value, 1);
	}

	%missing = ((4 - strlen(%str)) % 4) | 0;

	for (%i = 0; %i < %missing; %i++)
		%str = %str @ "=";

	return %str;
}

function BitArray::toBase256(%this, %mode)
{
	if (%mode $= "")
		%mode = "trim";
	else if (%mode !$= "trim" && %mode !$= "skip" && %mode !$= "replace" && %mode !$= "fail")
	{
		console.error("Unknown NULL handling mode '" @ %mode @ "'.");
		return "";
	}

	for (%i = %this.size - 1; %i >= 0; %i -= 8)
	{
		%value = 0;

		for (%j = 0; %j < 8; %j++)
			%value |= %this.get((%i - %j) | 0) << %j;
		
		if (%value == 0)
		{
			switch$ (%mode)
			{
				case "trim": %str = "";
				case "skip": continue;
				case "replace": %str = "<NULL>" @ %str;
				case "fail": return "";
			}
		}
		else
			%str = chr(%value) @ %str;
	}

	return %str;
}

function BitArray::get(%this, %i)
{
	if (%i < 0 || %i >= %this.size)
		return "";

	return Parent::get(%this, %i);
}

function BitArray::set(%this, %i, %v)
{
	if (%i < 0 || %i >= %this.size)
		return "";

	return Parent::set(%this, %i, %v);
}