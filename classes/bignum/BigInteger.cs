class (BigInteger, Number);
	BigInteger.definePrivateAttribute("ptr");

	function BigInteger::__construct__(%this, %i)
	{
		%this.ptr = makenum(%i);
	}

	function BigInteger::__destruct__(%this)
	{
		deletenum(%this.ptr);
	}

	BigInteger.defineMethod("fromDecimal", "BigInteger", "int value",
		"Create a BigInteger from a decimal value.");
	function BigInteger::fromDecimal(%this, %value)
	{
		if (%value $= "")
			return 0;

		while (%value > 0)
		{
			%hex = getSubStr("0123456789abcdef", %value & 15, 1) @ %hex;
			%value >>= 4;
		}

		return BigInteger(%hex);
	}

	function BigInteger::__str__(%this)
	{
		return "0x" @ printnum(%this.ptr);
	}

	function BigInteger::__add__(%this, %other)
	{
		if (assert(isInstanceOf(%other, BigInteger), "BigInteger math requires BigInteger on right-hand side"))
			return 0;

		%out = BigInteger(printnum(%this.ptr));
		BigInt_Add(%out.ptr, %other.ptr);
		return %out;
	}

	function BigInteger::__sub__(%this, %other)
	{
		if (assert(isInstanceOf(%other, BigInteger), "BigInteger math requires BigInteger on right-hand side"))
			return 0;

		%out = BigInteger(printnum(%this.ptr));
		BigInt_Subtract(%out.ptr, %other.ptr);
		return %out;
	}

	function BigInteger::__mul__(%this, %other)
	{
		if (assert(isInstanceOf(%other, BigInteger), "BigInteger math requires BigInteger on right-hand side"))
			return 0;

		%out = BigInteger(printnum(%this.ptr));
		BigInt_Multiply(%out.ptr, %other.ptr);
		return %out;
	}

	function BigInteger::__div__(%this, %other)
	{
		if (assert(isInstanceOf(%other, BigInteger), "BigInteger math requires BigInteger on right-hand side"))
			return 0;

		%out = BigInteger(printnum(%this.ptr));
		BigInt_Divide(%out.ptr, %other.ptr);
		return %out;
	}

	BigInteger.defineMethod("add", "this", "BigInteger other",
		"Add the value of *other* to this BigInteger.");
	function BigInteger::add(%this, %other)
	{
		if (!assert(isInstanceOf(%other, BigInteger), "other must be a BigInteger instance"))
			BigInt_Add(%this.ptr, %other.ptr);

		return %this;
	}

	BigInteger.defineMethod("subtract", "this", "BigInteger other",
		"Subtract the value of *other* from this BigInteger.");
	function BigInteger::subtract(%this, %other)
	{
		if (!assert(isInstanceOf(%other, BigInteger), "other must be a BigInteger instance"))
			BigInt_Subtract(%this.ptr, %other.ptr);

		return %this;
	}

	BigInteger.defineMethod("multiply", "this", "BigInteger other",
		"Multiply this BigInteger by the the value of *other*.");
	function BigInteger::multiply(%this, %other)
	{
		if (!assert(isInstanceOf(%other, BigInteger), "other must be a BigInteger instance"))
			BigInt_Multiply(%this.ptr, %other.ptr);

		return %this;
	}

	BigInteger.defineMethod("divide", "this", "BigInteger other",
		"Divide this BigInteger by the value of *other*.");
	function BigInteger::divide(%this, %other)
	{
		if (!assert(isInstanceOf(%other, BigInteger), "other must be a BigInteger instance"))
			BigInt_Subtract(%this.ptr, %other.ptr);

		return %this;
	}