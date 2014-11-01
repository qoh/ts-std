function _qsort(%seq, %left, %right, %key)
{
	if (%left >= %right)
		return;

	%seq.swap(%left, mFloor((%left + %right) / 2));
	%last = %left;

	for (%i = %left+1; %i <= %right; %i++)
	{
		%a = %seq.value[%i];
		%b = %seq.value[%left];

		if (%key !$= "")
		{
			%a = dynCall(%key, %a);
			%b = dynCall(%key, %b);
		}

		if (cmp(%a, %b) < 0)
			%seq.swap(%last++, %i);
	}

	%seq.swap(%left, %last);
	_qsort(%seq, %left, %last-1, %key);
	_qsort(%seq, %last+1, %right, %key);
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