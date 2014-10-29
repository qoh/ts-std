function LinearCongruentialGenerator(%seed)
{
	return RandomGenerator(%seed, "LinearCongruentialGenerator");
}

function LinearCongruentialGenerator::onAdd(%this)
{
	// based on glibc
	%this.a = 1103515245;
	%this.c = 12345;
	%this.m = 0x7fffffff;
}

function LinearCongruentialGenerator::setParams(%this, %a, %c, %m)
{
	%this.a = %a;
	%this.c = %c;
	%this.m = %m;

	%this._seed(%this.getSeed());
}

function LinearCongruentialGenerator::_seed(%this, %value)
{
	%this._seed = %value & %this.m;
}

function LinearCongruentialGenerator::_next(%this)
{
	return %this._seed = (%this._seed * %this.a + %this.c) & %this.m;
}