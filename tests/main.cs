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

	if (assert(isCallable(%callable), "callable is not valid for test " @ %name))
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

	%orig_true  = $test_true;
	%orig_false = $test_false;
	$test_true  = 0;
	$test_false = 0;

	dynCallArgs(%callable);

	%test_true  = $test_true;
	%test_false = $test_false;
	$test_true  = %orig_true;
	$test_false = %orig_false;

	%asserts = %test_true SPC "out of" SPC (%test_true + %test_false) SPC "assert(s)";

	console.log("");
	console.log((%test_false ? "\c2Test failed: " : "\c9Test succeeded: ") @ %asserts SPC "OK");

	console.endGroup();
	$assert_debug--;

	return %test_false == 0;
}

function test(%condition, %message, %expr)
{
	$test_true += !!%condition;
	$test_false += !%condition;

	if (%message !$= "")
	{
		if (%condition)
			console.log("\c9v  Success: " SPC %message @ (%expr $= "" ? "" : "  [" @ %expr @ "]"));
		else
			console.log("\c2" @ chr(215) @ "  Failed: " SPC %message @ (%expr $= "" ? "" : "  [" @ %expr @ "]"));
	}

	return !%condition;
}