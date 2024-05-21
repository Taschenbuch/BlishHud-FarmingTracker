namespace FarmingTracker
{
    public class Helper
    {
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
    }
}
