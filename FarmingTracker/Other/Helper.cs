using System;

namespace FarmingTracker
{
    public class Helper
    {
        public static bool IsUnknownEnumValue<T>(int enumValue)
        {
            return !Enum.IsDefined(typeof(T), enumValue);
        }

        public static string ConvertEnumValueToTextWithBlanks(string textWithUpperCasedWords)
        {
            var textWithBlanks = "";

            foreach (var character in textWithUpperCasedWords)
            {
                if (character == '_')
                    continue;

                textWithBlanks += char.IsUpper(character)
                    ? $" {character}" // blank
                    : $"{character}"; // no blank
            }

            return textWithBlanks;
        }

        public static string CreateSwitchCaseNotFoundMessage<T>(T enumValue, string enumName, string usedFallbackText) where T : System.Enum
        {
            return $"Fallback: {usedFallbackText}. Because switch case missing or should not be be handled here: {enumName}.{enumValue}.";
        }
    }
}
