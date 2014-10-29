// children needs to be a Set instead
class (TreeNode);

	TreeNode.tempref = 1;

	function TreeNode::constructor(%this, %parent)
	{
		%this.children = ref(Array("", 1));
		%this.parent = 0;

		if (%parent !$= "")
			%this._(setParent, %parent);
	}

	function TreeNode::destructor(%this)
	{
		unref(%this.children);
		unref(%this.parent);
	}

	function TreeNode::__repr__(%this)
	{
		return "TreeNode(" @ %this.getID() @ " <- " @ %this.parent @ ")";
	}

	function TreeNode::__iter__(%this)
	{
		return iter(%this.children);
	}

	function TreeNode::isDistantParent(%this, %node)
	{
		%target = %this;

		while (isObject(%target))
		{
			if (eq(%target.parent, %node))
				return 1;

			%target = %target.parent;
		}

		return 0;
	}

	function TreeNode::getParent(%this)
	{
		return %this.parent;
	}

	function TreeNode::setParent(%this, %parent)
	{
		if (assert(isType(%parent, TreeNode, 1), "parent must be a TreeNode"))
			return;

		if (%this.parent)
			unref(%this.parent);

		%this.parent = ref(%parent);
	}

	function TreeNode::isChild(%this, %node)
	{
		return %this.children.contains(%node);
	}

	function TreeNode::isDistantChild(%this, %node)
	{
		if (%this._(isChild, %node))
			return 1;

		for (%i = 0; %i < %this.children.length; %i++)
		{
			if (%this.children.value[%i]._(isDistantChild, %node))
				return 1;
		}

		return 0;
	}

	function TreeNode::addChild(%this, %node)
	{
		if (assert(isType(%node, TreeNode, 1), "node must be a TreeNode"))
			return;

		if (%this._(isChild, %node))
			return;

		%this.children.append(%node);
	}

	function TreeNode::removeChild(%this, %node)
	{
		%this.children.remove(%node);
	}

	function TreeNode::recursiveChildren(%this, %iter)
	{
		if (!isObject(%iter))
			%iter = ArrayIterator("", 1);

		for (%i = 0; %i < %this.children.length; %i++)
		{
			%iter.value[%iter.length] = ref(%this.children.value[%i]);
			%iter.length++;

			%this.children.value._(recursiveChildren(%iter));
		}

		return %iter;
	}