function EventEmitter()
{
	return tempref(new ScriptObject()
	{
		class = "EventEmitter";
	} @ "\x08");
}

function EventEmitter::onAdd(%this)
{
	%this.callbacks = ref(Map());
}

function EventEmitter::onRemove(%this)
{
	unref(%this.callbacks);
}

function EventEmitter::sendEvent(%this, %event,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17)
{
	%array = %this.callbacks.get(%event);

	if (!isObject(%array))
		return;

	%args = Tuple::fromArgs(
		%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
		%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17
	);

	%rebuilt = Array("", 1);

	for (%i = 0; %i < %array.length; %i++)
	{
		%listener = %array.value[%i].value[0];

		if (!isCallable(%listener))
			continue;

		dynCallArgs(%listener, %args);

		if (%array.value[%i].value[1])
			continue;

		%rebuilt.append(Tuple::fromArgs(%listener));
	}

	%array.delete(); // unnecessary but faster
	%this.callbacks.set(%event, %rebuilt);

	return %this;
}

function EventEmitter::addListener(%this, %event, %listener, %once)
{
	if (assert(isCallable(%listener), "listener is not callable"))
		return %this;

	%array = %this.callbacks.get(%event);

	if (!isObject(%array))
		%array = %this.callbacks.set(%event, Array("", 1));
	
	%array.append(Tuple::fromArgs(%listener, %once));
	return %this;
}

function EventEmitter::removeListener(%this, %event, %listener)
{
	%array = %this.callbacks.get(%event);

	if (isObject(%array))
	{
		for (%i = 0; %i < %array.length; %i++)
		{
			if (eq(%array.value[%i].value[0], %listener))
			{
				%array.pop(%i);

				if (%array.length < 1)
					%this.callbacks.set(%event, "");

				break;
			}
		}
	}

	return %this;
}

function EventEmitter::removeAllListeners(%this, %event)
{
	if (%event $= "")
		%this.callbacks.clear();
	else
		%this.callbacks.remove(%event);

	return %this;
}

function EventEmitter::getListeners(%this, %event)
{
	%array = %this.callbacks.get(%event);

	if (isObject(%array))
	{
		%tuple = Tuple();
		%tuple.length = %array.length;

		for (%i = 0; %i < %array.length; %i++)
			%tuple.value[%i] = %array.value[%i].value[0];

		return %tuple;
	}

	return Tuple();
}

// Shorthands
function EventEmitter::on(%this, %event, %listener)
{
	return %this.addListener(%event, %listener);
}

function EventEmitter::once(%this, %event, %listener)
{
	return %this.addListener(%event, %listener, 1);
}

function EventEmitter::off(%this, %event, %listener)
{
	return %this.removeListener(%event, %listener);
}

function EventEmitter::emit(%this, %event,
	%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
	%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17)
{
	return %this.sendEvent(%event,
		%a0, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9,
		%a10, %a11, %a12, %a13, %a14, %a15, %a16, %a17);
}