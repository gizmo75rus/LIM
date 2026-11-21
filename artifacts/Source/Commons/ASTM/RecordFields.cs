using System.Runtime.CompilerServices;

namespace AstmDriver.Commons.ASTM
{
    internal class RecordFields
    {
        private string[] fItems;

        private string @_Value;

        public string Value
        {
            get
            {
                return @_Value;
            }
            set
            {
                @_Value = value;
            }
        }

        public int Count => fItems.Length;

        public RecordFields(string lisString, char aSeporatorChar, int aNumberOfFields)
        {
            @_Value = lisString;
            fItems = lisString.Split(new char[1] { aSeporatorChar }, aNumberOfFields);
        }

        public string GetField(int indx)
        {
            if ((int)fItems.LongLength < indx)
            {
                return string.Empty;
            }
            string Result = fItems[indx - 1];
            Result = Result.Replace(new string(Delimiters.EscapeCharacter, 1) + "F" + new string(Delimiters.EscapeCharacter, 1), new string(Delimiters.FieldDelimiter, 1));
            Result = Result.Replace(new string(Delimiters.EscapeCharacter, 1) + "S" + new string(Delimiters.EscapeCharacter, 1), new string(Delimiters.ComponentDelimiter, 1));
            Result = Result.Replace(new string(Delimiters.EscapeCharacter, 1) + "R" + new string(Delimiters.EscapeCharacter, 1), new string(Delimiters.RepeatDelimiter, 1));
            return Result.Replace(new string(Delimiters.EscapeCharacter, 1) + "E" + new string(Delimiters.EscapeCharacter, 1), new string(Delimiters.EscapeCharacter, 1));
        }
    }
}
