namespace AstmDriver.Commons.ASTM
{
    public static class Delimiters
    {
        public static char EscapeCharacter;

        public static char FieldDelimiter;

        public static char ComponentDelimiter;

        public static char RepeatDelimiter;

        static Delimiters()
        {
            FieldDelimiter = '|';
            RepeatDelimiter = '\\';
            ComponentDelimiter = '^';
            EscapeCharacter = '&';
        }

        public static string AddFieldDelimiters(int numberOfDelimiters)
        {
            return new string(FieldDelimiter, numberOfDelimiters);
        }
    }
}
