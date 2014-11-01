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