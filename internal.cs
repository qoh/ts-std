// function _qswap(%v, %i, %j)
// {
// 	%t = %v.value[%i];
// 	%v.value[%i] = %v.value[%j];
// 	%v.value[%j] = %t;
// }

function _qsort(%v, %left, %right, %cmp)
{
	if (%left >= %right)
		return;

	//_qswap(%v, %left, (%left + %right) >> 1);
	%v.swap(%left, (%left + %right) >> 1);
	%last = %left;

	for (%i = %left + 1; %i <= %right; %i++)
		if (dynCall(%cmp, %v.value[%left], %v.value[%right]) < 0)
			//_qswap(%v, %last++, %i);
			%v.swap(%last++, %i);

	//_qswap(%v, %left, %last);
	%v.swap(%left, %last);
	_qsort(%v, %left, %last - 1, %cmp);
	_qsort(%v, %last + 1, %right, %cmp);
}

function _safeid(%attr)
{
	if (%attr $= "" || stripos("_abcdefghijklmnopqrstuvwxyz", getSubStr(%attr, 0, 1)) == -1)
		return 0;

	%len = strlen(%attr);

	for (%i = 1; %i < %len; %i++)
	{
		if (stripos("_abcdefghijklmnopqrstuvwxyz0123456789", getSubStr(%attr, %i, 1)) == -1)
			return 0;
	}

	return 1;
}

package ConsoleEntryExprPackage
{
	function ConsoleEntry::eval()
	{
		%value = ConsoleEntry.getValue();

		if (%value !$= "")
		{
			%trimmed = rtrim(%value);
			%last = getSubStr(%trimmed, strlen(%trimmed) - 1, 1);

			if (%last !$= ";" && %last !$= "}")
			{
				ConsoleEntry.setValue("");

				echo("==>" @ %value);
				eval("console.log(repr(" @ %value @ "));");

				return;
			}
		}

		Parent::eval();
	}
};

activatePackage("ConsoleEntryExprPackage");