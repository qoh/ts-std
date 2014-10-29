function iter(%value, %sentinel, %forceCopy)
{
	if (%sentinel !$= "" && Callable::isValid(%value))
	{
		return FunctionIterator(_isentinel_hasNext, _isentinel_next,
			new ScriptObject()
			{
				class = Struct;
				func = %value;
				sentinel = %sentinel;
			});
	}

	if (%value.superClass $= "Iterator")
	{
		if (%forceCopy)
			return %value.copy();

		return %value;
	}

	if (isObject(%value))
	{
		if (%value.hasMethod("__iter__"))
		{
			%iter = %value.__iter__();

			if (!isObject(%iter))
				return 0;

			return %iter;
		}

		if (%value.hasMethod("getCount") && %value.hasMethod("getObject"))
		{
			%count = %value.getCount();
			%iter = ArrayIterator(%count);

			for (%i = 0; %i < %count; %i++)
				%iter.value[%i] = %value.getObject(%i);

			return %iter;
		}

		return 0;
	}

	%len = strlen(%value);
	%iter = ArrayIterator(%len);

	for (%i = 0; %i < %len; %i++)
		%iter.value[%i] = getSubStr(%value, %i, 1);

	return %iter;
}

// ====================================
// Generic base Iterator class
// Yields nothing
function Iterator()
{
	return new ScriptObject()
	{
		class = "Iterator";
	};
}

function Iterator::onAdd(%this)
{
	tempref(%this);
}

function Iterator::copy(%this)
{
	return Iterator();
}

function Iterator::hasNext(%this)
{
	return 0;
}

function Iterator::next(%this)
{
	return "";
}

// ====================================
// FunctionIterator(Callable hasNext, Callable next, [any context])
// Proxies hasNext and next into the given callables, passing *context* if specified.
function FunctionIterator(%hasNext, %next, %context)
{
	return new ScriptObject()
	{
		class = "FunctionIterator";
		superClass = "Iterator";

		hasNext = %hasNext;
		next = %next;
		context = ref(%context);
	};
}

function FunctionIterator::onRemove(%this)
{
	unref(%this.context);
}

function FunctionIterator::copy(%this)
{
	// this is bad
	%this.context.setName("CopyTarget");
	%copy = new ScriptObject("" : CopyTarget)
	{
		___ref = "";
		___ref_sched = "";
	};
	%this.context.setName("");

	return FunctionIterator(%this.hasNext, %this.next, %copy);
}

function FunctionIterator::hasNext(%this)
{
	return call(%this.hasNext, %this.context);
}

function FunctionIterator::next(%this)
{
	if (!%this.hasNext())
		return "";

	return call(%this.next, %this.context);
}

// ====================================
// ArrayIterator(int length = 0, bool refer = false)
// Yields values from a predefined sequence
function ArrayIterator(%length, %refer)
{
	return new ScriptObject()
	{
		class = "ArrayIterator";
		superClass = "Iterator";

		length = %length | 0;
		index = 0;
		refer = %refer;
	};
}

function ArrayIterator::onRemove(%this)
{
	if (%this.refer)
	{
		for (%i = 0; %i < %this.length; %i++)
			unref(%this.value[%i]);
	}
}

function ArrayIterator::copy(%this)
{
	%iter = ArrayIterator(%this.length, %this.refer);
	%iter.index = %this.index;

	for (%i = 0; %i < %this.length; %i++)
	{
		if (%this.refer)
			ref(%this.value[%i]);

		%iter.value[%i] = %this.value[%i];
	}

	return %iter;
}

function ArrayIterator::hasNext(%this)
{
	return %this.index < %this.length;
}

function ArrayIterator::next(%this)
{
	%value = %this.value[%this.index];
	%this.index++;
	return %value;
}

// iteration with a sentinel
function _isentinel_hasNext(%ctx)
{
	if (!%ctx.cached && !%ctx.halt)
	{
		%next = tempref(Callable::call(%ctx.func));

		if (!eq(%next, %ctx.sentinel))
		{
			%ctx.cached = 1;
			%ctx.next = ref(%next);
		}
		else
			%ctx.halt = 1;
	}

	return !!%ctx.cached;
}

function _isentinel_next(%ctx)
{
	if (!%ctx.cached)
		return "";

	%next = passref(%ctx.next);
	%ctx.cached = %ctx.next = "";
	return %next;
}

// imap(seq, func)
function imap(%seq, %func)
{
	if (assert(Callable::isValid(%func), "func is not callable")) return 0;
	if (assert(%iter = iter(%seq), "seq is not iterable")) return 0;

	return FunctionIterator(_imap_hasNext, _imap_next,
		new ScriptObject()
		{
			class = Struct;
			iter = %iter;
			func = %func;
		});
}

function _imap_hasNext(%ctx) { %ctx.iter.hasNext(); }
function _imap_next(%ctx)    { return Callable::call(%ctx.func, %ctx.iter.next()); }

// enumerate(seq, start = 0)
function enumerate(%seq, %start)
{
	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return 0;

	return FunctionIterator(_enumerate_hasNext, _enumerate_next,
		new ScriptObject()
		{
			class = Struct;
			iter = %iter;
			i = %start - 1;
		});
}

function _enumerate_hasNext(%ctx) { return %ctx.iter.hasNext(); }
function _enumerate_next(%ctx)    { return Tuple::fromArgs(%ctx.i++, %ctx.iter.next()); }

// filter(seq, func)
function filter(%seq, %func)
{
	if (assert(Callable::isValid(%func), "func is not callable")) return "";
	if (assert(%iter = iter(%seq), "seq is not iterable")) return "";

	return FunctionIterator(_filter_hasNext, _filter_next,
		new ScriptObject()
		{
			class = Struct;
			iter = %iter;
			func = %func;
		});
}

function _filter_hasNext(%ctx)
{
	if (%ctx.cached)
		return 1;

	while (%ctx.iter.hasNext())
	{
		%next = %ctx.iter.next();

		if (bool(Callable::call(%ctx.func, %next)))
		{
			%ctx.cached = 1;
			%ctx.next = ref(%next);
			return 1;
		}
	}

	return 0;
}

function _filter_next(%ctx)
{
	if (!%ctx.cached)
		return "";

	%next = passref(%ctx.next);
	%ctx.cached = %ctx.next = "";
	return %next;
}

// range(start = 0, end, step = 1)
function range(%start, %end, %step)
{
	if (%step $= "")
		%step = 1;

	if (%start $= "" && %end $= "")
		return Iterator();
	
	if (%end $= "")
	{
		%end = %start;
		%start = 0;
	}

	return FunctionIterator(_range_hasNext, _range_next,
		new ScriptObject()
		{
			value = %start;
			end = %end;
			step = %step;
		});
}

function _range_hasNext(%ctx)
{
	return %ctx.step < 0 ? (%ctx.value > %ctx.end) : (%ctx.value < %ctx.end);
}

function _range_next(%ctx)
{
	%value = %ctx.value;
	%ctx.value += %ctx.step;
	return %value;
}

// reduce(seq, func, [start])
function reduce(%seq, %func, %start)
{
	if (assert(Callable::isValid(%func), "func is not callable")) return "";
	if (assert(%iter = iter(%seq), "seq is not iterable")) return "";

	if (%start $= "")
	{
		if (!%iter.hasNext())
			return "";

		%start = %iter.next();
	}

	while (%iter.hasNext())
		%start = Callable::call(%func, %start, %iter.next());

	%iter.delete();
	return %start;
}

// apply(seq, func)
function apply(%seq, %func)
{
	if (assert(Callable::isValid(%func), "func is not callable")) return "";
	if (assert(%iter = iter(%seq), "seq is not iterable")) return "";

	while (%iter.hasNext())
		%last = Callable::call(%func, %iter.next());

	%iter.delete();
	return %last;
}

// all(seq)
function all(%seq)
{
	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return "";

	while (%iter.hasNext())
	{
		if (!bool(%iter.next()))
		{
			%iter.delete();
			return 0;
		}
	}

	%iter.delete();
	return 1;
}

// any(seq)
function any(%seq)
{
	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return "";

	while (%iter.hasNext())
	{
		if (bool(%iter.next()))
		{
			%iter.delete();
			return 1;
		}
	}

	%iter.delete();
	return 0;
}

// join(seq, separator = " ")
function join(%seq, %separator)
{
	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return "";

	if (!%iter.hasNext())
		return "";

	%text = %iter.next();

	while (%iter.hasNext())
		%text = %text @ %separator @ %iter.next();

	%iter.delete();
	return %text;
}