function _qswap(%v, %i, %j)
{
	%t = %v.value[%i];
	%v.value[%i] = %v.value[%j];
	%v.value[%j] = %t;
}

function _qsort(%v, %left, %right, %cmp)
{
	if (%left >= %right)
		return;

	_qswap(%v, %left, (%left + %right) >> 1);
	%last = %left;

	for (%i = %left + 1; %i <= %right; %i++)
		if (Callable::call(%cmp, %v.value[%left], %v.value[%right]) < 0)
			_qswap(%v, %last++, %i);

	_qswap(%v, %left, %last);
	_qsort(%v, %left, %last - 1, %cmp);
	_qsort(%v, %last + 1, %right, %cmp);
}

function _safeattrtail(%attr)
{
	%len = strlen(%attr);

	for (%i = 1; %i < %len; %i++)
	{
		if (stripos("_abcdefghijklmnopqrstuvwxyz0123456789", getSubStr(%attr, %i, 1)) == -1)
			return 0;
	}

	return 1;
}