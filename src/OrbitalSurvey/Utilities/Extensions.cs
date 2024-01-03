namespace OrbitalSurvey.Utilities;

public static class Extensions
{
    public static string AddSpaceBeforeUppercase(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var charList = new List<char>(input.ToCharArray());

        for (var i = 1; i < charList.Count; i++)
        {
            if (char.IsUpper(charList[i]))
            {
                charList.Insert(i++, ' ');
            }
        }

        return new string(charList.ToArray());
    }
}