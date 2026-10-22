using System.Text.RegularExpressions;

namespace Garage1App
{
    public static class Validator
    {

        public static bool HasValidRegistrationNumber(string registrationNumber)
        {
            return !string.IsNullOrWhiteSpace(registrationNumber) && registrationNumber is { Length: 6 } &&
            char.IsAsciiLetter(registrationNumber[0]) &&
            char.IsAsciiLetter(registrationNumber[1]) &&
            char.IsAsciiLetter(registrationNumber[2]) &&
            char.IsAsciiDigit(registrationNumber[3]) &&
            char.IsAsciiDigit(registrationNumber[4]) &&
            char.IsAsciiDigit(registrationNumber[5]);
        }
        
        public static bool HasValidRegistrationNumber2(string registrationNumber)
        {
            return !string.IsNullOrEmpty(registrationNumber) &&
                   Regex.IsMatch(registrationNumber, @"\A[A-Za-z]{3}[0-9]{3}\z");
        }
    }
}