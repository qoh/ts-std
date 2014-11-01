function rotl(%value, %shift)
{
	return (%value << %shift) | (%value >> (32 - %value));
}

function sha1_(%bytes) // sha1() is, of course, used
{
	if (%bytes.class $= "BitArray")
		%bytes = ByteArray(%bytes);
	else if (%bytes.class !$= "ByteArray")
		%bytes = ByteArray::fromString(%bytes);

	%sha1 = createSHA1();
	%sha1.begin();
	%sha1.update(%bytes);

	%digest = %sha1.hexdigest();
	%sha1.delete();

	return %digest;
}

function createSHA1()
{
	return tempref(new ScriptObject()
	{
		class = "SHA1";
	} @ "\x08");
}

function SHA1::begin(%this)
{
	%this.h0 = 0x67452301;
	%this.h1 = 0xEFCDAB89;
	%this.h2 = 0x98BADCFE;
	%this.h3 = 0x10325476;
	%this.h4 = 0xC3D2E1F0;

	if (isObject(%this.buf))
		unref(%this.buf);

	%this.buf = ref(ByteArray());
	%this.idx = 0;
}

function SHA1::update(%this, %bytes)
{
	if (%bytes !$= "")
	{
		if (%bytes.class $= "BitArray")
			%bytes = ByteArray(%bytes);

		// this is bad
		%this.buf.concat(%chunk);
	}

	// really bad.
	while (%this.buf.size - %this.idx * 64 >= 64)
	{
		for (%i = 0; %i < 16; %i++)
			%w[%i] = %this.buf.int[%this.idx * 16 + %i];

		for (%i = 16; %i < 80; %i++)
			%w[%i] = rotl((%w[%i-3] ^ %w[%i-8] ^ %w[%i-14] ^ %w[%i-16]), 1);

		%a = %this.h0;
		%b = %this.h1;
		%c = %this.h2;
		%d = %this.h3;
		%e = %this.h4;

		for (%i = 0; %i < 80; %i++)
		{
			if (%i < 20)
			{
				%f = (%b & %c) | ((~%b) & %d);
				//%f = %d ^ ( %b & (%c ^ %d));
				%k = 0x5A827999;
			}
			else if (%i < 40)
			{
				%f = %b ^ %c ^ %d;
				%k = 0x6ED9EBA1;
			}
			else if (%i < 60)
			{
				%f = (%b & %c) | (%b & %d) | (%c & %d);
				%k = 0x8F1BBCDC;
			}
			else
			{
				%f = %b ^ %c ^ %d;
				%k = 0xCA62C1D6;
			}

			%temp = (rotl(%a, 5) + %f + %e + %k + %w[%i]) | 0; // issues could occur here regarding signedness?
			%e = %d;
			%d = %c;
			%c = rotl(%b, 30);
			%b = %a;
			%a = %temp;
		}

		%this.h0 = (%this.h0 + %a) | 0; // same here
		%this.h1 = (%this.h1 + %b) | 0;
		%this.h2 = (%this.h2 + %c) | 0;
		%this.h3 = (%this.h3 + %d) | 0;
		%this.h4 = (%this.h4 + %e) | 0;

		%this.idx++;
	}

	return 1;
}

function SHA1::digest(%this)
{
	%len = %this.buf.size;
	%this.buf.append(0x80);

	while ((%this.buf.size - %this.idx * 64 - 56) % 64 != 0)
		%this.buf.append(0x00);

	// need to actually append a 64-bit integer here
	%this.buf.appendInt(0);
	%this.buf.appendInt(%len);

	%this.update("");

	%bytes = ByteArray();
	%bytes.appendInt(%this.h0);
	%bytes.appendInt(%this.h1);
	%bytes.appendInt(%this.h2);
	%bytes.appendInt(%this.h3);
	%bytes.appendInt(%this.h4);
	return %bytes;
}

function SHA1::hexdigest(%this)
{
	%bytes = %this.digest();
	%digest = %bytes.toBase16();
	%bytes.delete();
	return %digest;
}