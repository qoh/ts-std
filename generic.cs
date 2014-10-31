//////
// ### Concrete object classification

function isFinite(%value)
{
	return strcmp(%value, "1.#INF") != 0 && strcmp(%value, "-1.#INF") != 0;
}

function isNaN(%value)
{
	return strcmp(%value, "-1.#IND") == 0;
}

function isFloat(%value)
{
	return strcmp(%value, %value + 0) == 0 || !isFinite(%value) || isNaN(%value);
}

function isInteger(%value)
{
	return strcmp(%value, %value | 0) == 0;
}

function isNumber(%value)
{
	return isInteger(%value) || isFloat(%value);
}

function isRealObject(%obj)
{
	return isObject(%obj) && expandEscape(getSubStr(%obj, strlen(%obj) - 1, 1)) $= "\\x08";
}

function getType(%value, %object)
{
	if (isRealObject(%value) || (%object && isObject(%value)))
	{
		if (%value.class $= "ClassInstance" && isObject(%value.__class))
			return %value.__class;

		if (%value.class !$= "")
			return %value.class;

		if (%value.superClass !$= "")
			return %value.superClass;

		%className = %value.getClassName();

		if (%value.className !$= "" && %className !$= "ScriptObject" && %className !$= "ScriptGroup")
			return %value.className;

		return %className;
	}

	if (isInteger(%value))
		return "int";

	if (isFloat(%value))
		return "float";

	return "string";
}

//////
// ### Generalized operations

function bool(%value, %object)
{
	if (%value $= "" || %value $= "0")
		return 0;

	if (%value $= "1")
		return 1;

	if (isRealObject(%value) || (%object && isObject(%value)))
	{
		if (%value.hasMethod("__bool__"))
			return SimObject::callArgs(%value, "__bool__");

		if (%value.hasMethod("__len__"))
			return SimObject::callArgs(%value, "__len__") > 0;

		return 1;
	}

	if (isNumber(%value))
		return %value != 0;

	return %value !$= "";
}

function cmp(%a, %b)
{
	if (isObject(%a))
	{
		if (%a.hasMethod("__cmp__"))
			return %a.callArgs("__cmp__", Tuple::fromArgs(%b));

		if (isObject(%b))
		{
			if (%b.hasMethod("__cmp__"))
				return %b.callArgs("__cmp__", Tuple::fromArgs(%a));

			return %a.getID() == %b.getID() ? 0 : 1;
		}

		return 1;
	}
	else if (isObject(%b))
	{
		if (%b.hasMethod("__cmp__"))
			return %b.callArgs("__cmp__", Tuple::fromArgs(%a));

		return 1;
	}

	// consider redoing this part
	if ((%a $= %a + 0 || %a $= (%a | 0)) &&
		(%b $= %b + 0 || %b $= (%b | 0)))
		return %a == %b ? 0 : (%a < %b ? -1 : 1);

	return stricmp(%a, %b);
}

function eq(%a, %b)
{
	if (isObject(%a))
	{
		if (%a.hasMethod("__eq__"))
			return %a.callArgs("__eq__", Tuple::fromArgs(%b));

		if (%a.hasMethod("__cmp__"))
			return %a.callArgs("__cmp__", Tuple::fromArgs(%b)) == 0;

		if (isObject(%b))
		{
			if (%b.hasMethod("__eq__"))
				return %b.callArgs("__eq__", Tuple::fromArgs(%a));

			if (%b.hasMethod("__cmp__"))
				return %b.callArgs("__cmp__", Tuple::fromArgs(%a)) == 0;

			return %a.getID() == %b.getID();
		}

		return 0;
	}
	else if (isObject(%b))
	{
		if (%b.hasMethod("__cmp__"))
			return %b.callArgs("__cmp__", Tuple::fromArgs(%a));

		return 0;
	}

	if (isNumber(%a) && isNumber(%b))
		return %a == %b;

	return %a $= %b;
}

function repr(%value)
{
	if (isObject(%value))
	{
		if (%value.hasMethod("__repr__"))
			return SimObject::callArgs(%value, "__repr__");

		return getType(%value, 1) SPC %value.getName() @ "(" @ %value.getID() @ ")";
	}

	if (strcmp(%value, "-1.#IND") == 0)
		return "NaN";

	if (strcmp(%value, "1.#INF") == 0)
		return "Infinity";

	if (strcmp(%value, "-1.#INF") == 0)
		return "-Infinity";

	if ((%value $= (%value | 0)) || (%value $= %value + 0))
		return %value;

	return "\"" @ expandEscape(%value) @ "\"";
}

function len(%value)
{
	if (isObject(%value))
	{
		if (%value.hasMethod("__len__"))
			return SimObject::callArgs(%value, "__len__");

		if (%value.superClass $= "Iterable" || %value.class $= "Iterable")
		{
			%len = 0;

			while (%value.hasNext())
			{
				%len = (%len + 1) | 0;
				%value.next();
			}

			return %len;
		}

		return 0;
	}

	return strlen(%value);
}

function SimObject::getAttribute(%this, %attr)
{
	switch (stripos("_abcdefghijklmnopqrstuvwxyz", getSubStr(%attr, 0, 1)))
	{
		case  0: return %this._[getSubStr(%attr, 1, strlen(%attr))];
		case  1: return %this.a[getSubStr(%attr, 1, strlen(%attr))];
		case  2: return %this.b[getSubStr(%attr, 1, strlen(%attr))];
		case  3: return %this.c[getSubStr(%attr, 1, strlen(%attr))];
		case  4: return %this.d[getSubStr(%attr, 1, strlen(%attr))];
		case  5: return %this.e[getSubStr(%attr, 1, strlen(%attr))];
		case  6: return %this.f[getSubStr(%attr, 1, strlen(%attr))];
		case  7: return %this.g[getSubStr(%attr, 1, strlen(%attr))];
		case  8: return %this.h[getSubStr(%attr, 1, strlen(%attr))];
		case  9: return %this.i[getSubStr(%attr, 1, strlen(%attr))];
		case 10: return %this.j[getSubStr(%attr, 1, strlen(%attr))];
		case 11: return %this.k[getSubStr(%attr, 1, strlen(%attr))];
		case 12: return %this.l[getSubStr(%attr, 1, strlen(%attr))];
		case 13: return %this.m[getSubStr(%attr, 1, strlen(%attr))];
		case 14: return %this.n[getSubStr(%attr, 1, strlen(%attr))];
		case 15: return %this.o[getSubStr(%attr, 1, strlen(%attr))];
		case 16: return %this.p[getSubStr(%attr, 1, strlen(%attr))];
		case 17: return %this.q[getSubStr(%attr, 1, strlen(%attr))];
		case 18: return %this.r[getSubStr(%attr, 1, strlen(%attr))];
		case 19: return %this.s[getSubStr(%attr, 1, strlen(%attr))];
		case 20: return %this.t[getSubStr(%attr, 1, strlen(%attr))];
		case 21: return %this.u[getSubStr(%attr, 1, strlen(%attr))];
		case 22: return %this.v[getSubStr(%attr, 1, strlen(%attr))];
		case 23: return %this.w[getSubStr(%attr, 1, strlen(%attr))];
		case 24: return %this.x[getSubStr(%attr, 1, strlen(%attr))];
		case 25: return %this.y[getSubStr(%attr, 1, strlen(%attr))];
		case 26: return %this.z[getSubStr(%attr, 1, strlen(%attr))];
	}

	return "";
}

function SimObject::setAttribute(%this, %attr, %value)
{
	switch (stripos("_abcdefghijklmnopqrstuvwxyz", getSubStr(%attr, 0, 1)))
	{
		case  0: %this._[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  1: %this.a[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  2: %this.b[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  3: %this.c[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  4: %this.d[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  5: %this.e[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  6: %this.f[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  7: %this.g[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  8: %this.h[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  9: %this.i[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 10: %this.j[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 11: %this.k[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 12: %this.l[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 13: %this.m[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 14: %this.n[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 15: %this.o[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 16: %this.p[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 17: %this.q[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 18: %this.r[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 19: %this.s[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 20: %this.t[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 21: %this.u[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 22: %this.v[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 23: %this.w[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 24: %this.x[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 25: %this.y[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 26: %this.z[getSubStr(%attr, 1, strlen(%attr))] = %value;
	}
}

function __general_op(%a, %b, %method, %fallback)
{
	if (isRealObject(%a) && %a.hasMethod(%method))
		return %a.callArgs(%method, Tuple::fromArgs(%b));

	if (isRealObject(%b) && %b.hasMethod(%method))
		return %b.callArgs(%method, Tuple::fromArgs(%a));

	return dynCall(%fallback, %a, %b);
}

function __add(%a, %b) { return %a + %b; }
function __sub(%a, %b) { return %a - %b; }
function __mul(%a, %b) { return %a * %b; }
function __div(%a, %b) { return %a / %b; }

function add(%a, %b) { return __general_op(%a, %b, "__add__", __add); }
function sub(%a, %b) { return __general_op(%a, %b, "__sub__", __sub); }
function mul(%a, %b) { return __general_op(%a, %b, "__mul__", __mul); }
function div(%a, %b) { return __general_op(%a, %b, "__div__", __div); }

function concat(%a, %b) { return %a @ %b; }

//////
// ### Utility

function assert(%condition, %message)
{
	if (%message !$= "" && !%condition)
	{
		console.error("Assertion failure: " @ %message);
		backTrace();
	}

	return !%condition;
}

function dec_base(%value, %chars)
{
	if (%chars $= "")
		return "";

	%len = strlen(%chars);

	while (%value > 0)
	{
		%text = getSubStr(%chars, %value % %len, 1) @ %text;
		%value = mFloor(%value / %len);
	}

	return %text;
}

function hex(%value, %upper)
{
	%chars = "0123456789abcdef";

	if (%upper)
		%chars = strUpr(%chars);

	return "0x" @ dec_base(%value, %chars);
}

function oct(%value)
{
	return "0o" @ dec_base(%value, stringlib.octdigits);
}

function bin(%value)
{
	return "0b" @ dec_base(%value, stringlib.bindigits);
}

//////
// ### Garbage collection

function ref(%obj)
{
	if (isObject(%obj) && !%obj.___constant)
	{
		if (%obj.___ref++ == 1)
			cancel(%obj.___ref_sched);
	}

	return %obj;
}

function unref(%obj)
{
	if (isObject(%obj) && !%obj.___constant && %obj.___ref-- == 0)
		%obj.___ref_sched = %obj.schedule(0, delete);

	return "";
}

function tempref(%obj)
{
	if (isObject(%obj) && !%obj.___constant && %obj.___ref $= "")
	{
		%obj.___ref = 0;
		%obj.___ref_sched = %obj.schedule(0, delete);
	}

	return %obj;
}

function passref(%obj)
{
	unref(%obj);
	return %obj;
}