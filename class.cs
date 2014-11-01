function isInstanceOf(%obj, %class)
{
	if (!isObject(%obj))
		return 0;

	if (%class !$= "Class" && isObject(%class) && %class.getID() == %obj.__class)
		return 1;

	return %obj.__class.inheritsFrom(%class);
}

function Class(%name,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	if (%name $= "")
	{
		console.error("class(): Concrete class definition must have a name");
		return "";
	}

	for (%count = 19; %count > 0; %count--)
	{
		if (%a[%count - 1] !$= "")
			break;
	}

	%bases = new ScriptObject()
	{
		class = "TupleInstance";
		length = %count;
	} @ "\x08";

	for (%i = 0; %i < %count; %i++)
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
		%obj._attribs_private.clear();
		%obj._methods.clear();
		unref(%obj._parents);
	}
	else
	{
		%obj = new ScriptObject(%name)
		{
			___constant = 1; // never garbage collect a class definition
			class = "Class";

			_attribs = ref(Array());
			_attribs_private = ref(Array());
			_methods = ref(Array());
		};
	}

	//for (%i = 0; %i < %bases.length; %i++)
	//{
		//%obj._attribs.concat(%bases.value[%i]._attribs);
		//%obj._attribs_private.concat(%bases.value[%i]._attribs_private);
		//%obj._methods.concat(%bases.value[%i]._methods);
	//}

	%obj._parents = %bases;

	return "";
}

function Class::onRemove(%this)
{
	if ($DEBUG >= 1)
		console.warn("Class definition for `" @ %this.getName() @ "` getting removed!");

	unref(%this._attribs);
	unref(%this._attribs_private);
	unref(%this._methods);
	unref(%this._parents);
}

function Class::__getParentTree(%this)
{
	if (%this._parents.length == 0)
		return %this.getName() @ " -> Class";

	if (%this._parents.length == 1)
		return %this.getName() @ " -> " @ %this._parents.value0.__getParentTree();

	// for now, just use ambiguous notation for multiple inheritance - too complex to print readily
	return %this.getName() @ join(imap(callableObj(getName), %this._parents), ", ") @ " -> ... -> Class";
}

function Class::delete(%this) { error("ERROR: delete - you cannot delete classes directly"); }
function Class::setName(%this, %name) { error("ERROR: setName - you cannot rename classes directly"); }

function Class::dump(%this, %instance)
{
	%self = %instance $= "" ? %this : %instance;
	%instance = %instance !$= "";

	if (%instance)
		echo("Class: " @ %this.getName());
	else
		echo("Inheritance path:\n  " @ %this.__getParentTree());

	%header = 0;

	for (%i = 0; %i < %this._attribs.length; %i++)
	{
		%attrib = %this._attribs.value[%i];
		%value = %self.getAttribute(%attrib.name);

		if (%value !$= "")
			%value = " = " @ repr(%value);

		%member[%attrib.name] = 1;

		if (!%header)
		{
			echo("Member Fields:");
			%header = 1;
		}

		echo("  " @ %attrib.type SPC %attrib.name @ (%attrib.array ? "[...]" : "") @ %value);

		if (%attrib.desc !$= "")
			echo("    " @ strReplace(%attrib.desc, "\n", "\n    "));
	}

	for (%i = 0; %i < %this._attribs_private.length; %i++)
		%member[%this._attribs_private.value[%i]] = 1;

	%header = 0;

	for (%i = 0; (%field = %self.getTaggedField(%i)) !$= ""; %i++)
	{
		%name = getField(%field, 0);

		if (%member[%name] ||
			%name $= "___ref" || %name $= "___ref_sched" ||
			%name $= "___temp" || %name $= "___constant" ||
			%name $= "class" || %name $= "superClass")
			continue;

		if (%instance)
		{
			if (%name $= "__class")
				continue;
		}
		else if (%name $= "_attribs" || %name $= "_attribs_private" || %name $= "_methods" || %name $= "_parents")
			continue;

		if (!%header)
		{
			echo("Tagged Fields:");
			%header = 1;
		}

		echo("  " @ %name SPC "= \"" @ expandEscape(getField(%field, 1)) @ "\"");
	}

	echo(
		"Methods:" NL
		"  any call(string name, ...)" NL
		"    Call method *name* on the object passing it varargs." NL
		"  any callArgs(string name, Iterable args)" NL
		"    Call method *name* on the object passing it *args* as arguments." NL
		"  void dump()" NL
		"    Display a list of fields and methods." NL
		"  bool hasMethod(string name)" NL
		"    Check if the object has an implementation for the method."
	);

	if (!%instance)
	{
		echo(
			"  " @ %this.getName() @ " create(...)" NL
			"    Create a new instance of the class, passing the arguments to the constructor."
		);
	}

	// TOOD: display methods of parent classes
	for (%i = 0; %i < %this._methods.length; %i++)
	{
		%method = %this._methods.value[%i];
		echo("  " @ %method.type SPC %method.name @ "(" @ %method.args @ ")");

		if (%method.desc !$= "")
			echo("    " @ strReplace(%method.desc, "\n", "\n    "));
	}
}

function Class::defineAttribute(%this, %name, %type, %desc, %array, %value)
{
	for (%i = 0; %i < %this._attribs.length; %i++)
	{
		if (%this._attribs.value[%i].name $= %name)
		{
			%this._attribs.pop(%i);
			break;
		}
	}

	if (%type $= "")
		%type = "string";

	%this._attribs.append(new ScriptObject()
	{
		class = "Struct";
		name = %name;
		type = %type;
		desc = %desc;
		array = %array;
	} @ "\x08");

	if (%value !$= "")
		%this.setAttribute(%name, %value);
}

function Class::definePrivateAttribute(%this, %name)
{
	if (!%this._attribs_private.contains(%name))
		%this._attribs_private.append(%name);
}

function Class::defineMethod(%this, %name, %type, %args, %desc)
{
	for (%i = 0; %i < %this._methods.length; %i++)
	{
		if (%this._methods.value[%i].name $= %name)
		{
			%this._methods.pop(%i);
			break;
		}
	}

	if (%type $= "")
		%type = "string";

	%this._methods.append(new ScriptObject()
	{
		class = "Struct";
		name = %name;
		type = %type;
		args = %args;
		desc = %desc;
	} @ "\x08");
}

function Class::create(%this,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	%instance = new ScriptObject()
	{
		class = "ClassInstance";
		__class = %this.getID();
	} @ "\x08";

	%this.__copyAttribs(%instance);

	if (%instance.___temp)
		tempref(%instance);

	if (%instance.hasMethod("__construct__"))
		%instance.callArgs("__construct__", Tuple::fromArgs(
			%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
			%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17
		));

	return %instance;
}

function Class::inheritsFrom(%this, %other)
{
	if (%other $= "Class")
		return 1;

	if (!isObject(%other))
		return 0;

	%other = %other.getID();

	for (%i = 0; %i < %this._parents.length; %i++)
	{
		if (%this._parents.value[%i].getID() == %other)
			return 1;
	}

	return 0;
}

function Class::__copyAttribs(%this, %instance)
{
	for (%i = 0; %i < %this._parents.length; %i++)
		%this._parents.value[%i].__copyAttribs(%instance);

	for (%i = 0; %i < %this._attribs.length; %i++)
	{
		%attrib = %this._attribs.value[%i];
		%instance.setAttribute(%attrib.name, %this.getAttribute(%attrib.name));
	}

	for (%i = 0; %i < %this._attribs_private.length; %i++)
	{
		%attrib = %this._attribs_private.value[%i];
		%instance.setAttribute(%attrib, %this.getAttribute(%attrib));
	}
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

	return "";
}

function Class::hasMethod(%this, %name)
{
	return Parent::hasMethod(%this, %name) || %this.__resolveMethod(%name) !$= "";
}

// ClassInstance implementation
function ClassInstance::onRemove(%this)
{
	if (%this.hasMethod("__destruct__"))
		%this.callArgs("__destruct__", Tuple());
}

function ClassInstance::dump(%this)
{
	%this.__class.dump(%this);
}

function ClassInstance::hasMethod(%this, %name)
{
	return Parent::hasMethod(%this, %name) || %this.__class.__resolveMethod(%name) !$= "";
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
	%impl = %this.__class.__resolveMethod(%name);

	if (%impl $= "")
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

		for (%i = 0; %iter.hasNext() && %i < 19; %i++)
			%a[%i] = %iter.next();
		
		%iter.delete();
	}

	// TODO: benchmark assembling %aX list VS many explicit args call
	return eval("return " @ %impl.getName() @ "::" @ %name @ "(%this,%a0,%a1,%a2,%a3,%a4,%a5,%a6,%a7,%a8,%a9,%a10,%a11,%a12,%a13,%a14,%a15,%a16,%a17,%a18);");
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