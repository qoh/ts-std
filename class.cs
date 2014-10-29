function Class(%name, %base)
{
	if (isObject(%name))
	{
		return %name.getID();
	}

	if (!isFunction(%name) && Map::__isSafe(%name)) // need general-purpose identifier test
	{
		for (%i = 0; %i < 20; %i++)
		{
			if (%i)
				%args = %args @ ",";
			%args = %args @ "%a" @ %i;
		}

		eval("function " @ %name @ "(" @ %args @ "){return " @ %name @ ".create(" @ %args @ ");}");
	}

	return obj(new ScriptObject(%name)
	{
		class = "Class";
		base = isObject(%base) ? %base.getID() : 0;
	});
}

function Class::create(%this,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17)
{
	if (%this.__instance)
	{
		error("ERROR: Trying to use Class::create on a class instance");
		return "";
	}

	%instance = new ScriptObject()
	{
		class = %this.getName();
		superClass = "Class";
		__class = %this;
		__instance = 1;
		___stack = -1;
	};

	if (%instance.hasMethod("constructor"))
		%instance.call("constructor",
			%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
			%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17);

	return makeRealObject(%instance, %this.tempref);
}

function Class::__repr__(%this)
{
	if (%this.__instance)
	{
		%name = %this.__class.getName();

		if (%name $= "")
			%name = "anonymous";

		return %name SPC "class instance" SPC %this.getName() @ "(" @ %this.getID() @ ")";
	}

	%text = %this.getName();

	if (%text $= "")
		%text = "anonymous <" @ %this.getID() @ ">";

	if (isObject(%this.base))
	{
		%base = %this.base.getName();

		if (%base $= "")
			%base = "anonymous <" @ %this.base.getID() @ ">";

		%base = " : " @ %base;
	}

	return "class (" @ %text @ %base @ ")";
}

function Class::__formatBaseTree(%this)
{
	%self = %this.getName();

	if (%self $= "")
		%self = "anonymous <" @ %this.getID() @ ">";

	if (isObject(%this.base))
	{
		if (%this.base.base == %this)
			return %self @ " -> ... -> Class -> ScriptObject -> SimObject";

		return %self @ " -> " @ %this.base.__formatBaseTree();
	}

	return %self @ " -> Class -> ScriptObject -> SimObject";
}

function Class::setName(%this, %name)
{
	if (%this.__instance)
		Parent::setName(%this, %name);
}

function Class::delete(%this)
{
	if (%this.__instance)
		Parent::delete(%this);

	// TODO: remove this in favor of finding a good way to handle orphaned class instances
}

function Class::___push_class(%this, %class)
{
	%this.___stack = (%this.___stack + 1) | 0;
	%this.___stack[%this.___stack] = %class;
}

function Class::___pop_class(%this)
{
	if (%this.___stack != -1)
	{
		%this.___stack[%this.___stack] = "";
		%this.___stack = (%this.___stack - 1) | 0;
	}
}

function Class::___get_class(%this)
{
	if (%this.___stack == -1)
		return %this.__class;

	return %this.___stack[%this.___stack];
}

function Class::onRemove(%this)
{
	if (%this.__instance && %this.hasMethod("destructor"))
		%this.call("destructor");
}

function Class::hasMethod(%this, %method)
{
	if (SimObject::hasMethod(%this, %method))
		return 1;

	if (%this.__instance)
	{
		%target = %this.___get_class();

		if (%target.__instance)
			return 0;

		return %target.hasMethod(%method);
	}

	%target = %this;

	while (isObject(%target) && !isFunction(%target.getName(), %method))
		%target = %target.base;

	return isObject(%target) && isFunction(%target.getName(), %method);
}

function Class::call(%this, %method,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %super)
{
	if (!%this.__instance)
		return "";

	%target = %this.___get_class();

	if (%super)
		%target = %target.base;

	while (isObject(%target) && !isFunction(%target.getName(), %method))
		%target = %target.base;

	if (!isObject(%target) || !isFunction(%target.getName(), %method))
	{
		%describe = %this.__describeInstance() SPC %this.__class.__describeBaseTree();
		echo("\c1<input> (0): Unknown command " @ %method @ ".\n  \c1" @ %describe);
		return "";
	}

	for (%count = 17; %count > 0; %count--)
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

	// TODO: add optional setting to verify `method` as a valid identifier
	//       though really, it's up to the developer to not send user input into %method
	//       not a priority

	%this.___push_class(%target);
	%result = eval("return " @ %target.getName() @ "::" @ %method @ "(%this," @ %args @ ");");
	%this.___pop_class();
	return %result;
}

function Class::super(%this, %method,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16)
{
	return %this.call(%method,
		%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
		%a10, %a11, %a12, %a13, %a14, %a15, %a16, 1);
}

function Class::_(%this, %method,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16)
{
	return %this.call(%method,
		%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
		%a10, %a11, %a12, %a13, %a14, %a15, %a16, 0);
}