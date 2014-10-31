$_::last::test_libs = 1;
$_::last::release = 0;
$_::last::version = 1;

if (isObject(_))
{
	if ($_::last::version < _.version)
		return;

	if ($_::last::release && $_::last::version == _.version)
		return;

	_.release = $_::last::release;
	_.version = $_::last::version;

	_.file = $Con::File;
	_.path = filePath($Con::File) @ "/";
}
else
	new ScriptObject(_)
	{
		release = $_::last::release;
		version = $_::last::version;

		file = $Con::File;
		path = filePath($Con::File) @ "/";
	};

exec("./internal.cs");
exec("./generic.cs");
exec("./iterators.cs");
exec("./callable.cs");
exec("./string.cs");
//exec("./class.cs");
exec("./class_new.cs");
exec("./event.cs");
exec("./console.cs");
exec("./tcp.cs");
exec("./file.cs");

exec("./types/Struct.cs");
exec("./types/Array.cs");
exec("./types/ByteArray.cs");
exec("./types/Map.cs");
exec("./types/Stack.cs");
exec("./types/Queue.cs");
exec("./types/Tuple.cs");
exec("./types/Set.cs");
exec("./types/String.cs");

exec("./libs.cs");

//exec("./libs/json/main.cs");
//exec("./libs/regex/main.cs");
//exec("./libs/redis/main.cs");

if (!$_::release)
{
	exec("./tests/main.cs");
	exec("./tests/general.cs");
}

if ($_::last::test_libs)
{
	// _.require("");
}