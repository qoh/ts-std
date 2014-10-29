function ByteArray()
{
	return new ScriptObject()
	{
		class = "ByteArrayInstance";
	};
}

// get:
//     get the int       shift to first byte   clamp to [0, 255]

// set:
// != 0?
	// clear (i%4)th byte of the int by making mask like 0xFFFF00FF (for 2nd byte, for example)
	// first make the left side (0xFFFF) by shifting 0xFFFFFF00 into position
	// then OR that with the right side which is made by shifting 0x00FFFFFF into position
	//%this.int[%i >> 2] &= (0xFFFFFF00 << ((%i & 3) << 3)) | (0x00FFFFFF >> (3 - (%i & 3) << 3));
// add into int: clamped %v shifted to (i%4)th byte

function ByteArrayInstance::get(%this, %i)
{
	return %this.int[%i >> 2] >> ((%i & 3) << 3) & 255;
}

function ByteArrayInstance::set(%this, %i, %v)
{
	if (%this.int[%i >> 2] != 0)
		%this.int[%i >> 2] &= ~(0xFF << ((%i & 3) << 3));

	%this.int[%i >> 2] |= (%v & 255) << ((%i & 3) << 3);
	return %v;
}

function SizedByteArray(%size)
{
	if (assert(%size >= 0, "size must not be be negative"))
		return 0;

	return new ScriptObject()
	{
		class = "SizedByteArrayInstance";
		superClass = "ByteArrayInstance";

		size = %size | 0;
	};
}

function SizedByteArrayInstance::get(%this, %i)
{
	if (%i < 0 || %i >= %this.size)
		return 0;

	return Parent::get(%this, %i);
}

function SizedByteArrayInstance::set(%this, %i, %v)
{
	if (%i < 0 || %i >= %this.size)
		return %v;

	return Parent::set(%this, %i, %v);
}

function SizedByteArray::setSize(%this, %size)
{
	if (assert(%size >= 0, "size must not be negative"))
		return;

	%size |= 0;

	if (%size < %this.size)
	{
		// set superfluous ints to "", closest to dealloc we can get
		%end = %this.size >> 2;
		
		for (%i = -~(%size >> 2) | 0; %i < %end; %i = -~%i | 0)
			%this.int[%i] = "";
	}

	%this.size = %size;
}