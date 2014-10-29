function Socket()
{
	return new ScriptObject()
	{
		class = "Socket";
		superClass = "EventEmitter";
	};
}

function Socket::onAdd(%this)
{
	Parent::onAdd(%this);

	%this.tcp = new TCPObject();
	%this.tcp._socket = %this;

	%this.mode = "disconnected";
}

function Socket::onRemove(%this)
{
	Parent::onRemove(%this);
}

function Socket::isConnected(%this)
{
	return %this.mode $= "connected";
}

function Socket::isListening(%this)
{
	return %this.mode $= "listening";
}

function Socket::isRemote(%this)
{
	return %this.mode $= "remote";
}

function Socket::connect(%this, %address)
{
	if (%this.mode $= "disconnected" || %this.mode $= "connected")
	{
		%this.tcp.connect(%address);
		%this.address = %address;
	}
}

function Socket::disconnect(%this, %address)
{
	if (%this.mode $= "connected" || %this.mode $= "remote")
	{
		%this.tcp.disconnect(%address);
		%this.tcp.onDisconnect();

		if (%this.mode !$= "remote")
			%this.address = "";
	}
}

function Socket::listen(%this, %port)
{
	if (%port $= "" || %port == 0)
	{
		if (%this.mode $= "listening")
		{
			%this.tcp.delete();

			%this.tcp = new TCPObject(_socket_tcp);
			%this.tcp._socket = %this;
		}
	}
	else if (%this.mode $= "disconnected" || %this.mode $= "connected")
	{
		if (%this.mode $= "connected")
			%this.tcp.disconnect();

		%this.mode = "listening";
		%this.port = %port;

		%this.tcp.listen(%port);
	}
}

if (!isFunction(TCPObject, onConnected)) eval("function TCPObject::onConnected(){}");
if (!isFunction(TCPObject, onDisconnect)) eval("function TCPObject::onDisconnect(){}");
if (!isFunction(TCPObject, onLine)) eval("function TCPObject::onLine(){}");
if (!isFunction(TCPObject, onConnectRequest)) eval("function TCPObject::onConnectRequest(){}");

package _socket_package
{
	function TCPObject::onConnected(%this)
	{
		if (isObject(%this._socket))
		{
			if (%this._socket.mode $= "disconnected" || %this._socket.mode $= "connected")
			{
				%this._socket.mode = "connected";
				%this._socket.sendEvent("connect", %this._socket.address);
			}
		}
		else
			Parent::onConnected(%this);
	}

	function TCPObject::onDisconnect(%this)
	{
		if (isObject(%this._socket))
		{
			if (%this._socket.mode $= "connected" || %this._socket.mode $= "remote")
			{
				%this._socket.mode = "disconnected";
				%this._socket.sendEvent("disconnect");
			}
		}
		else
			Parent::onDisconnect(%this);
	}

	function TCPObject::onLine(%this, %line)
	{
		if (isObject(%this._socket))
			%this._socket.sendEvent("line", %line);
		else
			Parent::onLine(%this, %line);
	}

	function TCPObject::onConnectRequest(%this, %address, %handle)
	{
		if (isObject(%this._socket))
		{
			if (%this._socket.mode $= "listening")
			{
				%socket = Socket();
				%socket.mode = "remote";
				%socket.address = %address;
				%socket.listener = %this._socket;

				%tcp = new TCPObject("", %handle);
				%tcp._socket = %this;

				%this._socket.sendEvent("connection", tempref(%socket));
			}
		}
		else
			Parent::onConnectRequest(%this, %address, %socket);
	}
};

activatePackage("_socket_package");