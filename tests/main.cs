if (!isObject(ts_test))
{
	new ScriptObject(ts_test)
	{
		tests = Map(1);
	};
}

function ts_test::add(%this, %name, %callable)
{
	if (assert(%name !$= "", "test name cannot be empty"))
		return 1;

	if (assert(Callable::isValid(%callable), "callable is not valid for test " @ %name))
		return 1;

	%this.tests.set(%name, %callable);
	return 0;
}

function ts_test::run(%this)
{
	console.group("Running all tests");
	%iter = iter(%this.tests.keys());

	while (%iter.hasNext())
		%this.run_one(%iter.next());

	console.endGroup();
}

function ts_test::run_one(%this, %name)
{
	%callable = %this.tests.get(%name, 0);

	$assert_debug++;
	console.group("Running test: " @ %name);

	%orig_true    = $assert_true;
	%orig_false   = $assert_false;
	$assert_true  = 0;
	$assert_false = 0;

	Callable::call(%callable);

	%test_true    = $assert_true;
	%test_false   = $assert_false;
	$assert_true  = %orig_true;
	$assert_false = %orig_false;

	%asserts = %test_true SPC "out of" SPC (%test_true + %test_false) SPC "assert(s)";

	console.log("");
	console.log((%test_false ? "\c2Test failed: " : "\c9Test succeeded: ") @ %asserts SPC "OK");

	console.endGroup();
	$assert_debug--;

	return %test_false == 0;
}