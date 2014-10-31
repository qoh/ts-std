if (!isObject(console))
{
	new ScriptObject(console)
	{
		class = "Module";

		groupText = "";
		groupDepth = 0;
	};
}

function console::write(%this, %text, %label)
{
	%indent = repeat("  ", %this.groupDepth);

	%width = 88 - strlen(%indent);
	%width_first = %width;

	if (%label !$= "")
	{
		%label_len = strlen(%label) + 1;

		if (%label_len >= %width - 1)
		{
			%label = getSubStr(%label, 0, %width - 2);
			%label_len = %width - 1;
		}

		%width_first -= %label_len;
	}

	%len = strlen(%text);

	if (%len >= %width_first)
	{
		// line wrapping
		%text = %ident @ %text @ %label; // to be implemented
	}
	else if (%label !$= "")
	{
		%text = %ident @ %text @ repeat(" ", 2 + (%width_first + strlen(%indent) - %len)) @ %label;
	}

	if (%this.groupDepth)
		%this.groupText = %this.groupText @ %text @ "\n";
	else
		echo(%text);
}

function console::children(%this, %obj)
{
	%count = %obj.getCount();
	%taglen = 9;

	console.group(repr(%obj));

	for (%i = 0; %i < %count; %i++)
	{
		%child = %obj.getObject(%i);

		%tag = %child.___ref $= "" ? "\c1no ref" : %child.___ref SPC "refs";
		%tag = getSubStr(%tag, 0, %taglen);

		console.log(%tag @ repeat(" ", (%taglen + 1) - strlen(stripColorChars(%tag))) @ "\c0" @ repr(%child));
	}

	console.endGroup();
}

function console::repr(%this, %value)
{
	%this.log(repr(%value));
}

function console::log(%this,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	%this.write(
		formatn(%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
				%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
	);
}

function console::debug(%this,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	%this.write(
		"\c9" @ formatn(%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
					%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18),
		"Debug"
	);
}

function console::info(%this,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	%this.write(
		"\c1" @ formatn(%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
					%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18),
		"Info"
	);
}

function console::warn(%this,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	%this.write(
		"\c5" @ formatn(%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
					%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18),
		"Warning"
	);
}

function console::error(%this,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18)
{
	%this.write(
		"\c2" @ formatn(%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
					%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17, %a18),
		"Error"
	);
}

function console::group(%this, %title)
{
	if (%title !$= "")
		%this.groupText = %this.groupText @ repeat("  ", %this.groupDepth) @ %title @ "\n";

	%this.groupText = %this.groupText @ repeat("  ", %this.groupDepth) @ "{\n";
	%this.groupDepth++;
}

function console::endGroup(%this)
{
	if (%this.groupDepth)
	{
		%this.groupDepth--;
		%this.groupText = %this.groupText @ repeat("  ", %this.groupDepth) @ "}";

		if (%this.groupDepth == 0)
		{
			echo(%this.groupText);
			%this.groupText = "";
		}
		else
			%this.groupText = %this.groupText @ "\n";
	}
	else
	{
		%this.error("console::groupEnd - not in a group!");
		%this.trace();
	}
}