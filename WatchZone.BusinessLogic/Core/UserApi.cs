using AutoMapper;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Web;
using WatchZone.Domain.Entities.User;
using WatchZone.Helper;
using WatchZone.BusinessLogic.Database;
using WatchZone.Domain.Enums;
using WatchZone.Domain.Model.User;
using WatchZone.BusinessLogic.Interface;

namespace WatchZone.BusinessLogic.Core
{
    public class UserApi
    {
		internal ULoginResp UserLoginAction(ULoginData data)
		{
			UDbTable result;
			var validate = new EmailAddressAttribute();
			if (validate.IsValid(data.Credential))
			{
				// pass = LoginUtility.GenHash(data.Password);
				using (var db = new UserContext())
				{
					result = db.Users.FirstOrDefault(u => u.Email == data.Credential && u.Password == data.Password);
				}

				if (result == null)
				{
					return new ULoginResp { Status = false, StatusMsg = "The Username or Password is Incorrect" };
				}

				using (var todo = new UserContext())
				{
					result.LasIp = data.LoginIp;
					result.LastLogin = data.LoginDateTime;
					todo.Entry(result).State = EntityState.Modified;
					todo.SaveChanges();
				}

				return new ULoginResp { Status = true };
			}
			else
			{
				var pass = LoginUtility.GenHash(data.Password);
				using (var db = new UserContext())
				{
					result = db.Users.FirstOrDefault(u => u.Username == data.Credential && u.Password == pass);
				}

				if (result == null)
				{
					return new ULoginResp { Status = false, StatusMsg = "The Username or Password is Incorrect" };
				}

				using (var todo = new UserContext())
				{
					result.LasIp = data.LoginIp;
					result.LastLogin = data.LoginDateTime;
					todo.Entry(result).State = EntityState.Modified;
					todo.SaveChanges();
				}

				return new ULoginResp { Status = true };
			}
		}
		internal URegisterResp UserRegisterAction(URegisterData data)
		{
			UDbTable result = new UDbTable();
			var validate = new EmailAddressAttribute();
			if (validate.IsValid(data.Credential))
			{
				var pass = LoginUtility.GenHash(data.Password);
				using (var db = new UserContext())
				{
					result.Username = data.Credential;
					result.Password = data.Password;
					result.Email = data.Credential;
					result = db.Users.Add(result);
					db.SaveChanges();
				}

				using (var todo = new UserContext())
				{
                    result.Username = data.Credential;
                    result.Password = data.Password;
                    result.LasIp = data.RegisterIp;
					result.LastLogin = data.RegisterDateTime;
					todo.Entry(result).State = EntityState.Modified;
					try
					{
						int state = todo.SaveChanges();
					}
					catch(DbEntityValidationException e)
                    {
                        foreach (var validationErrors in e.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Console.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                            }
                        }
						throw;
                    }

                }

				return new URegisterResp { Status = true };
			}
			return new URegisterResp { Status = false, StatusMsg = "Something went wrong." };
		}
		internal HttpCookie Cookie(string loginCredential)
		{
			var apiCookie = new HttpCookie("X-KEY")
			{
				Value = CookieUtility.Create(loginCredential)
			};

			using (var db = new SessionContext())
			{
				Session curent;
				var validate = new EmailAddressAttribute();
				if (validate.IsValid(loginCredential))
				{
					curent = (from e in db.Sessions where e.Username == loginCredential select e).FirstOrDefault();
				}
				else
				{
					curent = (from e in db.Sessions where e.Username == loginCredential select e).FirstOrDefault();
				}

				if (curent != null)
				{
					curent.CookieString = apiCookie.Value;
					curent.ExpireTime = DateTime.Now.AddMinutes(60);
					using (var todo = new SessionContext())
					{
						todo.Entry(curent).State = EntityState.Modified;
						todo.SaveChanges();
					}
				}
				else
				{
					db.Sessions.Add(new Session
					{
						Username = loginCredential,
						CookieString = apiCookie.Value,
						ExpireTime = DateTime.Now.AddMinutes(60)
					});
					db.SaveChanges();
				}
			}

			return apiCookie;
		}

		internal URole? GetUserRoleByCookie(string cookie)
		{
			Session session;
			UDbTable currentUser;

			using (var db = new SessionContext())
			{
				session = db.Sessions.FirstOrDefault(s => s.CookieString == cookie && s.ExpireTime > DateTime.Now);
			}

			if (session == null) return null;

			using (var db = new UserContext())
			{
				var validate = new EmailAddressAttribute();
				if (validate.IsValid(session.Username))
				{
					currentUser = db.Users.FirstOrDefault(u => u.Email == session.Username);
				}
				else
				{
					currentUser = db.Users.FirstOrDefault(u => u.Username == session.Username);
				}
			}

			if (currentUser == null) return null;

			return currentUser.Level;
		}

		internal UserMinimal UserCookie(string cookie)
		{
			Session session;
			UDbTable curentUser;

			using (var db = new SessionContext())
			{
				session = db.Sessions.FirstOrDefault(s => s.CookieString == cookie && s.ExpireTime > DateTime.Now);
			}

			if (session == null) return null;
			using (var db = new UserContext())
			{
				var validate = new EmailAddressAttribute();
				if (validate.IsValid(session.Username))
				{
					curentUser = db.Users.FirstOrDefault(u => u.Email == session.Username);
				}
				else
				{
					curentUser = db.Users.FirstOrDefault(u => u.Username == session.Username);
				}
			}

			if (curentUser == null) return null;
			var userminimal = Mapper.Map<UserMinimal>(curentUser);

			return userminimal;
		}

		internal string UserAuthLogicAction(UserLoginDTO data, IErrorHandler errorHandler = null)
		{
			try
			{
				using (var context = new UserContext())
				{
					var user = context.Users.FirstOrDefault(u => 
						(u.Username == data.UserName || u.Email == data.UserName) &&
						u.Password == data.Password);

					if (user != null)
					{
						errorHandler?.LogInfo($"User {data.UserName} authenticated successfully");
						return "Authentication successful";
					}

					errorHandler?.LogWarning($"Failed authentication attempt for {data.UserName}");
					return "Invalid credentials";
				}
			}
			catch (Exception ex)
			{
				errorHandler?.LogError(ex, $"Error during authentication for {data.UserName}");
				return "Authentication error occurred";
			}
		}
	}
} 