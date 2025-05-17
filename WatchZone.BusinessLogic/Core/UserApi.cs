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
			Mapper.Initialize(cfg => cfg.CreateMap<UDbTable, UserMinimal>());
			var userminimal = Mapper.Map<UserMinimal>(curentUser);

			return userminimal;
		}
	}
} 