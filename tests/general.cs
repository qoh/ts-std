function ts_test_array()
{
	%array = tempref(Array());
	if (assert(isObject(%array), "new Array creation succeeds", "isObject(%array)")) return;

	console.info("New array: " @ repr(%array));
	if (assert(len(%array) == 0, "new Array is empty", "len(%array) == 0")) return;

	for (%i = 0; %i < 10; %i++)
		%array.append(random.nextInt(20));

	if (assert(len(%array) == 10, "Array length after adding elements is 10", "len(%array) == 10")) return;

	console.info("Inserting [1, 2, 3] at start...");
	%array.insert(0, 3);
	%array.insert(0, 2);
	%array.insert(0, 1);

	if (assert(len(%array) == 13, "Array length after adding elements is 13", "len(%array) == 13")) return;
	if (assert(%array.contains(3), "Array contains 3", "%array.contains(3)")) return;
	if (assert(%array.find(2) == 1, "Value 2 is at 2nd index in Array", "%array.find(2) == 1")) return;
	if (assert(!%array.contains("foo"), "Array does not contain 'foo'", "!%array.contains(\"foo\")")) return;
	if (assert(eq(%array, tempref(%array.copy())), "Array is not equal to copied Array", "eq(%array, tempref(%array.copy()))")) return;

	%iter = iter(%array);

	if (assert(%iter, "Array is iterable")) return;

	for (%i = 0; %i < %array.length; %i++)
	{
		if (!%iter.hasNext())
		{
			if (assert(false, "Array has same length as produced iterator", "false"))
				return;
		}

		if (!eq(%array.value[%i], %iter.next()))
		{
			if (assert(false, "Array values match iterator values", "false", false))
				return;
		}
	}

	if (assert(!%iter.hasNext(), "Array has same length as produced iterator", "!%iter.hasNext()"))
		return;

	if (assert(!tempref(%array.filter(bool).contains(0)), "bool-filtered Array does not contain 0", "!tempref(%array.filter(bool).contains(0)))"))
		return;

	%array.reverse();
	console.info("Reversed: " @ repr(%array));

	%array.clear();

	if (assert(len(%array) == 0, "Array is empty after clear()", "len(%array) == 0"))
		return;

	%array.delete();
}

ts_test.add("Array", ts_test_array);