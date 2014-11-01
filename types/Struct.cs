function Struct::onAdd(%this)
{
	for (%i = 0; (%tag = %this.getTaggedField(%i)) !$= ""; %i++)
	{
		if (getSubStr(%tag, 0, 1) !$= "_")
			ref(getField(%tag, 1));
	}
}

function Struct::onRemove(%this)
{
	for (%i = 0; (%tag = %this.getTaggedField(%i)) !$= ""; %i++)
	{
		if ((%name = getField(%tag, 0)) !$= "_")
			%this.setAttribute(%this, %name, unref(getField(%tag, 1)));
	}
}