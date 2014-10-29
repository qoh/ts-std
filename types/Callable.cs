function Callable(%scope, %name, %refer)
{
	%obj = tempref(new ScriptObject()
	{
		class = "CallableInstance";

		_refer = %refer;
		mode = 0;
		args = ref(Array("", 1));
		args_after = ref(Array("", 1));
	});

	if (%name $= "")
	{
		if (%scope.class $= "CallableInstance")
		{
			%obj.mode = %scope.mode;
			%obj.scope = %scope.scope;
			%obj.name = %scope.name;
			%obj.args.concat(%scope.args);
			%obj.args_after.concat(%scope.args_after);

			return %obj;
		}

		if (assert(isFunction(%scope), "Function " @ %scope @ " does not exist"))
			return 0;

		%obj.name = %scope;
		return %obj;
	}

	if (isObject(%scope))
	{
		if (assert(%scope.hasMethod(%name), "Object " @ %scope @ " does not have method " @ %name))
			return 0;

		if (%obj._refer)
			ref(%scope);

		%obj.mode = 2;
		%obj.scope = %scope.getID();
		%obj.name = %name;

		return %obj;
	}

	if (assert(isFunction(%scope, %name), "Function " @ %scope @ "::" @ %name @ " does not exist"))
		return 0;

	%obj.mode = 1;
	%obj.scope = %scope;
	%obj.name = %name;

	return %obj;
}

function Callable::eval(%code, %args, %refer)
{
	%concat = Callable(concat).apply("%");
	%args = join(imap(tempref(%args), %concat), ",");

	%name = "____anonymous__callable_" @ sha1(getNonsense());
	eval("function " @ %name @ "(" @ %args @ "){" @ %code @ "}");

	if (assert(isFunction(%name), "cannot create anonymous function"))
		return 0;

	%callable = Callable(%name);
	%callable.code = %code;
	%callable.is_code = 1;

	return %callable;
}

function Callable::isValid(%value)
{
	if (%value.class $= "CallableInstance")
		return %value.isValid();

	return isFunction(%value);
}

function Callable::call(%value,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	if (%value.class $= "CallableInstance")
		return %value.call(%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
						%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18);

	if (!$EngineMethod[%value])
		return call(%value,
			%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
			%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18);

	for (%count = 19; %count > 0; %count--)
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

	if (assert(isFunction(%value), %value SPC "is not a valid callable"))
		return "";

	return eval("return " @ %value @ "(" @ %args @ ");");
}

function CallableInstance::onRemove(%this)
{
	unref(%this.args);

	if (%this._refer && %this.mode == 2)
		unref(%this.scope);
}

function CallableInstance::__repr__(%this)
{
	switch (%this.mode)
	{
		case 0:
			if (%this.is_code)
				return "Callable({ " @ %this.code @ " }, " @ repr(%this.args) @ ")";
			else
				return "Callable(" @ %this.name @ ", " @ repr(%this.args) @ ")";

		case 1: return "Callable(" @ %this.scope @ "::" @ %this.name @ ", " @ repr(%this.args) @ ")";
		case 2: return "Callable(" @ %this.scope @ "." @ %this.name @ ", " @ repr(%this.args) @ ")";
	}

	return "Callable(...)";
}

function CallableInstance::copy(%this)
{
	return Callable(%this, "", %this._refer);
}

function CallableInstance::apply(%this,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	%callable = %this.copy();

	%callable.args.concat(Array::fromArgs(
		%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
		%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18
	));
	
	return %callable;
}

function CallableInstance::isValid(%this)
{
	switch (%this.mode)
	{
		case 0: return isFunction(%this.name);
		case 1: return isFunction(%this.scope, %this.name);
		case 2: return isObject(%this.scope) && %this.scope.hasMethod(%this.name);

		default: return 0;
	}
}

function CallableInstance::call(%this,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	%call_args = Array::fromArgs(
		%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
		%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18
	);

	%list_args = %this.args.copy();
	%list_args.concat(%call_args);
	%list_args.concat(%this.args_after);

	%call_args.delete();

	for (%i = 0; %i < %args.length; %i++)
	{
		if (%i)
			%args = %args @ ",";

		%a[%i] = %args.value[%i]; // x[y] is faster than x.y[z]
		%args = %args @ "%a" @ %i;
	}

	%list_args.delete();

	switch (%this.mode)
	{
		case 0:
			if (assert(isFunction(%this.name), %this SPC "is not a valid callable"))
				return "";

			return eval("return " @ %this.name @ "(" @ %args @ ");");

		case 1:
			if (assert(isFunction(%this.scope, %this.name), %this SPC "is not a valid callable"))
				return "";

			return eval("return " @ %this.scope @ "::" @ %this.name @ "(" @ %args @ ");");

		case 2:
			if (assert(isObject(%this.scope) && %this.scope.hasMethod(%this.name), %this SPC "is not a valid callable"))
				return "";

			if (%this.superClass $= "Class")
				return %this.scope.call(%this.name,
					%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
					%a10, %a11, %a12, %a13, %a14, %a15, %a16);
			else
				return eval("return %this.scope." @ %this.name @ "(" @ %args @ ");");
	}
}