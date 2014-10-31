// this is the new version - probably doesn't work yet
function Class(%name,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	if (%name $= "")
	{
		console.error("class(): Concrete class definition must have a name");
		return "";
	}

	%bases = Tuple::fromArgs(
		%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
		%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, a18
	);

	for (%count = 19; %count > 0; %count--)
	{
		if (%base[%count - 1] !$= "")
			break;
	}

	%bases = new ScriptObject()
	{
		class = "Tuple";
		length = %count;
	};

	for (%i = 0; %i < %bases.length; %i++)
	{
		if (!isObject(%bases.value[%i] = nameToID(%a[%i])))
		{
			console.error("Invalid base class `" @ %a[%i] @ "` specified in class definition for " @ %name);
			%bases.delete();
			return "";
		}
	}

	if (!isFunction(%name) && _safeid(%name))
	{
		for (%i = 0; %i < 20; %i++)
		{
			if (%i)
				%args = %args @ ",";
			%args = %args @ "%a" @ %i;
		}

		eval("function " @ %name @ "(" @ %args @ "){return " @ %name @ ".create(" @ %args @ ");}");
	}

	%obj = nameToID(%name);

	if (isObject(%obj) && %obj.class $= "Class")
	{
		%obj._attribs.clear();
		%obj._methods.clear();

		%obj._parents.delete();
		%obj._parents = %bases;
	}
	else
	{
		%obj = new ScriptObject(%name)
		{
			___constant = 1; // never garbage collect a class definition
			class = "Class";

			_attribs = ref(Array());
			_methods = ref(Array());
			_parents = ref(%bases);
		};
	}

	return "";
}

function Class::onRemove(%this)
{
	if ($DEBUG >= 1)
		console.warn("Class definition for `" @ %this.getName() @ "` getting removed!");

	unref(%this._attribs);
	unref(%this._methods);
	unref(%this._parents);
}

function Class::__getParentTree(%this)
{
	if (%this.bases.length == 0)
		return %this.getName() @ " -> Class";

	if (%this.bases.length == 1)
		return %this.getName() @ %this.bases.value0.__getParentTree();

	// for now, just use ambiguous notation for multiple inheritance - too complex to print readily
	return %this.getName() @ join(imap(%this.bases, callableObj(getName)), ", ") @ " -> ... -> Class";
}

function Class::delete(%this) { error("ERROR: delete - you cannot delete classes directly"); }
function Class::setName(%this, %name) { error("ERROR: setName - you cannot rename classes directly"); }

// function Class::dump(%this, %instance)
// {
// 	%self = %instance $= "" ? %this : %instance;
// 	%instance = %instance !$= "";

// 	if (%instance)
// 		echo("Class:\n  " @ %this.getName());
// 	else
// 		echo("Inheritance path:\n  " @ %this.__getParentTree());

// 	echo("Member Fields:");

// 	for (%i = 0; %i < %this._attribs.length; %i++)
// 	{
// 		%attrib = %this._attribs.value[%i];
// 		%value = %self.getAttribute(%attrib.name);

// 		if (%value !$= "")
// 			%value = " = " @ repr(%value);

// 		%member[%attrib.name] = 1;

// 		echo("  " @ %attrib.type @ %attrib.name @ (%attrib.array ? "[...]" : "") @ %value);

// 		if (%attrib.desc !$= "")
// 			echo("    " @ strReplace(%attrib.desc, "\n", "\n    "));
// 	}

// 	echo("Tagged Fields:");

// 	for (%i = 0; (%field = %self.getTaggedField(%i)) !$= ""; %i++)
// 	{
// 		%name = getField(%field, 0);

// 		if (%member[%name] ||
// 			%name $= "___ref" || %name $= "___ref_sched" ||
// 			%name $= "___temp" || %name $= "___constant" ||
// 			%name $= "class" || %name $= "superClass" || (%instance $= "" && (
// 			%name $= "_attribs" || %name $= "_methods" || %name $= "_parents")))
// 			continue;

// 		echo("  " @ %name SPC "= \"" @ expandEscape(getField(%field, 1)) @ "\"");
// 	}

// 	echo(
// 		"Methods:" NL
// 		"  any call(string name, ...)" NL
// 		"    Call method *name* on the object passing it varargs." NL
// 		"  any callArgs(string name, Iterable args)" NL
// 		"    Call method *name* on the object passing it *args* as arguments." NL
// 		"  void dump()" NL
// 		"    Display a list of fields and methods." NL
// 		"  bool hasMethod(string name)" NL
// 		"    Check if the object has an implementation for the method."
// 	);

// 	if (%instance)
// 	{
// 		echo(
// 			"  any super(string name, ...)" NL
// 			"    Call method *name* on the instance with varargs, but use the parent class implementation." NL
// 			"  any superArgs(string name, Iterable args)" NL
// 			"    Call method *name* on the instance with *args* as arguments, but use the parent class implementation." NL
// 		);
// 	}
// 	else
// 	{
// 		echo(
// 			"  " @ %this.getName() @ " create(...)" NL
// 			"    Create a new instance of the class, passing the arguments to the constructor."
// 		);
// 	}

// 	// TOOD: display methods of parent classes
// 	for (%i = 0; %i < %this._methods.length; %i++)
// 	{
// 		%method = %this._methods.value[%i];
// 		echo("  " @ %method.type SPC %method.name @ "(<todo>)");

// 		if (%method.desc !$= "")
// 			echo("    " @ strReplace(%method.desc, "\n", "\n    "));
// 	}
// }

function Class::create(%this,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	%instance = new ScriptObject()
	{
		class = "ClassInstance";
		__class = %this;
		__stack = -1;
	} @ "\cp\co";

	for (%i = 0; %i < %this._attribs.length; %i++)
	{
		%attrib = %this._attribs.value[%i];
		%value = %self.getAttribute(%attrib.name);
		%instance.setAttribute(%attrib.name, %value, %attrib.type);
	}

	if (%this.__temp)
		tempref(%instance);

	if (%instance.hasMethod("__construct__"))
		%instance.callArgs("__construct__", Tuple::fromArgs(
			%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
			%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17
		));

	return %instance;
}

function Class::__resolveMethod(%this, %name, %skipThis)
{
	if (!%skipThis && isFunction(%this.getName(), %name))
		return %this;

	for (%i = 0; %i < %this._parents.length; %i++)
	{
		if (%this._parents.value[%i].hasMethod(%name))
			return %this._parents.value[%i];
	}

	return 0;
}

function Class::hasMethod(%this, %name)
{
	return Parent::hasMethod(%this, %name) || %this.__resolveMethod(%name);
}

// ClassInstance implementation
function ClassInstance::onRemove(%this)
{
	if (%this.hasMethod("__destroy__"))
		%this.callArgs("__destroy__", Tuple());
}

// function ClassInstance::dump(%this)
// {
// 	%this.__class.dump(%this);
// }

function ClassInstance::hasMethod(%this, %name)
{
	return Parent::hasMethod(%this, %name) || %this.__class.__resolveMethod(%name);
}

function ClassInstance::call(%this, %name,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17)
{
	return %this.callArgs(%name, Tuple::fromArgs(
		%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
		%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17
	));
}

function ClassInstance::callArgs(%this, %name, %args)
{
	%impl = %this.__class.__resolveMethod(%name, %super);

	if (!%impl)
	{
		console.error("Cannot find method `" @ %name @ "` for class `" @ %this.__class.getName() @ "`");
		%args.delete();
		return "";
	}

	// please pass us a tuple, thanks
	// it's faster.
	if (%args.class $= "Tuple")
	{
		for (%i = 0; %i < %args.length; %i++)
			%a[%i] = %args.value[%i];
	}
	else
	{
		if (assert(%iter = iter(%args), "args is not iterable"))
			return "";

		for (%i = 0; %iter.hasNext() && %i < 20; %i++)
			%a[%i] = %iter.next();
		
		%iter.delete();
	}

	// TODO: benchmark assembling %aX list VS many explicit args call
	return eval("return %impl." @ %name @ "(%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15,%a16,%a17,%a18,%a19);");
}

// alias of call()
function Class::_(%this, %name,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16)
{
	return %this.callArgs(%name, Tuple::fromArgs(
		%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
		%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17
	));
}
