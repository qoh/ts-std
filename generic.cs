//////
// ### Concrete object classification

function isFloat(%value)
{
	return %value + 0 $= %value;
}

function isNumber(%value)
{
	return isInteger(%value) || isFloat(%value);
}

function isInteger(%value)
{
	return %value | 0 $= %value;
}

function isType(%obj, %type)
{
	if (isRealObject(%obj))
	{
		if (%obj.__instance && %type.class $= "Class" && !%type.__instance)
		{
			if (%obj.__class == %type.getID())
				return 1;

			if (%inherit)
			{
				%target = %type.base;

				while (isObject(%target))
				{
					if (%obj.__class == %target)
						return 1;

					%target = %target.base;
				}

				return 0;
			}
		}

		if (%obj.class !$= "" && %type $= %obj.class)
			return 1;

		if ((%inherit || %obj.class $= "") && %obj.superClass !$= "" && %type $= %obj.superClass)
			return 1;

		if (%obj.className !$= "" && %type $= %obj.className)
			return 1;

		%className = %obj.getClassName();

		if (%className !$= "" && %type $= %className)
			return 1;
	}

	return eq(getType(%value), %type);
}

function isRealObject(%obj)
{
	return
		isObject(%obj) &&
		%obj !$= %obj.getName() &&
		expandEscape(getSubStr(%obj, strlen(%obj) - 2, 2)) $= "\\cp\\co";
}

function getType(%obj)
{
	if (isRealObject(%obj))
	{
		if (%obj.__instance && isObject(%obj.__class))
			return %obj.__class;

		if (%obj.class !$= "")
			return %obj.class;

		if (%obj.superClass !$= "")
			return %obj.superClass;

		if (%obj.className !$= "")
			return %obj.className;

		%className = %obj.getClassName();

		if (%className !$= "")
			return %className;
	}

	if (isInteger(%obj))
		return "int";

	if (isFloat(%obj))
		return "float";

	return "string";
}

function makeRealObject(%obj, %tempref)
{
	if (isRealObject(%obj))
		return %obj;

	if (isObject(%obj))
	{
		if (%tempref)
			tempref(%obj);

		return %obj @ "\cp\co";
	}

	return %obj;
}

function obj(%obj, %tempref)
{
	return makeRealObject(%obj, %tempref);
}

function type(%obj)
{
	return getType(%obj);
}

//////
// ### Generalized operations

// pass(any value)
// Returns *value*, for use in functional iterators.
function pass(%value)
{
	return %value;
}

function bool(%a)
{
	if (%a $= "" || %a $= "0")
		return 0;

	if (%a $= "1")
		return 1;

	if (isObject(%a))
	{
		if (%a.hasMethod("__bool__"))
			return %a.call("__bool__");

		if (%a.hasMethod("__len__"))
			return %a.call("__len__") > 0;

		return 1;
	}

	if (%a $= %a + 0 || %a $= %a | 0)
		return %a != 0;

	return 1;
}

function cmp(%a, %b)
{
	if (isObject(%a))
	{
		if (%a.hasMethod("__cmp__"))
			return %a.call("__cmp__", %b);

		if (isObject(%b))
		{
			if (%b.hasMethod("__cmp__"))
				return %b.call("__cmp__", %a);

			return %a.getID() == %b.getID() ? 0 : 1;
		}

		return 1;
	}

	if ((%a $= %a + 0 || %a $= %a | 0) &&
		(%b $= %b + 0 || %b $= %b | 0))
		return %a == %b ? 0 : (%a < %b ? -1 : 1);

	//return %a $= %b;
	return stricmp(%a, %b);
}

function eq(%a, %b)
{
	if (isObject(%a))
	{
		if (%a.hasMethod("__eq__"))
			return %a.call("__eq__", %b);

		if (%a.hasMethod("__cmp__"))
			return %a.call("__cmp__", %b) == 0;

		if (isObject(%b))
		{
			if (%b.hasMethod("__eq__"))
				return %b.call("__eq__", %a);

			if (%b.hasMethod("__cmp__"))
				return %b.call("__cmp__", %a) == 0;

			return %a.getID() == %b.getID();
		}

		return 0;
	}

	if ((%a $= %a + 0 || %a $= (%a | 0)) &&
		(%b $= %b + 0 || %b $= (%b | 0)))
		return %a == %b;

	return %a $= %b;
}

function repr(%a)
{
	if (isObject(%a))
	{
		if (%a.hasMethod("__repr__"))
			return %a.call("__repr__");

		if (%a.class $= "Class")
		{
			%class = %a.__class.getName();

			if (%class $= "")
				%class = "anonymous Class instance";
		}
		else
		{
			%class = %a.class;

			if (%class $= "")
				%class = %a.getClassName();
		}

		return %class SPC %a.getName() @ "(" @ %a.getID() @ ")";
	}

	if (strcmp(%a, %a | 0) == 0 || strcmp(%a, %a + 0) == 0)
		return %a;

	return "\"" @ expandEscape(%a) @ "\"";
}

function len(%a)
{
	if (isObject(%a))
	{
		if (%a.hasMethod("__len__"))
			return %a.call("__len__");

		if (%a.superClass $= "Iterable" || %a.class $= "Iterable")
		{
			%len = 0;

			while (%a.hasNext())
			{
				%len = (%len + 1) | 0;
				%a.next();
			}

			return %len;
		}

		return 0;
	}

	return strlen(%a);
}

function SimObject::getAttribute(%this, %attr)
{
	if (%attr $= "")
		return "";

	switch (stripos("_abcdefghijklmnopqrstuvwxyz", getSubStr(%attr, 0, 1)))
	{
		case  0: return %object._[getSubStr(%attr, 1, strlen(%attr))];
		case  1: return %object.a[getSubStr(%attr, 1, strlen(%attr))];
		case  2: return %object.b[getSubStr(%attr, 1, strlen(%attr))];
		case  3: return %object.c[getSubStr(%attr, 1, strlen(%attr))];
		case  4: return %object.d[getSubStr(%attr, 1, strlen(%attr))];
		case  5: return %object.e[getSubStr(%attr, 1, strlen(%attr))];
		case  6: return %object.f[getSubStr(%attr, 1, strlen(%attr))];
		case  7: return %object.g[getSubStr(%attr, 1, strlen(%attr))];
		case  8: return %object.h[getSubStr(%attr, 1, strlen(%attr))];
		case  9: return %object.i[getSubStr(%attr, 1, strlen(%attr))];
		case 10: return %object.j[getSubStr(%attr, 1, strlen(%attr))];
		case 11: return %object.k[getSubStr(%attr, 1, strlen(%attr))];
		case 12: return %object.l[getSubStr(%attr, 1, strlen(%attr))];
		case 13: return %object.m[getSubStr(%attr, 1, strlen(%attr))];
		case 14: return %object.n[getSubStr(%attr, 1, strlen(%attr))];
		case 15: return %object.o[getSubStr(%attr, 1, strlen(%attr))];
		case 16: return %object.p[getSubStr(%attr, 1, strlen(%attr))];
		case 17: return %object.q[getSubStr(%attr, 1, strlen(%attr))];
		case 18: return %object.r[getSubStr(%attr, 1, strlen(%attr))];
		case 19: return %object.s[getSubStr(%attr, 1, strlen(%attr))];
		case 20: return %object.t[getSubStr(%attr, 1, strlen(%attr))];
		case 21: return %object.u[getSubStr(%attr, 1, strlen(%attr))];
		case 22: return %object.v[getSubStr(%attr, 1, strlen(%attr))];
		case 23: return %object.w[getSubStr(%attr, 1, strlen(%attr))];
		case 24: return %object.x[getSubStr(%attr, 1, strlen(%attr))];
		case 25: return %object.y[getSubStr(%attr, 1, strlen(%attr))];
		case 26: return %object.z[getSubStr(%attr, 1, strlen(%attr))];
	}

	return "";
}

function SimObject::getAttributeType(%this, %attr)
{
	return %this.___attr_type[%attr];
}

function SimObject::setAttribute(%this, %attr, %value, %type)
{
	switch (stripos("_abcdefghijklmnopqrstuvwxyz", getSubStr(%attr, 0, 1)))
	{
		case  0: %object._[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  1: %object.a[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  2: %object.b[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  3: %object.c[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  4: %object.d[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  5: %object.e[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  6: %object.f[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  7: %object.g[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  8: %object.h[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case  9: %object.i[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 10: %object.j[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 11: %object.k[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 12: %object.l[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 13: %object.m[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 14: %object.n[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 15: %object.o[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 16: %object.p[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 17: %object.q[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 18: %object.r[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 19: %object.s[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 20: %object.t[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 21: %object.u[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 22: %object.v[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 23: %object.w[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 24: %object.x[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 25: %object.y[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case 26: %object.z[getSubStr(%attr, 1, strlen(%attr))] = %value;
		case -1: return;
	}

	%this.___attr_type[%attr] = %type;
}

function __general_op(%a, %b, %method, %fallback)
{
	if (isRealObject(%a) && %a.hasMethod(%method))
		return %a.call(%method, %b);

	if (isRealObject(%b) && %b.hasMethod(%method))
		return %b.call(%method, %a);

	return Callable::call(%fallback, %a, %b);
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

if ($DEBUG $= "")
	$DEBUG = "";

function debug(%message, %level)
{
	if ($level $= "")
		%level = 1;

	if (%message !$= "" && $DEBUG >= %level)
		console.debug(%message);

	return $DEBUG >= %level;
}

function assert(%condition, %message, %expr)
{
	if ($assert_debug && %expr !$= "")
	{
		$assert_true += !!%condition;
		$assert_false += !%condition;
	}

	if (%message !$= "")
	{
		if ($assert_debug && %expr !$= "")
		{
			if (%condition)
				console.log("\c9v  Success: " SPC %message @ (%expr $= "" ? "" : "  [" @ %expr @ "]"));
			else
				console.log("\c2" @ chr(215) @ "  Failed: " SPC %message @ (%expr $= "" ? "" : "  [" @ %expr @ "]"));
		}
		else if (!%condition)
		{
			console.error("Assertion failure: " @ %message);
			backTrace();
		}
	}

	return !%condition;
}

function dec_base(%value, %chars)
{
	%len = strlen(%chars);

	while (%value != 0)
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

function min(%a, %b) { return %a < %b ? %a : %b; }
function max(%a, %b) { return %a > %b ? %a : %b; }
function mid(%a, %b, %c) { return %a < %b ? (%a > %c ? %a : %c) : (%b > %c ? %b : %a); }
function lerp(%t, %a, %b) { return %a + (%b - %a) * %t; }

function SimObject::hasMethod(%this, %method)
{
	return
		isFunction(%this.class, %method) ||
		isFunction(%this.superClass, %method) ||
		isFunction(%this.getName(), %method) ||
		isFunction(%this.getClassName(), %method);
}

function SimObject::call(%this, %method,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17)
{
	if (!%this.hasMethod(%method))
	{
		echo("\c1<input> (0): Unknown command " @ %method @ ".\n  \c1" @ repr(%this));
		return "";
	}

	for (%count = 18; %count > 0; %count--)
	{
		if (%a[%count - 1] $= "")
			break;
	}

	for (%i = 0; %i < %count; %i++)
	{
		if (%i)
			%args = %args @ ",";

		%args = %args @ "%a" @ %i;
	}

	return eval("return %this." @ %method @ "(" @ %args @ ");");
}

//////
// ### Garbage collection

function ref(%obj)
{
	if (isObject(%obj))
	{
		if ($DEBUG >= 4)
			console.debug("ref " @ repr(%obj));

		%obj.___ref++;

		if (isEventPending(%obj.___ref_sched) && %obj.___ref == 1)
			cancel(%obj.___ref_sched);
	}

	return %obj;
}

function unref(%obj)
{
	if (isObject(%obj))
	{
		if ($DEBUG >= 4)
			console.debug("unref " @ repr(%obj));

		%obj.___ref--;

		if (!isEventPending(%obj.___ref_sched) && %obj.___ref <= 0)
			%obj.___ref_sched = schedule(0, 0, _garbage, %obj);

		if ($DEBUG >= 1 && %obj.___ref < 0)
		{
			%name = %obj.getClassName() SPC %obj.getName() @ "(" @ %obj.getID() @ ")";
			console.warn("Reference count for " @ %name @ " is negative; multiple unref()s");
		}
	}

	return "";
}

function passref(%obj)
{
	unref(%obj);
	return %obj;
}

function tempref(%obj)
{
	if (isObject(%obj) && %obj.___ref | 0 == 0 && !isEventPending(%obj.___ref_sched))
	{
		%obj.___ref = 0;
		%obj.___ref_sched = schedule(0, 0, _garbage, %obj);
	}

	return %obj;
}

function _garbage(%obj)
{
	if (!isObject(%obj))
		return;

	if (%obj.getGroup() == DataBlockGroup.getName())
		console.warn("_garbage: target <" @ repr(%obj) @ "> is in DataBlockGroup");
	
	if ($DEBUG >= 1 && %obj.getName() !$= "")
		console.warn("_garbage: target <" @ repr(%obj) @ "> has name" SPC %obj.getName());

	if ($DEBUG >= 3)
		console.info("_garbage: collecting <" @ repr(%obj) @ ">");

	%obj.___garbage = 1;
	%obj.delete();
}