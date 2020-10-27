using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyRec.Audio
{
	static class PathFormatter
	{
		private static Regex regex = new Regex(@"%(.*?)%");

		public static string Format(string path)
		{
			DateTime now = DateTime.Now;
			return regex.Replace(path, (m) => now.ToString(m.Groups[1].Value));
		}
	}
}
