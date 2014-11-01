// ============================================================
// Project		: MathLib
// File			: .\NumManagement.cs
// Author		: Ipquarx
//
// Created on	: Sunday, June 8, 2014 9:55 PM
// Editor		: TorqueDev v. 1.2.5129.4848
//
// Description	: Functions to help manage the numbers
//				: that are used with the math library.
//
// License		: THIS CODE IS LICENSED UNDDER THE GNU GPL v3
//				: LICENSE, PLEASE SEE THE FOLLOWING LINK
//				: FOR THE TERMS OF THE LICENSE:
//				: http://www.gnu.org/licenses/gpl.txt
// ============================================================

//Create a number pointer from a given little-endian hexadecimal string
function makenum(%str, %x)
{
	//Make sure the string contains only hex characters
	if(stripchars(%str,"0123456789abcdefABCDEF") !$= "")
		return;

	//Get the length of the string and add 0s so that the length is divisible by 4
	%l = strlen(%str);
	if(%l & 3)
		%str = getsubstr("000", 0, 4 - (%l & 3)) @ %str;

	//Make a random pointer for the number to be assigned to.
	//The pointer is really just a random string of a-z 0-9 characters.
	//With a random length of 8, it takes over 750,000 generated numbers before the chance of two sharing a pointer becomes even 10%.
	//Just in case a collision happens, with a length of 12 that number becomes over 5 million. If you're using more than that then edit the darn script yourself :l
	if(%x $= "")
	{
		%x = randomstr(8);
		if($n[%x, "l"] !$= "")
			%x = randomstr(12);
	}
	//Put each part of the hex number into the number, but backwards.
	//Backwards becase the numbers are big-endian, this is so that adding onto the length of a number is O(1), not O(n).
	$n[%x,"l"] = mCeil(%l / 4);
	%total = $n[%x,"l"] - 1;
	for(%a = 0; %a < %l; %a+=4)
	{
		//Unfortunately eval is the only way to do this without taking up like 5 more lines :(
		eval("%b=0x000" @ getsubstr(%str, %a, 4) @ ";");
		$n[%x, (%total - %a / 4)|0] = %b | 0;
	}

	//Return the pointer of the number, so it can be passed onto math functions.
	return %x;
}

//Removes all the parts of a number with the given pointer
function deletenum(%num)
{
	deleteVariables("$n" @ %num @ "_*");
}

//Returns a random alphanumerical string of the given length
function randomstr(%l)
{
	%first = "abcdefghijklmnopqrstuvwxyz0123456789";
	for(%a = 0; %a < %l; %a++)
		%str = %str @ getsubstr(%first, getrandom(0,35), 1);
	return %str;
}

//Returns the hexadecimal representation of the number with the given pointer.
function printnum(%num)
{
	//Set up the hex characters we print out
	%s = "0123456789abcdef";
	
	//Get the length of the number
	%l = $n[%num, "l"];
	
	//Loop through all the parts of the number
	for(%a = 0; %a < %l; %a++)
	{
		//Store the current part in a temporary variable
		%v = $n[%num, %a];
		
		//Extract the hex values from the current 16 bit part
		for(%b = 0; %b < 4; %b++)
		{
			%p[%b] = %v & 15;
			%v >>= 4;
		}
		
		//Prepend all the parts in hex to the result string
		for(%b = 0; %b < 4; %b++)
			%str = getsubstr(%s, %p[%b], 1) @ %str;
	}
	
	//Return the string
	return %str;
}