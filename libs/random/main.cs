// Private: Used by RandomGenerator subclasses to construct themselves.
function RandomGenerator(%seed, %class)
{
	if (assert(%class !$= "", "class must be given"))
		return 0;

	%gen = new ScriptObject()
	{
		class = %class;
		superClass = "RandomGenerator";
	};

	if (%seed $= "")
		%seed = getRealTime();

	%gen.setSeed(%seed);
	return %gen;
}

function RandomGenerator::_seed(%this, %value)
{
}

function RandomGenerator::_next(%this)
{
	console.error("Subclasses of RandomGenerator should implement next()");
	return 0x0;
}

function RandomGenerator::setSeed(%this, %value)
{
	%this.initialSeed = %value;
	%this._seed(%value);
}

function RandomGenerator::getSeed(%this)
{
	return %this.initialSeed;
}

function RandomGenerator::nextDist(%this, %dist)
{
	return Callable::call(%dist, %this._next());
}

function RandomGenerator::nextFloat(%this, %min, %max)
{
	%int = (%this._next() & 0x7fffffff) / 0x7fffffff;

	if (%min $= "")
		%min = 1;

	if (%max $= "")
	{
		%min = 0;
		%max = %min;
	}

	if (%min == 0 && %max == 1)
		return %int;

	if (%min == 0)
	{
		if (%max == 0)
			return %int;

		return %int * %max;
	}

	return %min + (%int * (%max - %min));
}

function RandomGenerator::nextInt(%this, %min, %max)
{
	if (%min $= "")
		%min = 1;

	if (%max $= "")
	{
		%max = %min;
		%min = 0;
	}

	%max = (%max + 1) | 0;

	if (%max < %min)
	{
		%this._next(); // sync rand
		return %min;
	}

	// FIXME: causes really weird distributions
	//return (%min + (%this._next() & 0x7fffffff) % ((%max - %min) + 1)) | 0;
	return (%min + (((%this._next() & 0x7fffffff) % ((%max - %min) | 0)) | 0)) | 0;
}

function RandomGenerator::choose(%this, %seq)
{
	return getitem(%seq, %this.nextInt((len(%seq) - 1) | 0));
}

function RandomGenerator::chooseIter(%this, %seq)
{
	if (assert(%iter = iter(%seq), "seq must be iterable"))
		return "";

	%value = "";
	%index = 0;

	while (%iter.hasNext())
	{
		if (((%this._next() % %index) | 0) == 0)
			%value = %iter.next();
		else
			%iter.next(); // sync iter

		%index = (%index + 1) | 0;
	}

	%iter.delete();
	return %value;
}

function RandomGenerator::shuffle(%this, %seq)
{

}

exec("./LinearCongruentialGenerator.cs");
exec("./MersenneTwister.cs");

exec("./diehard.cs");

if (!isObject(Random))
	ref(MersenneTwister()).setName("Random");