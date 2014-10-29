function parking_lot(%source, %samples)
{
	if (%samples $= "")
		%samples = 1200;

	if (!%source.hasMethod("nextFloat"))
	{
		echo("bad source");
		return;
	}

	%carx0 = 100 * %source.nextFloat();
	%cary0 = 100 * %source.nextFloat();

	%cars = 1;

	for (%n = 1; %n < %samples; %n++)
	{
		if (%samples > 2000 && %n % 1000 == 0)
			echo(%n);

		%x = 100 * %source.nextFloat();
		%y = 100 * %source.nextFloat();

		for (%i = 0; %i < %cars; %i++)
		{
			//if (mAbs(%carx[%i] - %x) <= 1 && mAbs(%cary[%i] - %y))
			if ((mPow(%carx[%i] - %x, 2) + mPow(%cary[%i] - %y, 2)) < 2)
				break;
		}

		if (%i == %cars)
		{
			%carx[%cars] = %x;
			%cary[%cars] = %y;
			%cars++;
		}
	}

	echo("parking_lot: " @ %samples @ " samples, " @ %cars @ " parked, " @ %samples - %cars @ " crashed");
}

function runs(%source, %samples)
{
	if (%samples $= "")
		%samples = 1200;

	if (!%source.hasMethod("nextFloat"))
	{
		echo("bad source");
		return;
	}

	%runs = 0;
	%last = -1;

	%samples0 = 0;
	%samples1 = 0;

	for (%i = 0; %i < %samples; %i++)
	{
		%curr = %source.nextFloat();
		%samples[%value = %curr >= 0.5]++;

		if (%value != %last)
		{
			%last = %value;
			%runs++;
		}
	}

	%mean = (2 * %samples0 * %samples1) / %samples + 1;
	%variance = ((%mean - 1) * (%mean - 2)) / (%samples - 1);

	echo("runs: " @ %samples @ " samples, " @ %runs @ " runs");
	echo("  " @ %samples0 @ " down samples, " @ %samples1 @ " up samples");
	echo("  expected: " @ %mean @ " mean, " @ %variance @ " variance");

	return %runs;
}