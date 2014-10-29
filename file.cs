function open(%file, %mode, %endline, %silent)
{
	if (%mode $= "")
		%mode = "r";

	if (%endline $= "")
		%endline = "\n";

	%fp = new FileObject();

	switch$ (%mode)
	{
		case "r": %result = %fp.openForRead(%file);
		case "w": %result = %fp.openForWrite(%file);
		case "a": %result = %fp.openForAppend(%file);

		default:
			error("ERROR: Invalid file mode '" @ %this.mode @ "'");
			%fp.delete();
			return 0;
	}

	if (!%result)
	{
		if (!%this.silent)
			echo("Failed to open '" @ %file @ "' with mode '" @ %mode @ "'");

		%fp.delete();
		return 0;
	}

	%fp.open = 1;

	%stream = new ScriptObject()
	{
		class = "FileStream";

		fp = %fp;
		mode = %mode;

		endline = %endline;
		endline_len = strLen(%endline);

		maxline = -1;
		curline = -1;
		offset = -1;
	};

	return %stream;
}

function FileStream::onRemove(%this)
{
	if (%this.fp.open)
	{
		if (%this.pending !$= "" && (%this.mode $= "w" || %this.mode $= "a"))
			%this.fp.writeLine(%this.pending);

		%this.fp.close();
	}

	%this.fp.delete();
}

function FileStream::__iter__(%this)
{
	return new FunctionIterator(_iterfile_hasNext, _iterfile_next,
		new ScriptObject()
		{
			class = "Struct";
			file = %this;
		});
}

function _iterfile_hasNext(%ctx)
{
	return isObject(%ctx.file) && !%ctx.file.isEOF();
}

function _iterfile_next(%ctx)
{
	return %ctx.file.readLine();
}

function FileStream::_consumeLine(%this)
{
	if (!%this.fp.isEOF())
		return %this.line[%this.maxline++] = %this.fp.readLine();

	return "";
}

function FileStream::isEOF(%this)
{
	if (%this.curline < %this.maxline)
		return 0;

	if (%this.curline == %this.maxline && %this.maxline != -1 &&
		%this.offset < strLen(%this.line[%this.curline]))
		return 0;

	return %this.fp.isEOF();
}

function FileStream::getOffset(%this)
{
	%tell = %this.offset + %this.endline_len * %this.curline;

	for (%i = 0; %i < %this.curline; %i++)
		%tell += strLen(%this.line[%i]);

	return %tell;
}

function FileStream::setOffset(%this, %index)
{
	if (%index < 0)
	{
		while (!%this.fp.isEOF())
		{
			%this._consumeLine();
		}

		%this.curline = %this.maxline;
		%this.offset = strLen(%this.line[%this.curline]);

		return;
	}

	%start = 0;

	for (%i = 0; %i <= %this.maxline; %i++)
	{
		%size = strLen(%this.line[%i]) + %this.endline_len;

		if (%index >= %start && %index < %start + %size)
		{
			%this.curline = %i;
			%this.offset = %index - %start;

			return 1;
		}

		%start += %size;
	}

	// this probably isn't gonna work at all
	while (%index < %offset && !%this.fp.isEOF())
	{
		%size = strLen(%this._consumeLine()) + %this.endline_len;

		if (%index >= %start && %index < %start + %size)
		{
			%this.curline = %i;
			%this.offset = %index - %start;

			return 1;
		}

		%start += %size;
	}

	return 0;
}

// Write *data* to the file without ending the current line.
// The data will be flushed to the file upon the next `writeLine` call,
// or when the FileStream instance is deleted.
function FileStream::write(%this, %data)
{
	if (%this.mode !$= "w" && %this.mode !$= "a")
	{
		error("ERROR: FileStream is not in write mode");
		return 0;
	}

	%this.pending = %this.pending @ %data;
}

// Write *data* to the file followed by an OS-determined line terminator.
function FileStream::writeLine(%this, %line)
{
	if (%this.mode !$= "w" && %this.mode !$= "a")
	{
		error("ERROR: FileStream is not in write mode");
		return 0;
	}

	// Include any pending write() data
	if (%this.pending !$= "")
	{
		%line = %this.pending @ %line;
		%this.pending = "";
	}

	%this.fp.writeLine(%line);
}

// Read the next *size* characters.
// Will read up to EOF if *size* is not specified.
function FileStream::read(%this, %size)
{
	if (%this.mode !$= "r")
	{
		error("ERROR: FileStream is not in read mode");
		return "";
	}

	if (%this.isEOF())
		return "";

	if (%size $= "")
	{
		while (!%this.isEOF())
			%buffer = %buffer @ %this.readLine() @ %this.endline;
		
		return %buffer;
	}

	// this code probably isn't gonna work right now
	while (strLen(%buffer) < %size)
	{
		%left = %size - strLen(%buffer);
		%length = strLen(%this.line[%this.curline]) + %this.endline_len;

		if (%this.curline < %this.maxline || (%this.maxline != -1 && %this.offset < %length))
		{
			%buffer = %buffer @ getSubStr(%this.line[%this.curline] @ %this.endline, %this.offset, %left);
			%this.offset += min(%left, %length - %this.offset);

			if (%this.offset >= %length)
			{
				%this.offset = 0;
				%this.curline++;
			}
		}
		else if (%this.isEOF())
			break;
		else
			%this._consumeLine();
	}

	return %buffer;
}

// Read up until the next line terminator.
function FileStream::readLine(%this)
{
	if (%this.mode !$= "r")
	{
		error("ERROR: FileStream is not in read mode");
		return "";
	}

	// The easy case: The virtual cursor is behind the file object.
	// Splice out a line from the cached file data.
	if (%this.curline < %this.maxline)
	{
		%offset = %this.offset;
		%curline = %this.curline;
		%length = strLen(%this.line[%curline]);

		%this.offset = 0;
		%this.curline++;

		return getSubStr(%this.line[%curline], %offset, %length);
	}

	// In the case where we're at the last read line, there's two possible situations:
	// The file object hasn't read anything yet / we're at read head, which means
	// that we now need to fetch another line.
	%curline = %this.curline;
	%Length = strLen(%this.line[%curline]);

	if (%this.maxline == -1 || %this.offset >= %length)
	{
		if (%this.isEOF())
			return "";

		%this.offset = 0;
		%this.curline++;

		return %this._consumeLine();
	}

	// ...or the cursor is in the middle of the last line
	if (%this.fp.isEOF())
	{
		%offset = %this.offset;
		%this.offset = %length;
	}
	else
	{
		%this.offset = 0;
		%this.curline++;
		%this._consumeLine();
	}

	return getSubStr(%this.line[%curline], %offset, %length);
}

function FileStream::readLines(%this)
{
	if (%this.mode !$= "r")
	{
		error("ERROR: FileStream is not in read mode");
		return 0;
	}

	%array = Array();

	while (!%this.isEOF())
		%array.append(%this.readLine());

	return %array;
}