// ============================================================
// Project		: MathLib
// File			: .\Conditionals.cs
// Author		: Ipquarx
//
// Created on	: Monday, June 16, 2014 6:52 PM
// Editor		: TorqueDev v. 1.2.5129.4848
//
// Description	: Provides basic conditional operations
// 				: for numbers used by the library.
//
// License		: THIS CODE IS LICENSED UNDDER THE GNU GPL v3
//				: LICENSE, PLEASE SEE THE FOLLOWING LINK
//				: FOR THE TERMS OF THE LICENSE:
//				: http://www.gnu.org/licenses/gpl.txt
// ============================================================

// ------------- NOTICE ------------- //
// IF YOU HAVE BETTER NAMES FOR THESE FUNCTIONS THAT ARENT
// INSANELY LONG PLEASE LET ME KNOW
// ---------------------------------- //

//Returns true if %Num1 < %Num2, false otherwise
function aLTb(%Num1, %Num2)
{
	%L1 = $n[%Num1, "l"];
	%L2 = $n[%Num2, "l"];
	
	//We only have to do extra conditional checks
	//if the lengths are equal
	if(%L1 != %L2)
		return %L1 < %L2;
	
	//Repeat for all the legs of the numbers
	for(%a = %L1 - 1; %a >= 0; %a--)
	{
		%T1 = $n[%Num1, %a]; %T2 = $n[%Num2, %a];
		
		if(%T1 != %T2)
			return %T1 < %T2;
	}
	
	//They're equal, so a < b is false
	return false;
}

//Returns true if %Num1 == %Num2, false otherwise
function aETb(%Num1, %Num2)
{
	%L1 = $n[%Num1, "l"];
	%L2 = $n[%Num2, "l"];
	
	//If L1 != L2 then automatically A != B
	if(%L1 != %L2)
		return false;
	
	//If any of the legs aren't equal then A != B
	for(%a = 0; %a < %L1; %a++)
		if($n[%Num1, %a] != $n[%Num2, %a])
			return false;
	
	//All legs equal, lengths equal, A == B
	return true;
}

//All these functions use only aLTb or aETb to save a bit of code. Yay inequalities.
//Returns true if %Num1 > %Num2, false otherwise
function aGTb(%Num1, %Num2)
{
	//b < a == a > b
	return aLTb(%Num2, %Num1);
}

//Returns true if %Num1 <= %Num2, false otherwise
function aLTEb(%Num1, %Num2)
{
	//a <= b == !(a > b) == !(b < a)
	return !aLTb(%Num2, %Num1);
}

//Returns true if %Num1 >= %Num2, false otherwise
function aGTEb(%Num1, %Num2)
{
	//a >= b == !(a < b)
	return !aLTb(%Num1, %Num2);
}

//Returns true if %Num1 != %Num2, false otherwise
function aNETb(%Num1, %Num2)
{
	return !aETb(%Num1, %Num2);
}