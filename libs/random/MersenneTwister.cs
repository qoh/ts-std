function MersenneTwister(%seed)
{
	return RandomGenerator(%seed, "MersenneTwister");
}

function MersenneTwister::_seed(%this, %value)
{
	%this.index = 0;
	%this.state0 = %value | 0;

	for (%i = 1; %i < 624; %i++)
					    // 0x6c078965
		%this.state[%i] = (1812433253 * (%this.state[%i - 1] ^ (%this.state[%i - 1] >> 30)) + %i) | 0;
}

function MersenneTwister::_next(%this)
{
	if ((%index = %this.index) == 0)
		%this._generate_numbers();

	%y = %this.state[%index];
	%y ^=  %y >> 11;
	%y ^= (%y <<  7) & 2636928640; // 0x9d2c5680
	%y ^= (%y << 15) & 4022730752; // 0xefc60000
	//%y ^=  %y >> 18;

	%this.index = (%index + 1) % 624;
	return %y ^ (%y >> 18);
}

function MersenneTwister::_generate_numbers(%this)
{
	for (%i = 0; %i < 624; %i++)
	{
		%y = ((%this.state[%i] & 0x80000000) + (%this.state[(%i + 1) % 624] & 0x7fffffff)) | 0;
		%this.state[%i] = %this.state[(%i + 397) % 624] ^ (%y >> 1);

		if (%y & 1 == 1)
			%this.state[%i] ^= 2567483615; // 0x9908b0df
	}
}