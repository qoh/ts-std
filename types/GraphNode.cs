class (GraphNode);

	GraphNode.tempref = 1;

	function GraphNode::constructor(%this, %parent)
	{
		%this.linkOut = ref(Array("", 1));
		%this.linkIn = ref(Array("", 1));
	}

	function GraphNode::destructor(%this)
	{
		unref(%this.linkOut);
		unref(%this.linkIn);
	}

	function GraphNode::addEdge(%this, %other)
	{
		if (assert(isType(%other, GraphNode, 1), "other must be a GraphNode"))
			return;

		//if (%this.linkOut.contains(%other))
	}