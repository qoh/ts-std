function Struct::onAdd(%this)
{
	%this.___attr_ref = 1;

	if (%this._tempref)
		tempref(%this);

	for (%i = 0; (%tag = %this.getTaggedField(%i)) !$= ""; %i++)
	{
		if (getSubStr(%tag, 0, 1) !$= "_")
			ref(getSubStr(%tag, strpos(%tag, "\t"), strlen(%tag)));
	}
}

function Struct::onRemove(%this)
{
	for (%i = 0; (%tag = %this.getTaggedField(%i)) !$= ""; %i++)
	{
		if ((%name = getSubStr(%tag, 0, %pos = strpos(%tag, "\t"))) !$= "_")
			%this.setAttribute(%this, %name, unref(getSubStr(%tag, %pos, strlen(%tag))));
	}
}