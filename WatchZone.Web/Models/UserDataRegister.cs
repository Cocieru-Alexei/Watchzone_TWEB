using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WatchZone.Web.Models.Auth
{
	public class UserDataRegister
	{
		public string Password { get; set; }
		//[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
		public string UserName { get; set; }
	}
}