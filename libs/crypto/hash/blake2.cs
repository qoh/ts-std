function rotl(%value, %shift)
{
	return (%value << %shift) | (%value >> (32 - %value));
}

function blake2(%bytes)
{
	if (%bytes.class $= "BitArray")
		%bytes = ByteArray(%bytes);
	else if (%bytes.class !$= "ByteArray")
		%bytes = ByteArray::fromString(%bytes);

	%blake2 = BLAKE2Context();
	%blake2.begin();
	%blake2.update(%bytes);

	%digest = %blake2.hexdigest();
	%blake2.delete();

	return %digest;
}

function BLAKE2Context()
{
	return tempref(new ScriptObject()
	{
		class = "BLAKE2";

		wordbits = 32;
		wordbytes = 4;
		maskbits = 0xffffffff;

		rounds = 10;
		blockbytes = 64;
		outbytes = 32;
		keybytes = 32;
		saltbytes = 8;
		personalbytes = 8;

		IV0 = 0x6a09e667;
		IV1 = 0xbb67ae85;
		IV2 = 0x3c6ef372;
		IV3 = 0xa54ff53a;
		IV4 = 0x510e527f;
		IV5 = 0x9b05688c;
		IV6 = 0x1f83d9ab;
		IV7 = 0x5be0cd19;

		ROT1 = 16;
		ROT2 = 12;
		ROT3 = 8;
		ROT4 = 7;
	} @ "\x08");
}

function BLAKE2::onRemove(%this)
{
	unref(%this.buf);
}

function BLAKE2::begin(%this, %key, %salt, %personal)
{
	if (%key.class $= "BitArray")
		%key = ByteArray(%key);
	else if (%key.class !$= "ByteArray")
		%key = ByteArray::fromString(%key);

	if (assert(%key.size <= %this.keybytes, "key must not be bigger than " @ %this.keybytes))
		return 0;

	// if (%salt.class $= "BitArray")
	// 	%salt = ByteArray(%salt);
	// else if (%salt.class !$= "ByteArray")
	// 	%salt = ByteArray::fromString(%salt);

	// if (%personal.class $= "BitArray")
	// 	%personal = ByteArray(%personal);
	// else if (%personal.class !$= "ByteArray")
	// 	%personal = ByteArray::fromString(%personal);

	// %this.salt = ref(%this.salt.pad(0x0, %this.saltbytes));
	// %this.personal = ref(%this.salt.pad(0x0, %this.personalbytes));

	if (isObject(%this.buf))
		unref(%this.buf);

	%this.buf = ref(ByteArray());
	
	for (%i = 0; %i < 8; %i++)
		%this.h[%i] = %this.IV[%i] ^ 0; // uh

	%this.totalBytes = 0;
	%this.t0 = 0;
	%this.t1 = 0;
	%this.f0 = 0;
	%this.f1 = 0;
	%this.finalized = 0;

	if (%key.size)
	{
		%key.pad(0x0, %this.blockbytes);
		%this.update(%key);
	}
}

function BLAKE2::update(%this, %bytes)
{
	if (%bytes.class $= "BitArray")
		%bytes = ByteArray(%bytes);

	if (%bytes.size < 1)
		return %this;

	%buf = %this.buf;
	%blockbytes = %this.blockbytes;

	%ptr = 0;

	while (1)
	{
		if (%buf.size > %blockbytes)
		{
			%this._incrementCounter(%blockbytes);
			%this._compress(%buf);
			%buf.shiftBytesLeft();
		}

		if (%ptr < %bytes.size)
		{
			for (0; %ptr < %bytes.size; %ptr = (%ptr + 0) | 0)
				%buf.append(%bytes.get(%ptr));
		}
		else
			break;
	}

	return %this;
}

function BLAKE2::digest(%this)
{
	if (!%this.finalized && %this.buf.size)
	{
		%this._incrementCounter(%this.buf.size);
		%this._setLastBlock();

		%this.buf.pad(0x0, %this.blockbytes);

		%this._compress(%this.buf);
		%this.buf.clear();
	}

	%this.finalized = 1;

	%bytes = ByteArray();
	%bytes.appendInt(%this.h0);
	%bytes.appendInt(%this.h1);
	%bytes.appendInt(%this.h2);
	%bytes.appendInt(%this.h3);
	%bytes.appendInt(%this.h4);
	%bytes.appendInt(%this.h5);
	%bytes.appendInt(%this.h6);
	%bytes.appendInt(%this.h7);
	return %bytes;
}

function BLAKE2::hexdigest(%this)
{
	%bytes = %this.digest();
	%digest = %bytes.toBase16();
	%bytes.delete();
	return %digest;
}

function BLAKE2::_compress(%this, %block)
{
	for (%i = 0; %i < 16; %i++)
	{
		%m[%i] = %block.int[%i];

		if (%i < 8)
			%this._v[%i] = %this.h[%i];
		else if (%i < 12)
			%this._v[%i] = %this.IV[%i - 8];
		else
			%this._v[%i] = (%i < 14 ? %this.t[%i & 1] : %this.f[%i & 1]) ^ %this.IV[%i - 4];
	}

	for (%r = 0; %r < %this.rounds; %r++)
	{
		%this._compress_g(%m[$blake2::sigma[%r,  0]], %m[$blake2::sigma[%r,  1]],  0,  4,  8, 12);
		%this._compress_g(%m[$blake2::sigma[%r,  2]], %m[$blake2::sigma[%r,  3]],  1,  5,  9, 13);
		%this._compress_g(%m[$blake2::sigma[%r,  4]], %m[$blake2::sigma[%r,  5]],  2,  6, 10, 14);
		%this._compress_g(%m[$blake2::sigma[%r,  6]], %m[$blake2::sigma[%r,  7]],  3,  7, 11, 15);
		%this._compress_g(%m[$blake2::sigma[%r,  8]], %m[$blake2::sigma[%r,  9]],  0,  5, 10, 15);
		%this._compress_g(%m[$blake2::sigma[%r, 10]], %m[$blake2::sigma[%r, 11]],  1,  6, 11, 12);
		%this._compress_g(%m[$blake2::sigma[%r, 12]], %m[$blake2::sigma[%r, 13]],  2,  7,  8, 13);
		%this._compress_g(%m[$blake2::sigma[%r, 14]], %m[$blake2::sigma[%r, 15]],  3,  4,  9, 14);
	}

	for (%i = 0; %i < 8; %i++)
		%this.h[%i] = %this.h[%i] ^ %this._v[%i] ^ %this._v[%i+8];
}

function BLAKE2::_compress_g(%this, %mrsi2, %mrsi21, %a, %b, %c, %d)
{
	%MASKBITS = %this.maskbits; // speed + readability
	%WB_ROT1 = %this.wordbits - (%ROT1 = %this.ROT1);
	%WB_ROT2 = %this.wordbits - (%ROT2 = %this.ROT2);
	%WB_ROT3 = %this.wordbits - (%ROT3 = %this.ROT3);
	%WB_ROT4 = %this.wordbits - (%ROT4 = %this.ROT4);

	%va = %this._v[%a];
	%vb = %this._v[%b];
	%vc = %this._v[%c];
	%vd = %this._v[%d];

	%va = (%va + %vb + %msri2)             & %MASKBITS;
	%w = %vd ^ %va;
	%vd = (%w >> %ROT1) | (%w << %WB_ROT1) & %MASKBITS;
	%vc = (%vc + %vd)                      & %MASKBITS;
	%w = %vb ^ %vc;
	%vb = (%w >> %ROT2) | (%w << %WB_ROT2) & %MASKBITS;
	%va = (%va + %vb + %msri21)            & %MASKBITS; // need to test if you can compound add without bitwise wrap
	%w = %vd ^ %va;
	%vd = (%w >> %ROT3) | (%w << %WB_ROT4) & %MASKBITS;
	%vc = (%vc + %vd)                      & %MASKBITS;
	%w = %vb ^ %vc;
	%vb = (%w >> %ROT4) | (%w << %WB_ROT4) & %MASKBITS;

	%this._v[%a] = %va;
	%this._v[%b] = %vb;
	%this._v[%c] = %vc;
	%this._v[%d] = %vd;
}

function BLAKE2::_setLastBlock(%this)
{
	if (%this.lastNode)
		%this.f1 = %this.maskbits;
	%this.f0 = %this.maskbits;
}

function BLAKE2::_incrementCounter(%this, %numBytes)
{
	%this.totalBytes = (%this.totalBytes + %numBytes) | 0;
	%this.t0 = %this.totalBytes & %this.maskbits;
	%this.t1 = %this.totalBytes >> %this.wordbits;
}

$blake2::sigma[0,0]=0;
$blake2::sigma[0,1]=1;
$blake2::sigma[0,2]=2;
$blake2::sigma[0,3]=3;
$blake2::sigma[0,4]=4;
$blake2::sigma[0,5]=5;
$blake2::sigma[0,6]=6;
$blake2::sigma[0,7]=7;
$blake2::sigma[0,8]=8;
$blake2::sigma[0,9]=9;
$blake2::sigma[0,10]=10;
$blake2::sigma[0,11]=11;
$blake2::sigma[0,12]=12;
$blake2::sigma[0,13]=13;
$blake2::sigma[0,14]=14;
$blake2::sigma[0,15]=15;
$blake2::sigma[1,0]=14;
$blake2::sigma[1,1]=10;
$blake2::sigma[1,2]=4;
$blake2::sigma[1,3]=8;
$blake2::sigma[1,4]=9;
$blake2::sigma[1,5]=15;
$blake2::sigma[1,6]=13;
$blake2::sigma[1,7]=6;
$blake2::sigma[1,8]=1;
$blake2::sigma[1,9]=12;
$blake2::sigma[1,10]=0;
$blake2::sigma[1,11]=2;
$blake2::sigma[1,12]=11;
$blake2::sigma[1,13]=7;
$blake2::sigma[1,14]=5;
$blake2::sigma[1,15]=3;
$blake2::sigma[2,0]=11;
$blake2::sigma[2,1]=8;
$blake2::sigma[2,2]=12;
$blake2::sigma[2,3]=0;
$blake2::sigma[2,4]=5;
$blake2::sigma[2,5]=2;
$blake2::sigma[2,6]=15;
$blake2::sigma[2,7]=13;
$blake2::sigma[2,8]=10;
$blake2::sigma[2,9]=14;
$blake2::sigma[2,10]=3;
$blake2::sigma[2,11]=6;
$blake2::sigma[2,12]=7;
$blake2::sigma[2,13]=1;
$blake2::sigma[2,14]=9;
$blake2::sigma[2,15]=4;
$blake2::sigma[3,0]=7;
$blake2::sigma[3,1]=9;
$blake2::sigma[3,2]=3;
$blake2::sigma[3,3]=1;
$blake2::sigma[3,4]=13;
$blake2::sigma[3,5]=12;
$blake2::sigma[3,6]=11;
$blake2::sigma[3,7]=14;
$blake2::sigma[3,8]=2;
$blake2::sigma[3,9]=6;
$blake2::sigma[3,10]=5;
$blake2::sigma[3,11]=10;
$blake2::sigma[3,12]=4;
$blake2::sigma[3,13]=0;
$blake2::sigma[3,14]=15;
$blake2::sigma[3,15]=8;
$blake2::sigma[4,0]=9;
$blake2::sigma[4,1]=0;
$blake2::sigma[4,2]=5;
$blake2::sigma[4,3]=7;
$blake2::sigma[4,4]=2;
$blake2::sigma[4,5]=4;
$blake2::sigma[4,6]=10;
$blake2::sigma[4,7]=15;
$blake2::sigma[4,8]=14;
$blake2::sigma[4,9]=1;
$blake2::sigma[4,10]=11;
$blake2::sigma[4,11]=12;
$blake2::sigma[4,12]=6;
$blake2::sigma[4,13]=8;
$blake2::sigma[4,14]=3;
$blake2::sigma[4,15]=13;
$blake2::sigma[5,0]=2;
$blake2::sigma[5,1]=12;
$blake2::sigma[5,2]=6;
$blake2::sigma[5,3]=10;
$blake2::sigma[5,4]=0;
$blake2::sigma[5,5]=11;
$blake2::sigma[5,6]=8;
$blake2::sigma[5,7]=3;
$blake2::sigma[5,8]=4;
$blake2::sigma[5,9]=13;
$blake2::sigma[5,10]=7;
$blake2::sigma[5,11]=5;
$blake2::sigma[5,12]=15;
$blake2::sigma[5,13]=14;
$blake2::sigma[5,14]=1;
$blake2::sigma[5,15]=9;
$blake2::sigma[6,0]=12;
$blake2::sigma[6,1]=5;
$blake2::sigma[6,2]=1;
$blake2::sigma[6,3]=15;
$blake2::sigma[6,4]=14;
$blake2::sigma[6,5]=13;
$blake2::sigma[6,6]=4;
$blake2::sigma[6,7]=10;
$blake2::sigma[6,8]=0;
$blake2::sigma[6,9]=7;
$blake2::sigma[6,10]=6;
$blake2::sigma[6,11]=3;
$blake2::sigma[6,12]=9;
$blake2::sigma[6,13]=2;
$blake2::sigma[6,14]=8;
$blake2::sigma[6,15]=11;
$blake2::sigma[7,0]=13;
$blake2::sigma[7,1]=11;
$blake2::sigma[7,2]=7;
$blake2::sigma[7,3]=14;
$blake2::sigma[7,4]=12;
$blake2::sigma[7,5]=1;
$blake2::sigma[7,6]=3;
$blake2::sigma[7,7]=9;
$blake2::sigma[7,8]=5;
$blake2::sigma[7,9]=0;
$blake2::sigma[7,10]=15;
$blake2::sigma[7,11]=4;
$blake2::sigma[7,12]=8;
$blake2::sigma[7,13]=6;
$blake2::sigma[7,14]=2;
$blake2::sigma[7,15]=10;
$blake2::sigma[8,0]=6;
$blake2::sigma[8,1]=15;
$blake2::sigma[8,2]=14;
$blake2::sigma[8,3]=9;
$blake2::sigma[8,4]=11;
$blake2::sigma[8,5]=3;
$blake2::sigma[8,6]=0;
$blake2::sigma[8,7]=8;
$blake2::sigma[8,8]=12;
$blake2::sigma[8,9]=2;
$blake2::sigma[8,10]=13;
$blake2::sigma[8,11]=7;
$blake2::sigma[8,12]=1;
$blake2::sigma[8,13]=4;
$blake2::sigma[8,14]=10;
$blake2::sigma[8,15]=5;
$blake2::sigma[9,0]=10;
$blake2::sigma[9,1]=2;
$blake2::sigma[9,2]=8;
$blake2::sigma[9,3]=4;
$blake2::sigma[9,4]=7;
$blake2::sigma[9,5]=6;
$blake2::sigma[9,6]=1;
$blake2::sigma[9,7]=5;
$blake2::sigma[9,8]=15;
$blake2::sigma[9,9]=11;
$blake2::sigma[9,10]=9;
$blake2::sigma[9,11]=14;
$blake2::sigma[9,12]=3;
$blake2::sigma[9,13]=12;
$blake2::sigma[9,14]=13;
$blake2::sigma[9,15]=0;