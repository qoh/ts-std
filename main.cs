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
exec("./class.cs");
exec("./console.cs");
exec("./tcp.cs");
exec("./file.cs");

exec("./types/Struct.cs");			// tested
exec("./types/Array.cs");			// tested, %key does not work for sort()
exec("./types/Binary.cs");			// tested
exec("./types/EventEmitter.cs");	// tested, could do with some more
exec("./types/Map.cs");				// tested
exec("./types/Stack.cs");			// tested
exec("./types/Queue.cs");			// tested
exec("./types/Tuple.cs");			// tested
exec("./types/Set.cs");				// tested, needs mathematical set operations

class (TemporaryClass);
	TemporaryClass.definePrivateAttribute("___temp");
	TemporaryClass.___temp = 1;

exec("./classes/Number.cs");
exec("./classes/String.cs");
exec("./classes/bignum/BigInteger.cs");

exec("./libs.cs");

if (!$_::release)
{
	exec("./tests/main.cs");
	exec("./tests/general.cs");
}