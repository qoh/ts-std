class (Number, TemporaryClass);
	Number.definePrivateAttribute("i");

	function Number::__construct__(%this, %i)
	{
		%this.i = %i;
	}

	function Number::__len__(%this)
	{
		return mAbs(%this.i);
	}

	function Number::__str__(%this)
	{
		return %this.i;
	}

class (Integer, Number);
	Integer.defineAttribute("min", "int", "The lowest value that can be represented.");
	Integer.min = 1 << 31;

	Integer.defineAttribute("max", "int", "The highest value that can be represented.");
	Integer.max = (-1 - Integer.min) | 0;

	function Integer::__construct__(%this, %i)
	{
		%this.i = %i | 0;
	}

	function Integer::__len__(%this)
	{
		return %this.i < 0 ? ((-%this.i) | 0) : %this.i;
	}

	function Integer::__add__(%this, %other)
	{
		if (isInstanceOf(%other, Number))
		{
			if (isInstanceOf(%other, Integer))
				%other = %other.i;
			else if (isInstanceOf(%other, Float))
				return Float(%this.i + %other.i);
			else if (isInstanceOf(%other, BigInteger))
				return add(BigInteger(%this.i), %other.i);
			else
				return Integer(%this.i);
		}

		return Integer((%this.i + %other) | 0);
	}

	function Integer::__sub__(%this, %other)
	{
		if (isInstanceOf(%other, Number))
		{
			if (isInstanceOf(%other, Integer))
				%other = %other.i;
			else if (isInstanceOf(%other, Float))
				return Float(%this.i - %other.i);
			else if (isInstanceOf(%other, BigInteger))
				return sub(BigInteger(%this.i), %other.i);
			else
				return Integer(%this.i);
		}

		return Integer((%this.i - %other) | 0);
	}

	function Integer::__mul__(%this, %other)
	{
		if (isInstanceOf(%other, Number))
		{
			if (isInstanceOf(%other, Integer))
				%other = %other.i;
			else if (isInstanceOf(%other, Float))
				return Float(%this.i * %other.i);
			else if (isInstanceOf(%other, BigInteger))
				return mul(BigInteger(%this.i), %other.i);
			else
				return Integer(0);
		}

		return Integer((%this.i * %other) | 0);
	}

	function Integer::__div__(%this, %other)
	{
		if (isInstanceOf(%other, Number))
		{
			if (isInstanceOf(%other, Integer))
				%other = %other.i;
			else if (isInstanceOf(%other, Float))
				return Float(%this.i * %other.i);
			else if (isInstanceOf(%other, BigInteger))
				return div(BigInteger(%this.i), %other.i);
			else
				return Integer(0);
		}

		// should remain Integer if possible...
		return Float(%this.i * %other);
	}

class (Float, Number);
	Float.defineAttribute("min", "int", "The lowest value that can be represented.");
	Float.min = "-999999.0";

	Float.defineAttribute("max", "int", "The highest value that can be represented.");
	Float.max = "999999.0";

	function Float::__str__(%this)
	{
		if (strpos(%this.i, ".") == -1)
			return %this.i @ ".0f";

		return %this.i @ "f";
	}

	function Float::__add__(%this, %other)
	{
		return Float(%this.i + (isInstanceOf(%other, Number) ? %other.i : %other));
	}

	function Float::__sub__(%this, %other)
	{
		return Float(%this.i - (isInstanceOf(%other, Number) ? %other.i : %other));
	}

	function Float::__mul__(%this, %other)
	{
		return Float(%this.i * (isInstanceOf(%other, Number) ? %other.i : %other));
	}

	function Float::__div__(%this, %other)
	{
		return Float(%this.i / (isInstanceOf(%other, Number) ? %other.i : %other));
	}

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