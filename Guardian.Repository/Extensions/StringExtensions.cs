namespace Guardian.Repository.Extensions
{
    public static class StringExtensions
    {
        public static string UpperCaseFirstLetter(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            return char.ToUpper(s[0]) + s[1..];
        }
    }
}