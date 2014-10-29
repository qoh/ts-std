if (!isObject(_.required_libs))
	_.required_libs = ref(Array());

if (!isObject(_.loaded_libs))
	_.loaded_libs = ref(Array());

function _::require(%this, %lib, %reload)
{
	if (!%this.required_libs.contains(%lib))
		%this.required_libs.append(%lib);

	%index = %this.loaded_libs.find(%lib);

	if (%index != -1 && !%reload)
	{
		echo("no! need to reload!");
		return 0;
	}

	%entry = %this.path @ "libs/" @ %lib @ "/main.cs";

	if (!isFile(%entry) && !isFile(%entry @ ".dso"))
	{
		console.error("require() cannot find library '" @ %lib @ "'");
		return 1;
	}

	$_::lib::explicit_success = 0;
	$_::lib::success = 0;

	exec(%entry);

	if ($_::lib::explicit_success && !$_::lib::success)
	{
		console.error("require() encountered an error while loading library '" @ %lib @ "'");
		return 1;
	}

	if (%index != -1)
		%this.loaded_libs.pop(%index);

	%this.loaded_libs.append(%lib);
	return 0;
}

function _::_require_prev(%this)
{
	// Update engine file cache
	setModPaths(getModPaths());

	for (%i = 0; %i < %this.required_libs.length; %i++)
		%this.require(%this.required_libs.value[%i], 1);
}

_._require_prev();