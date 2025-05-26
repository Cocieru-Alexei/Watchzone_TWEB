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
			using (SHA256 sha256 = SHA256.Create())
			{
				var originalBytes = Encoding.UTF8.GetBytes(password + __sauce);
				var encodedBytes = sha256.ComputeHash(originalBytes);
				return BitConverter.ToString(encodedBytes).Replace("-", "").ToLower();
			}
		}
	}
}
