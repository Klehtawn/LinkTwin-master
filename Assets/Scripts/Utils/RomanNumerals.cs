using UnityEngine;
using System.Collections;
using System;

public class RomanNumerals
{
    public static string FromArabic(int number)
    {
        if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
        if (number < 1) return string.Empty;
        if (number >= 1000) return "M" + FromArabic(number - 1000);
        if (number >= 900) return "CM" + FromArabic(number - 900); //EDIT: i've typed 400 instead 900
        if (number >= 500) return "D" + FromArabic(number - 500);
        if (number >= 400) return "CD" + FromArabic(number - 400);
        if (number >= 100) return "C" + FromArabic(number - 100);
        if (number >= 90) return "XC" + FromArabic(number - 90);
        if (number >= 50) return "L" + FromArabic(number - 50);
        if (number >= 40) return "XL" + FromArabic(number - 40);
        if (number >= 10) return "X" + FromArabic(number - 10);
        if (number >= 9) return "IX" + FromArabic(number - 9);
        if (number >= 5) return "V" + FromArabic(number - 5);
        if (number >= 4) return "IV" + FromArabic(number - 4);
        if (number >= 1) return "I" + FromArabic(number - 1);
        throw new ArgumentOutOfRangeException("something bad happened");
    }
}
