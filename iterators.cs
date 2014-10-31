function iter(%value, %sentinel, %forceCopy)
{
	if (%sentinel !$= "" && isCallable(%value))
	{
		return FunctionIterator(_isentinel_hasNext, _isentinel_next,
			new ScriptObject()
			{
				class = Struct;
				func = %value;
				sentinel = %sentinel;
			} @ "\x08");
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
	} @ "\x08";
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
	return tempref(new ScriptObject()
	{
		class = "FunctionIterator";
		superClass = "Iterator";

		hasNext = %hasNext;
		next = %next;
		context = ref(%context);
	} @ "\x08");
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
	} @ "\x08";

	%this.context.setName("");
	return FunctionIterator(%this.hasNext, %this.next, %copy);
}

function FunctionIterator::hasNext(%this)
{
	return %this.hasNext $= "" || dynCall(%this.hasNext, %this.context);
}

function FunctionIterator::next(%this)
{
	if (!%this.hasNext())
		return "";

	return dynCall(%this.next, %this.context);
}

// ====================================
// ArrayIterator(int length = 0)
// Yields values from a predefined sequence
function ArrayIterator(%length)
{
	return tempref(new ScriptObject()
	{
		class = "ArrayIterator";
		superClass = "Iterator";

		length = %length | 0;
		index = 0;
	} @ "\x08");
}

function ArrayIterator::fromArgs(%this,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	for (%count = 19; %count > 0; %count--)
	{
		if (%a[%count - 1] !$= "")
			break;
	}

	%iter = tempref(new ScriptObject()
	{
		class = "ArrayIterator";
		superClass = "Iterator";

		length = %count;
		index = 0;
	} @ "\x08");

	for (%i = 0; %i < %count; %i++)
		%iter.value[%i] = ref(%a[%i]);

	return %iter;
}

function ArrayIterator::onRemove(%this)
{
	for (%i = 0; %i < %this.length; %i++)
		unref(%this.value[%i]);
}

function ArrayIterator::copy(%this)
{
	%iter = tempref(new ScriptObject()
	{
		class = "ArrayIterator";
		superClass = "Iterator";

		length = %this.length;
		index = %this.index;
	} @ "\x08");

	for (%i = 0; %i < %this.length; %i++)
		%iter.value[%i] = ref(%this.value[%i]);

	return %iter;
}

function ArrayIterator::hasNext(%this)
{
	return %this.index < %this.length;
}

function ArrayIterator::next(%this)
{
	%value = %this.value[%this.index];
	%this.index = (%this.index + 1) | 0;
	return %value;
}

// iteration with a sentinel
function _isentinel_hasNext(%ctx)
{
	if (!%ctx.cached && !%ctx.halt)
	{
		%next = tempref(dynCallArgs(%ctx.func));

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

// all(Iterable seq)
// Test if all values yielded by *seq* are true.
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

// any(Iterable seq)
// Test if any values yielded by *seq* are true.
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

// apply(Callable func, Iterable seq)
// Call *func* for each value from *seq*.
function apply(%func, %seq)
{
	if (assert(isCallable(%func), "func is not callable")) return;
	if (assert(%iter = iter(%seq), "seq is not iterable")) return;

	while (%iter.hasNext())
		dynCall(%func, %iter.next());

	%iter.delete();
}

// enumerate(Iterable seq, number start = 0)
// Transform the iterable *seq* into an iterator yielding (index, value) pairs.
//
// Example:
//
//     // Display ASCII character codes next to actual characters.
//     %codes = enumerate(imap(chr, range(256)));
//     apply(echo, imap(repr, %codes));
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
		} @ "\x08");
}

function _enumerate_hasNext(%ctx) { return %ctx.iter.hasNext(); }
function _enumerate_next(%ctx)    { return Tuple::fromArgs(%ctx.i++, %ctx.iter.next()); }

// filter(Callable func, Iterable seq)
// Make an iterator yielding the values from *seq* for which *func* returns a true result.
// If *func* is "", the raw values will be evaluated instead.
//
// Example:
//
//     // Iterate through non-empty elements of %array
//     filter(len, %array)
//     // Iterate through truthy elements of %array
//     filter("", %array)
function filter(%func, %seq)
{
	if (assert(%func $= "" || isCallable(%func), "func is not callable")) return "";
	if (assert(%iter = iter(%seq), "seq is not iterable")) return "";

	return FunctionIterator(_filter_hasNext, _filter_next,
		new ScriptObject()
		{
			class = Struct;
			iter = %iter;
			func = %func;
		} @ "\x08");
}

function _filter_hasNext(%ctx)
{
	if (%ctx.cached)
		return 1;

	while (%ctx.iter.hasNext())
	{
		%next = %ctx.iter.next();

		if (bool(%ctx.func $= "" ? %next : dynCall(%ctx.func, %next)))
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

// imap(Callable func, Iterable seq)
// Create an iterator that computes *func* for each value yielded by iterating over *seq*.
function imap(%func, %seq)
{
	if (assert(isCallable(%func), "func is not callable")) return 0;
	if (assert(%iter = iter(%seq), "seq is not iterable")) return 0;

	return FunctionIterator(_imap_hasNext, _imap_next,
		new ScriptObject()
		{
			class = Struct;
			iter = %iter;
			func = %func;
		} @ "\x08");
}

function _imap_hasNext(%ctx) { %ctx.iter.hasNext(); }
function _imap_next(%ctx) { return dynCall(%ctx.func, %ctx.iter.next()); }

// range(start = 0, end, step = 1)
// Make an iterator that yields values from *start* to *end*,
// in increments of *step*.
function range(%start, %end, %step)
{
	if (%step $= "")
		%step = 1;

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
		} @ "\x08");
}

function _range_hasNext(%ctx)
{
	if (%ctx.end $= "*")
		return 1;

	return %ctx.step < 0 ? (%ctx.value > %ctx.end) : (%ctx.value < %ctx.end);
}

function _range_next(%ctx)
{
	%value = %ctx.value;
	%ctx.value += %ctx.step;
	return %value;
}

// reversed(Iterable seq)
function reversed(%seq)
{
	console.error("reversed() is not implemented");
}

// reduce(Callable func, Iterable seq, [start])
function reduce(%func, %seq, %start)
{
	if (assert(isCallable(%func), "func is not callable")) return "";
	if (assert(%iter = iter(%seq), "seq is not iterable")) return "";

	if (%start $= "")
	{
		if (!%iter.hasNext())
			return "";

		%start = %iter.next();
	}

	while (%iter.hasNext())
		%start = dynCall(%func, %start, %iter.next());

	%iter.delete();
	return %start;
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

// sorted(Iterable seq, [Callable cmp])
function sorted(%seq, %cmp)
{
	console.error("sorted() is not implemented");
}

// permutations(Iterable seq, int r = len(seq))
function permutations(%seq, %r)
{
	if (%r !$= "" && %r < 1)
		return Iterator();

	if (assert(%iter = iter(%seq), "seq is not iterable"))
		return 0;

	if (!%iter.hasNext())
	{
		%iter.delete();
		return Iterator();
	}

	%struct = new ScriptObject()
	{
		class = Struct;
		n = 0;
		r = %r;
	} @ "\x08";

	while (%iter.hasNext())
	{
		%struct.value[%struct.n] = ref(%iter.next());

		if (%struct.n++ > %r)
		{
			%struct.delete();
			%iter.delete();
			return Iterator();
		}
	}

	if (%r $= "")
		%struct.r = %struct.n;

	%iter.delete();
	%struct.cycles = ref(Tuple(range(%n, %n - %r, -1)));
	return FunctionIterator(_permutations_hasNext, _permutations_next, %struct);
}

function _permutations_hasNext(%ctx)
{
	if (!%ctx.first)
		return 1;

	return 0; // TODO
}

function _permutations_next(%ctx)
{
	%result = tempref(new ScriptObject()
	{
		class = TupleInstance;
		length = %ctx.r;
	} @ "\x08");

	if (!%ctx.first)
	{
		%ctx.first = 1;

		for (%i = 0; %i < %ctx.r; %i++)
			%result.value[%i] = %ctx.value[%i];

		return %result;
	}

	// TODO
}

// zip(...Iterable seqs)
function zip(
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18, %a19)
{
	for (%count = 20; %count > 0; %count--)
	{
		if (%a[%count - 1] $= "")
			break;
	}

	%ctx = new ScriptObject()
	{
		class = Struct;
		count = %count;
	} @ "\x08";

	for (%i = 0; %i < %count; %i++)
	{
		if (assert(%iter = iter(%a[%i]), "seqs[" @ %i @ "] is not iterable"))
		{
			for (%j = 0; %j < %i; %j++)
				%ctx.iter[%j].delete();

			%ctx.delete();
			return 0;
		}

		%ctx.iter[%i] = ref(%iter);
	}

	return FunctionIterator(_zip_hasNext, _zip_next, %ctx);
}

function _zip_hasNext(%ctx)
{
	for (%i = 0; %i < %ctx.count; %i++)
	{
		if (!%ctx.iter[%i].hasNext())
			return 0;
	}

	return 1;
}

function _zip_next(%ctx)
{
	%tuple = new ScriptObject()
	{
		class = TupleInstance;
		length = %ctx.count;
	} @ "\x08";

	for (%i = 0; %i < %ctx.count; %i++)
		%tuple.value[%i] = ref(%ctx.iter[%i].next());

	return tempref(%tuple);
}

// iter::chain(Iterable seqs)
function iter::chain(%seqs)
{
	if (assert(%iter = iter(%seqs), "seqs is not iterable"))
		return 0;

	if (!%seqs.hasNext())
		return Iterator();

	return FunctionIterator(_iter_chain_hasNext, _iter_chain_next,
		new ScriptObject()
		{
			class = Struct;
			iter = %iter;
			skip = 0;
		} @ "\x08");
}

function _iter_chain_hasNext(%ctx)
{
	// this is probably wrong?
	while (!%ctx.curr || !%ctx.curr.hasNext())
	{
		if (!%ctx.iter.hasNext())
			return 0;

		%ctx.curr = ref(%ctx.iter.next());
	}
	
	return %ctx.curr && %ctx.curr.hasNext();
}

function _iter_chain_next(%ctx)
{
	return %ctx.curr.next();
}

// iter::compress(Iterable seq, Iterable which)
function iter::compress(%seq, %which)
{
	if (assert(%seq = iter(%seq), "seq is not iterable"))
		return 0;

	if (assert(%which = iter(%which), "which is not iterable"))
	{
		%seq.delete();
		return 0;
	}

	return FunctionIterator(_iter_compress_hasNext, _iter_compress_next,
		new ScriptObject()
		{
			class = Struct;
			seq = %seq;
			which = %which;
		} @ "\x08");
}

function _iter_compress_hasNext(%ctx)
{
	return %ctx.next || (%ctx.next = %ctx.seq.hasNext() && %ctx.which.hasNext() && bool(%ctx.which.next()));
}

function _iter_compress_next(%ctx)
{
	return %ctx.seq.next();
}

// iter::count(number start = 0, number step = 1)
function iter::count(%start, %step)
{
	return FunctionIterator("", _iter_count_next,
		new ScriptObject()
		{
			class = Struct;
			next = %start - %step;
			step = %step $= "" ? 1 : %step;
		} @ "\x08");
}

function _iter_count_next(%ctx)
{
	return %ctx.next += %step;
}

// iter::repeat(any value, [int times])
function iter::repeat(%value, %times)
{
	return FunctionIterator(_repeat_hasNext, _repeat_next,
		new ScriptObject()
		{
			class = Struct;
			value = %value;
			times = %times;
		} @ "\x08");
}

function _repeat_hasNext(%ctx)
{
	return %ctx.times $= "" || %ctx.index < %ctx.times;
}

function _repeat_next(%ctx)
{
	if (%ctx.times !$= "")
		%ctx.index = (%ctx.index + 1) | 0;

	return %ctx.value;
}

// iter::takeWhile(Callable func, Iterable seq)
// Make an iterator that returns values from *seq* as long as
// *func* is true for each value.
function iter::takeWhile(%func, %seq)
{
	if (assert(isCallable(%func), "func is not callable")) return 0;
	if (assert(%iter = iter(%seq), "seq is not iterable")) return 0;

	return FunctionIterator(_takeWhile_hasNext, _takeWhile_next,
		new ScriptObject()
		{
			class = Struct;
			func = %func;
			iter = %iter;
		} @ "\x08");
}

function _takeWhile_hasNext(%ctx)
{
	return !%ctx.ended && %ctx.iter.hasNext();
}

function _takeWhile_next(%ctx)
{
	%next = %ctx.iter.next();

	if (!bool(dynCall(%ctx.func, %next)))
		%ctx.ended = 1;

	return %next;
}