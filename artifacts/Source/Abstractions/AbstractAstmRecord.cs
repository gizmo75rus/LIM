using AstmDriver.Commons.ASTM;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AstmDriver.Abstractions
{
    public abstract class AbstractAstmRecord
	{
		protected const char CR = '\r';

		private AbstractAstmSubRecord CreateSubrecord(string inputString, Type type)
		{
			return (AbstractAstmSubRecord)Activator.CreateInstance(type, inputString);
		}

		private object CreateAstmEnum(string @string, Type type)
		{
			object[] flagsAttribs = type.GetCustomAttributes(typeof(FlagsAttribute), inherit: false);
			if ((int)flagsAttribs.LongLength > 0)
			{
				string inputString = string.Empty;
				string enumStringValue = null;
				int i = 0;
				FieldInfo[] fields = type.GetFields();
				if (fields != null)
				{
					for (; i < (int)fields.LongLength; i++)
					{
						FieldInfo fi = fields[i];
						AstmEnumAttribute[] attribs = fi.GetCustomAttributes(typeof(AstmEnumAttribute), inherit: false) as AstmEnumAttribute[];
						if ((int)attribs.LongLength > 0)
						{
							enumStringValue = attribs[0].LisID;
						}
						if (@string == null)
						{
							continue;
						}
						CharEnumerator enumerator = @string.GetEnumerator();
						if (enumerator == null)
						{
							continue;
						}
						try
						{
							while (enumerator.MoveNext())
							{
								char ch = enumerator.Current;
								if (string.Compare(enumStringValue, new string(ch, 1), ignoreCase: true) == 0)
								{
									inputString = inputString + fi.Name + ",";
								}
							}
						}
						finally
						{
							enumerator.Dispose();
						}
					}
				}
				if (inputString.Length > 0)
				{
					inputString = inputString.Remove(inputString.Length - 1, 1);
					return Enum.Parse(type, inputString);
				}
			}
			else
			{
				string inputString = null;
				int i = 0;
				FieldInfo[] fields = type.GetFields();
				if (fields != null)
				{
					for (; i < (int)fields.LongLength; i++)
					{
						FieldInfo fi = fields[i];
						AstmEnumAttribute[] attribs = fi.GetCustomAttributes(typeof(AstmEnumAttribute), inherit: false) as AstmEnumAttribute[];
						if ((int)attribs.LongLength > 0)
						{
							inputString = attribs[0].LisID;
						}
						if (string.Compare(inputString, @string, ignoreCase: true) == 0)
						{
							return Enum.Parse(type, fi.Name);
						}
					}
				}
			}
			object Result = default(object);
			return Result;
		}

		private string[] CreateRemainingFieldsArray(RecordFields recordFields, int startIndex)
		{
			List<string> temp = new List<string>();
			int count = recordFields.Count;
			int i = startIndex;
			if (i <= count)
			{
				count++;
				do
				{
					temp.Add(recordFields.GetField(i));
					i++;
				}
				while (i != count);
			}
			return temp.ToArray();
		}

		private string GetEnumAstmString(object @enum)
		{
			Type enumType = @enum?.GetType();
			object[] flagsAttribs = enumType.GetCustomAttributes(typeof(FlagsAttribute), inherit: false);
			FieldInfo ev;
			AstmEnumAttribute[] attribs;
			if ((int)flagsAttribs.LongLength > 0)
			{
				string Result = string.Empty;
				string[] inputVals = @enum.ToString().Split(new char[1] { ',' });
				FieldInfo[] enumValues = enumType.GetFields();
				int i = 0;
				FieldInfo[] array = enumValues;
				if (array != null)
				{
					for (; i < (int)array.LongLength; i++)
					{
						ev = array[i];
						attribs = ev.GetCustomAttributes(typeof(AstmEnumAttribute), inherit: false) as AstmEnumAttribute[];
						if ((int)attribs.LongLength <= 0)
						{
							continue;
						}
						int j = 0;
						string[] array2 = inputVals;
						if (array2 == null)
						{
							continue;
						}
						for (; j < (int)array2.LongLength; j++)
						{
							string iv = array2[j];
							if (iv.Trim() == ev.Name)
							{
								Result += attribs[0].LisID;
							}
						}
					}
				}
				return Result;
			}
			ev = enumType.GetField(@enum.ToString());
			attribs = ev.GetCustomAttributes(typeof(AstmEnumAttribute), inherit: false) as AstmEnumAttribute[];
			return ((int)attribs.LongLength <= 0) ? null : attribs[0].LisID;
		}

		private string EscapeString(string @string, bool isSubrecord)
		{
			string Result = @string;
			Result = Result.Replace(new string(Delimiters.EscapeCharacter, 1), new string(Delimiters.EscapeCharacter, 1) + "E" + new string(Delimiters.EscapeCharacter, 1));
			Result = Result.Replace(new string(Delimiters.FieldDelimiter, 1), new string(Delimiters.EscapeCharacter, 1) + "F" + new string(Delimiters.EscapeCharacter, 1));
			if (isSubrecord)
			{
				Result = Result.Replace(new string(Delimiters.ComponentDelimiter, 1), new string(Delimiters.EscapeCharacter, 1) + "S" + new string(Delimiters.EscapeCharacter, 1));
			}
			return Result;
		}

		private string RemoveOptionalSubFields(string aString)
		{
			if (aString.Contains(new string(Delimiters.ComponentDelimiter, 1)))
			{
				return aString.Split(new char[1] { Delimiters.ComponentDelimiter })[0];
			}
			return aString;
		}

		public virtual string ToAstmString()
		{
			bool isSubRecord = this is AbstractAstmSubRecord;
			char sepChar = ((!isSubRecord) ? Delimiters.FieldDelimiter : Delimiters.ComponentDelimiter);
			StringBuilder sb = new StringBuilder();
			Dictionary<int, string> fieldList = new Dictionary<int, string>();
			Type selfType = this?.GetType();
			PropertyInfo[] props = selfType.GetProperties();
			int maxFieldIndex = int.MinValue;
			int minFieldIndex = int.MaxValue;
			int i = 0;
			PropertyInfo[] array = props;
			string propString;
			if (array != null)
			{
				for (; i < (int)array.LongLength; i++)
				{
					PropertyInfo prop = array[i];
					object[] attribs = prop.GetCustomAttributes(typeof(AstmRecordFieldAttribute), inherit: false);
					if ((int)attribs.LongLength <= 0)
					{
						continue;
					}
					AstmRecordFieldAttribute attrib = (AstmRecordFieldAttribute)attribs[0];
					Type propType = prop.PropertyType;
					Type nullablePropType = Nullable.GetUnderlyingType(propType);
					if ((object)nullablePropType != null)
					{
						propType = nullablePropType;
					}
					propString = null;
					object propVal = prop.GetValue(this, null);
					if (propVal != null)
					{
						if (propType == typeof(DateTime))
						{
							AstmDateTimeUsage dateTimeUsage = AstmDateTimeUsage.DateTime;
							attribs = prop.GetCustomAttributes(typeof(AstmDateTimeUsageAttribute), inherit: false);
							if ((int)attribs.LongLength == 1)
							{
								AstmDateTimeUsageAttribute dtAttrib = (AstmDateTimeUsageAttribute)attribs[0];
								dateTimeUsage = dtAttrib.DateTimeUsage;
							}
							propString = ((DateTime)propVal).ToAstmDate(dateTimeUsage);
						}
						else if (propType.IsEnum)
						{
							propString = GetEnumAstmString(propVal);
						}
						else if (propType.BaseType == typeof(AbstractAstmSubRecord))
						{
							propString = (propVal as AbstractAstmSubRecord).ToAstmString();
						}
						else if (propType.IsArray)
						{
							if (attrib is AstmRecordRemainingFieldsAttribute)
							{
								string[] ar = (string[])propVal;
								propString = string.Join(new string(sepChar, 1), ar);
							}
						}
						else
						{
							propString = propVal.ToString();
						}
					}
					if (!string.IsNullOrEmpty(propString))
					{
						fieldList.Add(attrib.FieldIndex, propString);
						if (attrib.FieldIndex > maxFieldIndex)
						{
							maxFieldIndex = attrib.FieldIndex;
						}
					}
					if (attrib.FieldIndex < minFieldIndex)
					{
						minFieldIndex = attrib.FieldIndex;
					}
				}
			}
			if (minFieldIndex <= maxFieldIndex)
			{
				int num = maxFieldIndex - 1;
				i = minFieldIndex;
				if (i <= num)
				{
					num++;
					do
					{
						fieldList.TryGetValue(i, out propString);
						if (!string.IsNullOrEmpty(propString))
						{
							sb.Append(EscapeString(propString, isSubRecord));
						}
						sb.Append(sepChar);
						i++;
					}
					while (i != num);
				}
			}
			fieldList.TryGetValue(maxFieldIndex, out var field);
			if (!string.IsNullOrEmpty(field))
			{
				sb.Append(field);
			}
			if (!isSubRecord)
			{
				sb.Append('\r');
			}
			return sb.ToString();
		}

		public string RawData { get; set; }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			Type selfType = this?.GetType();
			PropertyInfo[] props = selfType.GetProperties();
			int i = 0;
			PropertyInfo[] array = props;
			if (array != null)
			{
				for (; i < (int)array.LongLength; i++)
				{
					PropertyInfo prop = array[i];
					object[] attribs = prop.GetCustomAttributes(typeof(AstmRecordFieldAttribute), inherit: false);
					if ((int)attribs.LongLength > 0)
					{
						object propVal = prop.GetValue(this, null);
						string propString = null;
						if (propVal != null)
						{
							propString = propVal.ToString();
						}
						if (!string.IsNullOrEmpty(propString))
						{
							sb.Append(prop.Name);
							sb.Append(": ");
							sb.AppendLine(propString);
						}
					}
				}
			}
			return sb.ToString();
		}

		public AbstractAstmRecord(string recordString)
		{
			RawData = string.Copy(recordString);

			bool isSubRecord = this is AbstractAstmSubRecord;
			char sepChar = ((!isSubRecord) ? Delimiters.FieldDelimiter : Delimiters.ComponentDelimiter);
			Type selfType = this?.GetType();
			PropertyInfo[] props = selfType.GetProperties();
			int limit = int.MaxValue;
			if (isSubRecord && (int)props.LongLength > 0 && !props[(int)props.LongLength - 1].PropertyType.IsArray)
			{
				limit = props.Length;
			}
			RecordFields rf = new RecordFields(recordString, sepChar, limit);
			int i = 0;
			PropertyInfo[] array = props;
			if (array == null)
			{
				return;
			}
			for (; i < (int)array.LongLength; i++)
			{
				PropertyInfo prop = array[i];
				object[] attribs = prop.GetCustomAttributes(typeof(AstmRecordFieldAttribute), inherit: false);
				if ((int)attribs.LongLength <= 0)
				{
					continue;
				}
				AstmRecordFieldAttribute attrib = (AstmRecordFieldAttribute)attribs[0];
				string field = rf.GetField(attrib.FieldIndex);
				if (string.IsNullOrEmpty(field))
				{
					continue;
				}
				Type propType = prop.PropertyType;
				Type nullablePropType = Nullable.GetUnderlyingType(propType);
				if ((object)nullablePropType != null)
				{
					propType = nullablePropType;
				}
				if (!(propType == typeof(int)))
				{
					if (!(propType == typeof(string)))
					{
						if (propType == typeof(DateTime))
						{
							AstmDateTimeUsage dateTimeUsage = AstmDateTimeUsage.DateTime;
							attribs = prop.GetCustomAttributes(typeof(AstmDateTimeUsageAttribute), inherit: false);
							if ((int)attribs.LongLength == 1)
							{
								AstmDateTimeUsageAttribute dtAttrib = (AstmDateTimeUsageAttribute)attribs[0];
								dateTimeUsage = dtAttrib.DateTimeUsage;
							}
							prop.SetValue(this, RemoveOptionalSubFields(field).AstmStringToDateTime(dateTimeUsage), null);
						}
						else if (propType.IsEnum)
						{
							prop.SetValue(this, CreateAstmEnum(RemoveOptionalSubFields(field), propType), null);
						}
						else if (propType.BaseType == typeof(AbstractAstmSubRecord))
						{
							prop.SetValue(this, CreateSubrecord(field, propType), null);
						}
						else if (propType.IsArray)
						{
							if (!(attrib is AstmRecordRemainingFieldsAttribute))
							{
								throw new FormatException("The input String was not of the correct format.");
							}
							prop.SetValue(this, CreateRemainingFieldsArray(rf, attrib.FieldIndex), null);
						}
					}
					else
					{
						prop.SetValue(this, field, null);
					}
				}
				else
				{
					prop.SetValue(this, int.Parse(RemoveOptionalSubFields(field)), null);
				}
			}
		}

		public AbstractAstmRecord()
		{
		}
	}
}
