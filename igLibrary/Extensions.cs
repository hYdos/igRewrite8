namespace igLibrary
{
	public static class Extensions
	{
		public static string ReplaceEnd(this string str, string original, string replacement)
		{
			if(original == null) throw new ArgumentNullException(nameof(original));
			if(replacement == null) throw new ArgumentNullException(nameof(replacement));
			if(str.EndsWith(original))
			{
				return str.Substring(0, str.Length - original.Length) + replacement;
			}
			else return str;
		}
		public static string ReplaceBeginning(this string str, string original, string replacement)
		{
			if(original == null) throw new ArgumentNullException(nameof(original));
			if(replacement == null) throw new ArgumentNullException(nameof(replacement));
			if(str.StartsWith(original))
			{
				return replacement + str.Substring(original.Length);
			}
			else return str;
		}
	}
}