// ====================================
// Non-Callable-type general purpose interfaces to object methods.


function SimObject::hasMethod(%this, %name)
{
	return
		isFunction(%this.class, %name) ||
		isFunction(%this.superClass, %name) ||
		isFunction(%this.getName(), %name) ||
		isFunction(%this.getClassName(), %name) ||
		isFunction("SimObject", %name);
}

function SimObject::call(%this, %name,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17)
{
	return %this.callArgs(%name, Tuple::fromArgs(
		%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
		%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17
	));
}

function SimObject::callArgs(%this, %name, %args)
{
	// consider adding a way to speed up calls for non-engine functions
	// by calling with prewritten %a0,%a1,%a2,... list

	if (%args !$= "")
	{
		// let's trust the outside world for once and hope for the best?
		// should remove in favor of sequence interface
		if (%args.length !$= "")
		{
			for (%i = 0; %i < %args.length && %i < 20; %i++)
			{
				if (%i)
					%list = %list @ ",";

				// is new var faster than 4+ string table lookups, etc?
				// %a[%i] = %iter.next();
				// %list = %list @ "%a" @ %i;
				%list = %list @ "%args.value" @ %i;
			}
		}
		else // do it the slow way
		{
			if (assert(%iter = iter(%args), "args is not iterable"))
				return "";

			for (%i = 0; %iter.hasNext() && %i < 20; %i++)
			{
				if (%i)
					%list = %list @ ",";

				%a[%i] = %iter.next();
				%list = %list @ "%a" @ %i;
			}

			%iter.delete();
		}
	}

	// finding a way to call a named method on an object reference without eval is a top priority
	// even being able to call a namespaced method would be better than this
	// call("foo::bar") does not work
	// in the engine, there's a Con::execute overload that takes an object to execute on
	// of course, call() doesn't use that.
	return eval("return %this." @ %name @ "(" @ %list @ ");");
}


// ====================================
// Generic interfaces to Callables that also support named functions.


function isCallable(%func)
{
	return %func.superClass $= "Callable" ? %func.isValid() : isFunction(%func);
}

function dynCall(%func,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	if (%func.superClass $= "Callable")
		return %func.dynCall(
			%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
			%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18);

	if (!isFunction(%func))
		return "";

	// long, fast and worth it.
	// avoid eval at all costs.
	for (%count = 19; %count > 0; %count--)
	{
		if (%a[%count - 1] !$= "")
			break;
	}

	switch (%count)
	{
		case  0: return call(%func);
		case  1: return call(%func,%a0);
		case  2: return call(%func,%a0,%a1);
		case  3: return call(%func,%a0,%a1,%a2);
		case  4: return call(%func,%a0,%a1,%a2,%a3);
		case  5: return call(%func,%a0,%a1,%a2,%a3,%a4);
		case  6: return call(%func,%a0,%a1,%a2,%a3,%a4,%a5);
		case  7: return call(%func,%a0,%a1,%a2,%a3,%a4,%a5,%a6);
		case  8: return call(%func,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7);
		case  9: return call(%func,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8);
		case 10: return call(%func,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9);
		case 11: return call(%func,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10);
		case 12: return call(%func,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11);
		case 13: return call(%func,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12);
		case 14: return call(%func,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13);
		case 15: return call(%func,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14);
		case 16: return call(%func,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15);
		case 17: return call(%func,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15,%a16);
		case 18: return call(%func,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15,%a16,%a17);
		case 19: return call(%func,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15,%a16,%a17,%a18);
	}
}

function dynCallArgs(%func, %args)
{
	if (%func.superClass $= "Callable")
		return %func.dynCallArgs(%args);

	if (!isFunction(%func))
		return "";

	if (%args $= "")
		return call(%func);

	// i apologize.
	switch (%args.length > 20 ? 20 : (%args.length < 0 ? 0 : %args.length))
	{
		case  0: return call(%func);
		case  1: return call(%func,%args.value0);
		case  2: return call(%func,%args.value0,%args.value1);
		case  3: return call(%func,%args.value0,%args.value1,%args.value2);
		case  4: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3);
		case  5: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4);
		case  6: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4,%args.value5);
		case  7: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4,%args.value5,%args.value6);
		case  8: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4,%args.value5,%args.value6,%args.value7);
		case  9: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4,%args.value5,%args.value6,%args.value7,%args.value8);
		case 10: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4,%args.value5,%args.value6,%args.value7,%args.value8,%args.value9);
		case 11: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4,%args.value5,%args.value6,%args.value7,%args.value8,%args.value9,%args.value10);
		case 12: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4,%args.value5,%args.value6,%args.value7,%args.value8,%args.value9,%args.value10,%args.value11);
		case 13: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4,%args.value5,%args.value6,%args.value7,%args.value8,%args.value9,%args.value10,%args.value11,%args.value12);
		case 14: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4,%args.value5,%args.value6,%args.value7,%args.value8,%args.value9,%args.value10,%args.value11,%args.value12,%args.value13);
		case 15: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4,%args.value5,%args.value6,%args.value7,%args.value8,%args.value9,%args.value10,%args.value11,%args.value12,%args.value13,%args.value14);
		case 16: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4,%args.value5,%args.value6,%args.value7,%args.value8,%args.value9,%args.value10,%args.value11,%args.value12,%args.value13,%args.value14,%args.value15);
		case 17: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4,%args.value5,%args.value6,%args.value7,%args.value8,%args.value9,%args.value10,%args.value11,%args.value12,%args.value13,%args.value14,%args.value15,%args.value16);
		case 18: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4,%args.value5,%args.value6,%args.value7,%args.value8,%args.value9,%args.value10,%args.value11,%args.value12,%args.value13,%args.value14,%args.value15,%args.value16,%args.value17);
		case 19: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4,%args.value5,%args.value6,%args.value7,%args.value8,%args.value9,%args.value10,%args.value11,%args.value12,%args.value13,%args.value14,%args.value15,%args.value16,%args.value17,%args.value18);
		case 20: return call(%func,%args.value0,%args.value1,%args.value2,%args.value3,%args.value4,%args.value5,%args.value6,%args.value7,%args.value8,%args.value9,%args.value10,%args.value11,%args.value12,%args.value13,%args.value14,%args.value15,%args.value16,%args.value17,%args.value18,%args.value19);
	}
}


// ====================================
// constructors for Callable subclasses
// TODO: rename `callableObj`.


function callable(%name)
{
	%pos = strpos(%name, "::");

	if (%pos == -1)
	{
		return tempref(new ScriptObject()
		{
			class = "PlainCallable";
			superClass = "Callable";
			args = ref(Array("", 1));
			args_after = ref(Array("", 1));
			name = %name;
		} @ "\x08");
	}
	
	return tempref(new ScriptObject()
	{
		class = "ScopeCallable";
		superClass = "Callable";
		args = ref(Array("", 1));
		args_after = ref(Array("", 1));
		scope = getSubStr(%name, 0, %pos);
		name = getSubStr(%name, %pos + 2, strlen(%name));
	} @ "\x08");
}

function callableObj(%name)
{
	return tempref(new ScriptObject()
	{
		class = "ObjectCallable";
		superClass = "Callable";
		args = ref(Array("", 1));
		args_after = ref(Array("", 1));
		name = %name;
	} @ "\x08");
}

function lambda(%code)
{
	return tempref(new ScriptObject()
	{
		class = "CodeCallable";
		superClass = "Callable";
		args = ref(Array("", 1));
		args_after = ref(Array("", 1));
		code = %code;
	} @ "\x08");
}

function attribute(%name)
{
	%tuple = tempref(new ScriptObject()
	{
		class = "TupleInstance";
		length = 1;
		value0 = %name;
	} @ "\x08");

	return callableObj("getAttribute").applyArgsAfter(%tuple);
}


// ====================================
// implementation of base Callable class
// should not be instanced normally


function Callable::onRemove(%this)
{
	unref(%this.args);
	unref(%this.args_after);
}

function Callable::__repr__(%this)
{
	return "Callable<...(" @ %this.__repr_args__() @ ")>";
}

function Callable::__repr_args__(%this)
{
	if (%this.args.length)
		%text = %text @ repr(%this.args) @ ", ";

	%text = %text @ "...";

	if (%this.args_after.length)
		%text = %text @ ", " @ repr(%this.args_after);

	return %text;
}

function Callable::_getArgs(%this, %args_mid, %skip)
{
	%args = new ScriptObject()
	{
		length = (%this.args.length + %args_mid.length + %this.args_after.length) - %skip;
	};

	// this is like the worst way of doing this
	for (%i = 0; %i < %this.args.length; %i++)
	{
		if (%i >= %skip)
			%args.value[%i - 1] = %this.args.value[%i];
	}

	for (%j = 0; %j < %args_mid.length; %i++ && %j++)
	{
		if (%i >= %skip)
			%args.value[%i] = %args_mid.value[%j];
	}

	for (%j = 0; %j < %this.args_after.length; %i++ && %j++)
	{
		if (%i >= %skip)
			%args.value[%i] = %this.args_after.value[%j];
	}

	return %args;
}

function Callable::isValid(%this)
{
	return 0;
}

function Callable::apply(%this,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	return %this.applyArgs(Tuple::fromArgs(
		%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
		%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18
	));
}

function Callable::applyArgs(%this, %args)
{
	%callable = %this.copy();
	%callable.args.concat(%args);
	return %callable;
}

function Callable::applyAfter(%this,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	return %this.applyArgsAfter(Tuple::fromArgs(
		%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
		%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18
	));
}

function Callable::applyArgsAfter(%this, %args)
{
	%callable = %this.copy();
	%callable.args_after.concat(%args);
	return %callable;
}

function Callable::dynCall(%this,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	return %this.dynCallArgs(Tuple::fromArgs(
		%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
		%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18
	));
}

function Callable::dynCallArgs()
{
	return "";
}


// ====================================
// PlainCallable: a simple global-namespace function
// `function foo() ...`


function PlainCallable::__repr__(%this)
{
	return "Callable<" @ %this.name @ "(" @ %this.__repr_args__() @ ")>";
}

function PlainCallable::copy(%this)
{
	return tempref(new ScriptObject()
	{
		class = PlainCallable;
		superClass = Callable;
		args = ref(%this.args.copy());
		args_after = ref(%this.args_after.copy());
		name = %this.name;
	} @ "\x08");
}

function PlainCallable::isValid(%this)
{
	return isFunction(%this.name);
}

function PlainCallable::dynCallArgs(%this, %args)
{
	%args = %this._getArgs(%args);
	%result = dynCallArgs(%this.name, %args);
	%args.delete();
	return %result;
}


// ====================================
// ScopeCallable: a namespaced function
// `function foo::bar() ...`


function ScopeCallable::__repr__(%this)
{
	return "Callable<" @ %this.scope @ "::" @ %this.name @ "(" @ %this.__repr_args__() @ ")>";
}

function ScopeCallable::copy(%this)
{
	return tempref(new ScriptObject()
	{
		class = ScopeCallable;
		superClass = Callable;
		args = ref(%this.args.copy());
		args_after = ref(%this.args_after.copy());
		scope = %this.scope;
		name = %this.name;
	} @ "\x08");
}

function ScopeCallable::isValid(%this)
{
	return isFunction(%this.scope, %this.name);
}

function ScopeCallable::dynCallArgs(%this, %args)
{
	// improve this
	%args = %this._getArgs(%args);

	for (%i = 0; %i < %args.length && %i < 20; %i++)
	{
		if (%i)
			%list = %list @ ",";
		%list = %list @ "%args.value" @ %i;
	}

	%result = eval("return " @ %this.scope @ "::" @ %this.name @ "(" @ %list @ ");");
	%args.delete();
	return %result;
}


// ====================================
// ObjectCallable: dynamic scoped callable
// determines namespace based on first argument (required)
// `<firstarg>.foo(...)`


function ObjectCallable::__repr__(%this)
{
	if (%this.args.length)
		return "Callable<" @ repr(%this.args.value[0]) @ "::" @ %this.name @ "(" @ %this.__repr_args__() @ ")>";

	return "Callable<{...}::" @ %this.name @ "(" @ %this.__repr_args__() @ ")>";
}

function ObjectCallable::copy(%this)
{
	return tempref(new ScriptObject()
	{
		class = ObjectCallable;
		superClass = Callable;
		args = ref(%this.args.copy());
		args_after = ref(%this.args_after.copy());
		name = %this.name;
	} @ "\x08");
}

function ObjectCallable::isValid(%this)
{
	return 1; // no way to tell.
}

function ObjectCallable::dynCallArgs(%this, %args_mid)
{
	if (%this.args.length + %args_mid.length + %this.args_after.length < 1)
	{
		console.error("Missing object argument for object method callable");
		return "";
	}

	%args = %this.args.copy();
	%args.concat(%args_mid);
	%args.concat(%this.args_after);
	%iter = iter(%args);

	%args.delete(); // too hacky
	%target = %iter.next();
	%args = Tuple(%iter);
	//%iter.delete();

	%result = %target.callArgs(%this.name, %args);
	%args.delete();
	return %result;
}


// ====================================
// CodeCallable: represents a callable block of anonymous code
// host code is protected from accessing the Callable itself
// `{ ... }`


function CodeCallable::__repr__(%this)
{
	return "Callable<{" @ %this.code @ "}(" @ %this.__repr_args__() @ ")>";
}

function CodeCallable::copy(%this)
{
	return tempref(new ScriptObject()
	{
		class = CodeCallable;
		superClass = Callable;
		args = ref(%this.args.copy());
		args_after = ref(%this.args_after.copy());
		code = %this.code;
	} @ "\x08");
}

function CodeCallable::isValid(%this)
{
	return 1;
}

function CodeCallable::dynCallArgs(%this, %args)
{
	%args = %this._getArgs(%args);

	for (%i = 0; %i < %args.length && %i < 20; %i++)
		%arg[%i] = %args.value[%i];

	%args.delete();
	%___code = %this.code;

	// empty the scope.
	%i = "";
	%this = ""; // definitely don't allow access to the callable
	%args = "";

	return eval(%___code);
}