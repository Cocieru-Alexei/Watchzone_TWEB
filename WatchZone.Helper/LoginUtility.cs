using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WatchZone.Helper
{
    public static class LoginUtility
    {
		static readonly string __sauce = "twutm2018";
		public static string GenHash(string password)
		{
			MD5 md5 = new MD5CryptoServiceProvider();
			var originalBytes = Encoding.Default.GetBytes(password + __sauce);
			var encodedBytes = md5.ComputeHash(originalBytes);
			return BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
		}
	}
}
