namespace FarmingTracker
{
    public class Helper
    {
        public static string AddBlanksBetweenUpperCasedWords(string textWithUpperCasedWords)
        {
            var textWithBlanks = "";

            foreach (var character in textWithUpperCasedWords)
                textWithBlanks += char.IsUpper(character)
                    ? $" {character}" // blank
                    : $"{character}"; // no blank

            return textWithBlanks;
        }
    }
}
