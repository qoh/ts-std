//FIXME
// regex "a.+a" does not match ababa
// not greedy enough; stops at second a

function regex::compile(%re)
{
	%post = regex::_re2post(%re);

	if (%post $= "")
		return 0;

	%nfa = regex::_post2nfa(%post);

	if (%nfa == 0)
		return 0;

	return RegexProgram(%nfa);
}

// this is horribly ugly
// needs refactoring
function regex::_re2post(%re)
{
	%concat = "\x01";

	%len = strlen(%re);
	%depth = 0;

	%nalt = 0;
	%natom = 0;

	%nalt[0] = 0;
	%natom[0] = 0;

	for (%i = 0; %i < %len; %i++)
	{
		%c = getSubStr(%re, %i, 1);

		switch$ (%c)
		{
		case "(":
			if (%natom > 1)
			{
				%natom--;
				%out = %out @ %concat;
			}
			if (%nalt[%depth] >= %nalt[100])
				return "";
			%depth++;
			%nalt[%depth] = %nalt;
			%natom[%depth] = %natom;
			%nalt = 0;
			%natom = 0;

		case "|":
			if (%natom == 0)
				return "";
			while (%natom-- > 0)
				%out = %out @ %concat;
			%nalt++;

		case ")":
			if (%nalt[%depth] == %nalt[0])
				return "";
			if (%natom == 0)
				return "";
			while (%natom-- > 0)
				%out = %out @ %concat;
			for (0; %nalt > 0; %nalt--)
				%out = %out @ "|";
			%nalt[%depth] = "";
			%natom[%depth] = "";
			%depth--;
			%nalt = %nalt[%depth];
			%natom = %natom[%depth];
			%natom++;

		case "*" or "+" or "?":
			if (%natom == 0)
				return "";
			%out = %out @ %c;

		default:
			if (%natom > 1)
			{
				%natom--;
				%out = %out @ %concat;
			}
			%out = %out @ %c;
			%natom++;
		}
	}

	if (%depth != 0)
		return "";
	while (%natom-- > 0)
		%out = %out @ %concat;
	for (0; %nalt > 0; %nalt--)
		%out = %out @ "|";
	return %out;
}

// this is ugly too
function regex::_post2nfa(%post)
{
	%matchstate = RegexState(257);

	%len = strlen(%post);
	%stack = -1;

	for (%i = 0; %i < %len; %i++)
	{
		%c = getSubStr(%post, %i, 1);

		switch$ (%c)
		{
		case "\x01": // concatenate
			%e2 = unref(%stack[%stack]); %stack[%stack] = ""; %stack--;
			%e1 = unref(%stack[%stack]); %stack[%stack] = ""; %stack--;
			%e1.out.patch(%e2.start);
			%stack[%stack++] = ref(RegexFrag(%e1.start, %e2.out));

		case "|": // alternate
			%e2 = unref(%stack[%stack]); %stack[%stack] = ""; %stack--;
			%e1 = unref(%stack[%stack]); %stack[%stack] = ""; %stack--;
			%s = RegexState(256, %e1.start, %e2.start);
			%stack[%stack++] = ref(RegexFrag(%s, %e1.out.append(%e2.out)));

		case "?": // zero or one
			%e = unref(%stack[%stack]); %stack[%stack] = ""; %stack--;
			%s = RegexState(256, %e.start, 0);
			%stack[%stack++] = ref(RegexFrag(%s, %e.out.append(tempref(RegexOuts(%s, 1)))));

		case "*": // zero or more
			%e = unref(%stack[%stack]); %stack[%stack] = ""; %stack--;
			%s = RegexState(256, %e.start, 0);
			%e.out.patch(%s);
			%stack[%stack++] = ref(RegexFrag(%s, RegexOuts(%s, 1)));

		case "+": // one or more
			%e = unref(%stack[%stack]); %stack[%stack] = ""; %stack--;
			%s = RegexState(256, %e.start, 0);
			%e.out.patch(%s);
			%stack[%stack++] = ref(RegexFrag(%e.start, RegexOuts(%s, 1)));

		default:
			%s = RegexState(%c $= "." ? 0 : ord(%c), 0, 0);
			%stack[%stack++] = ref(RegexFrag(%s, RegexOuts(%s, 0)));
		}
	}

	%e = unref(%stack[%stack]); %stack[%stack] = ""; %stack--;

	if (%stack != -1)
	{
		%matchstate.delete();
		return 0;
	}

	%e.out.patch(%matchstate);
	return %e.start;
}

// RegexState impl: one state in a NFA state machine
function RegexState(%match, %out0, %out1)
{
	return new ScriptObject()
	{
		class = "RegexState";
		lastlist = 0;
		match = %match;
		out0 = ref(%out0);
		out1 = ref(%out1);
	};
}

function RegexState::onRemove(%this)
{
	unref(%this.out0);
	unref(%this.out1);
}

// RegexFrag impl: a single part of a NFA state machine
function RegexFrag(%start, %out)
{
	return new ScriptObject()
	{
		class = "RegexFrag";
		start = ref(%start);
		out = ref(%out);
	};
}

function RegexFrag::onRemove(%this)
{
	unref(%this.start);
	unref(%this.out);
}

// RegexOuts impl: the list of dangling arrows on a fragment
function RegexOuts(%state, %index)
{
	return new ScriptObject()
	{
		class = "RegexOuts";
		length = 1;

		state0 = ref(%state);
		index0 = %index;
	};
}

function RegexOuts::onRemove(%this)
{
	for (%i = 0; %i < %this.length; %i++)
		unref(%this.state[%i]);
}

function RegexOuts::append(%this, %outs)
{
	%result = new ScriptObject()
	{
		class = "RegexOuts";
		length = 0;
	};

	for (%i = 0; %i < %this.length; %i++)
	{
		%result.state[%result.length] = ref(%this.state[%i]);
		%result.index[%result.length] = %this.index[%i];
		%result.length++;
	}

	for (%i = 0; %i < %outs.length; %i++)
	{
		%result.state[%result.length] = ref(%outs.state[%i]);
		%result.index[%result.length] = %outs.index[%i];
		%result.length++;
	}

	return %result;
}

function RegexOuts::patch(%this, %state)
{
	for (%i = 0; %i < %this.length; %i++)
		%this.state[%i].out[%this.index[%i]] = %state;
}

// RegexStates impl: keeps track of current active states
function RegexStates(%start)
{
	%states = new ScriptObject()
	{
		class = "RegexStates";
		length = 0;
	};

	if (isObject(%start))
		%states.add(%start);

	return %states;
}

function RegexStates::onRemove(%this)
{
	for (%i = 0; %i < %this.length; %i++)
		unref(%this.state[%i]);
}

function RegexStates::reset(%this)
{
	for (%i = 0; %i < %this.length; %i++)
	{
		unref(%this.state[%i]);
		%this.state[%i] = "";
	}

	%this.length = 0;
}

function RegexStates::add(%this, %state)
{
	if (!isObject(%state) || %state.lastlist == %this)
		return;

	%state.lastlist = %this;

	// optimization: at branch state
	// no need to push state as it only has free transitions
	if (%state.match == 256)
	{
		// follow unlabeled arrows
		%this.add(%state.out0);
		%this.add(%state.out1);
		return;
	}

	%this.state[%this.length] = ref(%state);
	%this.length++;
}

function RegexStates::isMatch(%this)
{
	for (%i = 0; %i < %this.length; %i++)
	{
		if (%this.state[%i].match == 257)
			return 1;
	}

	return 0;
}

function RegexStates::step(%this, %chr, %outlist)
{
	%ord = ord(%chr);
	%outlist.reset();

	for (%i = 0; %i < %this.length; %i++)
	{
		%s = %this.state[%i];

		if (%s.match < 256 && (%s.match == %ord || %s.match == 0))
			%outlist.add(%s.out0);
	}
}

// RegexProgram impl: a wrapper on top of a single state for matching
function RegexProgram(%state)
{
	return new ScriptObject()
	{
		class = "RegexProgram";
		state = ref(%state);
	};
}

function RegexProgram::onRemove(%this)
{
	unref(%this.state);
}

function RegexProgram::match(%this, %text)
{
	%clist = RegexStates(%this.state);
	%nlist = RegexStates();

	%len = strlen(%text);

	for (%i = 0; %i < %len; %i++)
	{
		%clist.step(getSubStr(%text, %i, 1), %nlist);
		%t = %clist; %clist = %nlist; %nlist = %t;
	}

	%match = %clist.isMatch();

	%clist.delete();
	%nlist.delete();

	return %match;
}

function RegexProgram::inspect(%this)
{
	%context = new ScriptObject();
	%context.length = 0;

	%this.inspectInner(%this.state, %context);

	for (%i = 0; %i < %context.length; %i++)
	{
		%state = %context.state[%i];
		%head = "state " @ %i @ ": ";

		%out0 = "state " @ %context.index[%state.out0];
		%out1 = "state " @ %context.index[%state.out1];

		if (%state.match == 257)
			echo(%head @ "match");
		else if (%state.match == 256)
			echo(%head @ "* => " @ %out0 @ ", * => " @ %out1);
		else
			echo(%head @ (%state.match == 0 ? "*" : chr(%state.match)) @ " => " @ %out0);
	}
}

function RegexProgram::inspectInner(%this, %state, %context)
{
	%context.saw[%state] = 1;
	%context.state[%context.length] = %state;
	%context.index[%state] = %context.length;
	%context.length++;

	if (isObject(%state.out0) && !%context.saw[%state.out0])
		%this.inspectInner(%state.out0, %context);

	if (isObject(%state.out1) && !%context.saw[%state.out1])
		%this.inspectInner(%state.out1, %context);
}