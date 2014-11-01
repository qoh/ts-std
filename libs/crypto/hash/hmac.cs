function HMAC(%bits, %key, %hash)
{
	%hmac = createHMAC();
	%hmac.begin(%key, %hash);
	%hmac.update(%bits);
	%digest = %hmac.hexdigest();
	%hmac.delete();
	return %digest;
}

function createHMAC()
{
	return tempref(new ScriptObject()
	{
		class = "HMAC";
	} @ "\x08");
}

function HMAC::onRemove(%this)
{
	unref(%this.hash);
	unref(%this.outer);
}

// probably doesn't work
// well, it works-ish, but it doesn't seem to care about the key
// at all?
function HMAC::begin(%this, %key, %hash)
{
	if (%hash $= "")
	{
		if (!isObject(%this.hash))
			%this.hash = ref(createSHA1());
	}
	else
	{
		if (assert(isObject(%hash), "hasher does not exist!"))
			return 0;

		%this.hash = ref(%hash);
	}

	if (%key !$= "")
	{
		%key = BitArray(ByteArray::fromString(%key));

		if (%key.size >= 512)
		{
			%this.hash.begin();
			%this.hash.update(%key);
			%key = %this.hash.digest();
		}

		%inner = ByteArray();
		%outer = ByteArray();

		// this is really, really bad and probably doesn't even work.
		%key.class = "ByteArray";
		%key.size >>= 3;

		for (%i = 0; %i < %key.size; %i++)
		{
			%tmp = %key.get(%i);
			%inner.append(0x36 ^ %tmp);
			%outer.append(0x5C ^ %tmp);
		}

		if (%key.size < 64)
		{
			%tmp = 64 - %key.size;
			for (%i = 0; %i < %tmp; %i++)
			{
				%inner.append(0x36);
				%outer.append(0x5C);
			}
		}

		%this.outer = ref(BitArray(%outer));
	}

	%this.hash.begin();
	%this.hash.update(BitArray(%inner));

	return 1;
}

function HMAC::update(%this, %bits)
{
	return %this.hash.update(%bits);
}

function HMAC::digest(%this)
{
	%inner = %this.hash.digest();
	%this.hash.begin();
	%this.hash.update(%this.outer);
	%this.hash.update(%inner);
	return %this.hash.digest();
}

function HMAC::hexdigest(%this)
{
	return %this.digest().toBase16();
}