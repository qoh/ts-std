// ============================================================
// Project        : MathLib
// File           : .\Basic.cs
// Author         : Ipquarx
//
// Created on     : Sunday, June 15, 2014 4:23 PM
// Editor         : TorqueDev v. 1.2.5129.4848
//
// Description    : Provides the basic math functions, +, -, *, /, %,
//                : optimized for speed with no external function calls.
//
// License        : THIS CODE IS LICENSED UNDDER THE GNU GPL v3
//                : LICENSE, PLEASE SEE THE FOLLOWING LINK
//                : FOR THE TERMS OF THE LICENSE:
//                : http://www.gnu.org/licenses/gpl.txt
// ============================================================

function BigInt_Add(%Num1, %Num2)
{
    //Length in legs
    %L1 = $n[%Num1, "l"];
    %L2 = $n[%Num2, "l"];

    //Don't use getMin because function calling is expensive
    %min = (%L1 < %L2 ? %L1 : %L2) | 0;
    %carry = false;

    //Calculate for the total length of the smallest number
    for(%a = 0; %a < %min; %a++)
    {
        //Make temporary result, use | 0 to speed up result and force result as int
        %N = ($n[%Num1, %a] + $n[%Num2, %a]) | 0;

        //Check if we need to carry, and set the carry variable to the result
        if((%carry = (%N & -65536)) != 0)
        {
            //Do carrying
            $n[%Num1, %a + 1] = $n[%Num1, %a + 1] + 1 | 0;
            %N &= 65535;
        }

        $n[%Num1, %a] = %N;
    }

    //If they're equal then we don't need to do anything else
    if(%l1 == %L2)
    {
        $n[%Num1, "l"] = %a + %carry;
        return;
    }

    //If %min is equal to l1 then we need to actually do a bit more computing.
    if(%min == %l2)
    {
        //Perform more carrying as long as we need to
        while(($n[%Num1, %a] & -65536) != 0)
        {
            $n[%Num1, %a] &= 65535;
            %a++;
            $n[%Num1, %a] = $n[%Num1, %a] + 1 | 0;
        }
    }
    else
    {
        if(%Carry)
        {
            //carry while setting num1 to the 2nd number
            $n[%Num1, %a] = $n[%Num2, %a] + 1 | 0;

            while(($n[%Num1, %a] & -65536) != 0)
            {
                $n[%Num1, %a] &= 65535;
                %a++;
                $n[%Num1, %a] = $n[%Num2, %a] + 1 | 0;
            }
        }

        //Set the remaining legs of num1 to num2
        while(%a < %L2)
        {
            $n[%Num1, %a] = $n[%Num2, %a];
            %a++;
        }
    }

    $n[%Num1, "l"] = %a + 1;
}

//Subtracts %Num2 from %Num1, placing the result in %Num1
function BigInt_Subtract(%Num1, %Num2)
{
    %L1 = $n[%Num1, "l"] | 0;
    %L2 = $n[%Num2, "l"] | 0;

    //Ensure that %Num1 > %Num2 as this math library doesn't support negatives
    //This math library doesn't support negatives.
    if(%L1 < %L2)
        return "ERROR";

    if(%L1 == %L2)
    {
        //Check if Num1 > Num2 by comparing the content of the integers, staring from the most significant one.
        for(%a = %L1 - 1; %a >= 0; %a--)
        {
            %T1 = $n[%Num1, %a]; %T2 = $n[%Num2, %a];

            if(%T1 < %T2)
                return "ERROR";

            if(%T1 > %T2)
                break;
        }
    }

    //Only loop up to L2 because L2 <= L1, since Num2 <= Num1
    for(%a = 0; %a < %L2; %a++)
    {
        //Make temporary result, use | 0 to speed up result and force result as int
        //Extra |0's necessary because negatives don't like playing nice :c
        %N = ( ($n[%Num1, %a] | 0) - ( $n[%Num2, %a] | 0) ) | 0;

        //Check if we need to carry, and set the carry variable to the result
        if((%carry = (%N & -65536)) != 0)
        {
            //Do carrying
            $n[%Num1, %a + 1] = $n[%Num1, %a + 1] - 1 | 0;
            %N = (%N|0) + 65536 | 0;
        }

        $n[%Num1, %a] = %N;
    }

    //This part is a bit easier for subtraction since %L2 <= %L1 100% of the time
    if(%carry)
    {
        //Carry for as long as we need to
        while(($n[%Num1, %a] & -65536) != 0)
        {
            $n[%Num1, %a] = $n[%Num1, %a] + 65536 | 0;
            %a++;
            $n[%Num1, %a] = $n[%Num1, %a] - 1 | 0;
        }
    }

    for(%a = %L1 - 1; %a > 0; %a--)
    {
        if($n[%Num1, %a] != 0)
            break;
    }

    $n[%Num1, "l"] = %a + 1;
}

//Both %Num1 & %Num2 are pointers to numbers stored in global variables.
//Both arguments can be the same without screwing anything up.
//Places result in %Num1; %Num2 stays the same. If %Num1 & %Num2 are the same then the result is either/or.
//This function will handle numbers up to just under one megabit in size, or in other words, 10^315652.
function BigInt_Multiply(%Num1, %Num2)
{
    //Length in legs
    %L1 = $n[%Num1, "l"]|0;
    %L2 = $n[%Num2, "l"]|0;

    //Store all the pieces of num1 in local variables
    for(%a = 0; %a < %L1; %a++)
        %n1[%a] = $n[%Num1, %a] | 0;

    //%TC is the variable that contains the total length of the new number
    %TC = 0;

    //This algorithm is O(n^2). Karatsuba multiplication for really large numbers might be implemented later.
    for(%a = 0; %a < %L2; %a++)
    {
        //Store our current part of %n2 in a local variable
        %n2 = $n[%Num2, %a] | 0;

        for(%b = 0; %b < %L1; %b++)
        {
            //Calculate a temporary result, use | 0 to force result as int
            %tmp = %n2 * %n1[%b] | 0;
            %ind = %a + %b;

            //Check if we need to carry
            if((%tmp & -65536) != 0)
            {
                //If so, do carrying
                %tmps[%ind] = %tmps[%ind] + (%tmp & 65535) | 0;
                %TC = %ind++ > %TC ? %ind : %TC;
                %tmps[%ind] = %tmps[%ind] + (%tmp >> 16) | 0;
            }
            else
            {
                //If not, just add the temporary result.
                %t = %tmps[%ind];
                %tmps[%ind] = %tmps[%ind] + %tmp | 0;
            }
        }
    }

    //I'm not sure if this will ever carry more than once. If anyone can confirm or disprove it please do.
    %carried = true;
    while(%carried)
    {
        %carried = false;

        for(%a = %TC - 1; %a > 0; %a--)
        {
            %tt = %tmps[%a] = ((%t=%tmps[%a]) & 65535) | 0;
            if(%t != %tt)
            {
                %aa = %a + 1;
                if(%aa > %TC)
                    %TC = %aa;
                %tmps[%aa] = (%tmps[%aa] + (%t >> 16)) | 0;
                %carried = true;
            }
        }
    }

    if((%tmps[%TC] | 0) != 0)
        %TC++;

    for(%a = 0; %a < %TC; %a++)
        $n[%Num1, %a] = %tmps[%a];

    $n[%Num1, "l"] = %TC;
}

//Divides %Num1 by %Num2, placing the quotient in %Num1
function BigInt_Quotient(%Num1, %Num2)
{
    //__region Prep
    %L1 = $n[%Num1, "l"];
    %L2 = $n[%Num2, "l"];
    
    //This would only happen due to some sort of bug
    if(%L1 == 0 || %L2 == 0)
    {
        error("ERROR: ZERO LENGTH NUMBER OCCURED");
        return "ERROR" SPC %L1 SPC %L2;
    }
    //__end

    //__region Checks
    if(%L2 == 1)
    {
        %c = $n[%Num2, 0];

        //No dividing by 0!
        if(%c == 0)
            return "ERROR";

        //Dividing by 1, return becase %Num1 wouldn't be modified anyway.
        if(%c == 1)
            return "";
    }
    
    if(%L2 > 1)
    {
        //Round to the nearest 23 bits
        %P2 = ($n[%Num2, %L2 - 1] << 8) | ($n[%Num2, %L2 - 2] >> 8);
        %P2 = ((%P2 >> 1) + (%P2 & 1)) | 0;
        %E2 = %L2 - 2;
    }
    else
    {
        //No rounding needed
        %P2 = $n[%Num2, 0] << 7;
        %E2 = -1;
    }
    
    %c = -1;
    //__end
    
    //__region More prep
    
    //Put all the parts of %Num2 into a local variable array
    //so we don't have to access the global variables more than once
    for(%a = 0; %a < %L2; %a++)
        %N2[%a] = $n[%Num2, %a] | 0;
    
    //__end
    
    //We break out of the while loop when we're done, no worries
    while(true)
    {
        //__region Estimation
        
        //Get %P1
        if(%L1 > 1)
        {
            %P1 = ($n[%Num1, %L1 - 1] << 8) | ($n[%Num1, %L1 - 2] >> 8);
            %P1 = ((%P1 >> 1) + (%P1 & 1)) | 0;
            %E1 = %L1 - 2;
        }
        else
        {
            //%L1 = %L2 = 1 because %L2 <= %L1 and %L1 = 1 and %L2 != 0
            %P1 = $n[%Num1, 0];
            %E1 = %E2;
        }

        //Estimate %Num1 / %Num2 as %Ratio * 65536^%Exp
		if(%ratio == 0)
		{
			%Ratio = %P1 / %P2;
			%Exp = %E1 - %E2;
        }
		
        if(%Ratio - 1 < 0)
        {
            %Ratio *= 65536;
            %Exp--;
        }

        //Use only the integer part of %Ratio
        %Ratio = %Ratio | 0;
        
        //__end

        //__region Multiply
        
        %a = 0;
        while(%a < %L2)
        {
            //Calculate a temporary result, use | 0 to force result as int
            %tmp1 = (%Ratio|0) * (%N2[%a]|0) | 0;

            //Check if we need to carry
            if((%tmp1 & -65536) != 0)
            {
                //If so, do carrying
                %tmps[%a] = %tmps[%a] + (%tmp1 & 65535) | 0;
                %a++;
                %tmps[%a] = %tmps[%a] + (%tmp1 >> 16) | 0;
            }
            else
            {
                //If not, just add the temporary result.
                %tmps[%a] = %tmps[%a] + %tmp1 | 0;
                %a++;
            }
        }
        
        if((%tmps[%L2] | 0) != 0)
            %TC = %L2 + 1;
        else
            %TC = %L2;

        //__region Carry
        
        for(%a = %TC - 1; %a > 0; %a--)
        {
            %tt = %tmps[%a] = ((%t=%tmps[%a]) & 65535) | 0;
            if(%t != %tt)
            {
                %aa = %a + 1;
                if(%aa > %TC)
                    %TC = %aa;
                %tmps[%aa] = (%tmps[%aa] + (%t >> 16)) | 0;
            }
        }
        
		//Sometimes the estimate is 1 too high, so we correct for that here if it happens
        if(%tmps[%TC - 2] > $n[%Num1, %L1 - 2] && %tmps[%TC - 1] == $n[%Num1, %L1 - 1] || %tmps[%TC - 1] > $n[%Num1, %L1 - 1])
		{
			%ratio--;
			for(%a = 0; %a < %TC; %a++)
				%tmps[%a] = 0;
			continue;
		}
        
        //__end
        
        //__end
        
        //__region Subtract
        
        for(%a = %Exp; %a < %L1; %a++)
        {
            %N = ( ($n[%Num1, %a] | 0) - ( %tmps[%a-%Exp] | 0) ) | 0;
            
            //Set the array to 0 once we're done with each part (to avoid looping again)
            %tmps[%a-%Exp] = 0;
        
            if((%Carry=(%N & -65536)) != 0)
            {
                //Do carrying
                $n[%Num1, %a + 1] = $n[%Num1, %a + 1] - 1 | 0;
                %N = (%N|0) + 65536 | 0;
            }
    
            $n[%Num1, %a] = %N;
        }
        
        if(%carry)
        {
            //Carry for as long as we need to
            while(($n[%Num1, %a] & -65536) != 0)
            {
                $n[%Num1, %a] = $n[%Num1, %a] + 65536 | 0;
                %a++;
                $n[%Num1, %a] = $n[%Num1, %a] - 1 | 0;
            }
        }
    
        for(%a = %L1 - 1; %a > 0; %a--)
        {
            if($n[%Num1, %a] != 0)
                break;
        }
    
        $n[%Num1, "l"] = %a + 1;
        
        //__end

        //__region Preps
        
        //Update %L1
        %L1 = $n[%Num1, "l"];

        //Put the ratio into an array & reset ratio
        %N[%c++] = %Ratio;
		%ratio = 0;
        
        //__end
        
        //__region compare
        if(%L1 > %L2)
            continue;
        
        if(%L1 == %L2)
        {
            for(%a = %L1 - 1; %a >= 0; %a--)
                if($n[%Num2, %a] > $n[%Num1, %a])
                    break;
            
            if(%a != -1)
                break;
        }
        else
            break;
        
        //__end
    }
    
    //Put the quotient into %Num1
    $n[%Num1, "l"] = %c + 1;

    %b = -1;
    for(%a = %c; %a >= 0; %a--)
        $n[%Num1, %b++] = %N[%a];

    //Return
    return "";
}

//Returns %Num1 mod %Num2
function BigInt_Modulus(%Num1, %Num2)
{
    %L1 = $n[%Num1, "l"];
    %L2 = $n[%Num2, "l"];
    
    if(%L1 == 0 || %L2 == 0)
    {
        error("ERROR: ZERO LENGTH NUMBER OCCURED");
        return "ERROR" SPC %L1 SPC %L2;
    }

    if(%L2 == 1)
    {
        %c = $n[%Num2, 0];
        
        if(%c == 0)
            return "ERROR";

        if(%c == 1)
            return "";
    }
    
    if(%L2 > 1)
    {
        %P2 = ($n[%Num2, %L2 - 1] << 8) | ($n[%Num2, %L2 - 2] >> 8);
        %P2 = ((%P2 >> 1) + (%P2 & 1)) | 0;
        %E2 = %L2 - 2;
    }
    else
    {
        %P2 = $n[%Num2, 0] << 7;
        %E2 = -1;
    }
    
    for(%a = 0; %a < %L2; %a++)
        %N2[%a] = $n[%Num2, %a] | 0;
	
	if(%L1 < %L2)
		return;
	
	if(%L2 == %L1)
		for(%a = %L1 - 1; %a >= 0; %a--)
		{
			%T1 = $n[%Num1, %a]; %T2 = %N2[%a];
			
			if(%T1 < %T2)
				return;
			if(%T2 > %T1)
				break;
		}
    
	echo(printnum(%Num1) SPC printnum(%Num2));
	
    while(%c++ < 100)
    {
        if(%L1 > 1)
        {
            %P1 = ($n[%Num1, %L1 - 1] << 8) | ($n[%Num1, %L1 - 2] >> 8);
            %P1 = ((%P1 >> 1) + (%P1 & 1)) | 0;
            %E1 = %L1 - 2;
        }
        else
        {
            %P1 = $n[%Num1, 0];
            %E1 = %E2;
        }
	    
		if(%Ratio == 0)
		{
			//echo(%P1 SPC %P2);
			%Ratio = %P1 / %P2;
			%Exp = %E1 - %E2;
			
			if(%Check = %P1 < %P2)
			{
				%Ratio *= 65536;
				%Exp--;
			}
			
			%Ratio = %Ratio | 0;
		}
		//echo(%ratio);
		//echo(%exp);
        
		//echo("multiplying");
        %a = 0;
        while(%a < %L2)
        {
            %tmp1 = (%Ratio|0) * (%N2[%a]|0) | 0;
            
            if((%tmp1 & -65536) != 0)
            {
                %tmps[%a] = %tmps[%a] + (%tmp1 & 65535) | 0;
                %a++;
                %tmps[%a] = %tmps[%a] + (%tmp1 >> 16) | 0;
            }
            else
            {
                %tmps[%a] = %tmps[%a] + %tmp1 | 0;
                %a++;
            }
        }
		
        if((%tmps[%L2] | 0) != 0)
		{
			echo("Branch1");
            %TC = %L2 + 1;
		}
        else
		{
			echo("Branch2");
            %TC = %L2;
		}
        
		//echo("carrying");
        for(%a = %TC - 1; %a > 0; %a--)
        {
            %tt = %tmps[%a] = ((%t=%tmps[%a]) & 65535) | 0;
            if(%t != %tt)
            {
                %aa = %a + 1;
                if(%aa > %TC)
                    %TC = %aa;
                %tmps[%aa] = (%tmps[%aa] + (%t >> 16)) | 0;
            }
        }
        
		//echo("TC: "@%TC);
		//echo(%tmps[%TC - 2 + %Check] SPC $n[%Num1, %L1 - 2] SPC %tmps[%TC - 1 + %Check] SPC $n[%Num1, %L1 - 1]);
        if(%tmps[%TC - 2 + %Check] > $n[%Num1, %L1 - 2] && %tmps[%TC - 1 + %Check] == $n[%Num1, %L1 - 1] || %tmps[%TC - 1 + %Check] > $n[%Num1, %L1 - 1])
		{
			//echo("ESTIMATION FAIL");
			%ratio--;
			for(%a = 0; %a < %TC; %a++)
				%tmps[%a] = 0;
			continue;
		}
		
        //echo("SUBTRACTING");
        for(%a = %Exp; %a < %L1; %a++)
        {
			//echo(%a SPC %L1-1 SPC $n[%Num1, %a] SPC %tmps[%a-%Exp]);
            %N = ( ($n[%Num1, %a] | 0) - ( %tmps[%a-%Exp] | 0) ) | 0;
            %tmps[%a-%Exp] = 0;
        
            if((%Carry=(%N & -65536)) != 0)
            {
                $n[%Num1, %a + 1] = $n[%Num1, %a + 1] - 1 | 0;
                %N = (%N|0) + 65536 | 0;
            }
    
            $n[%Num1, %a] = %N;
        }
        
        if(%carry)
        {
			//echo("carrying");
            while(($n[%Num1, %a] & -65536) != 0 && %a < %L1+1)
            {
                $n[%Num1, %a] = $n[%Num1, %a] + 65536 | 0;
                %a++;
                $n[%Num1, %a] = $n[%Num1, %a] - 1 | 0;
            }
        }
    
        for(%a = %L1 - 1; %a > 0; %a--)
            if($n[%Num1, %a] != 0)
                break;
    
        $n[%Num1, "l"] = %a + 1;
        %L1 = $n[%Num1, "l"];
		%Ratio = 0;
		
		//echo(%l1 SPC %l2);
        if(%L1 > %L2)
            continue;
        
        if(%L1 == %L2)
        {
            for(%a = %L1 - 1; %a >= 0; %a--)
                if($n[%Num2, %a] > $n[%Num1, %a])
                    break;
            
            if(%a != -1)
                break;
        }
        else
            break;
    }
	
	if(%c == 1000)
	{
		echo("FAILLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL");
	}
}