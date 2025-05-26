using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using WatchZone.BusinessLogic.Database;
using WatchZone.BusinessLogic.Interface;
using WatchZone.Domain.Entities.User;

namespace WatchZone.BusinessLogic.Repository
{
    public class SessionRepository : ISessionRepository
    {
        public async Task<Session> GetSessionByCookieAsync(string cookieValue)
        {
            using (var context = new SessionContext())
            {
                return await Task.FromResult(context.Sessions.FirstOrDefault(s => 
                    s.CookieString == cookieValue && s.ExpireTime > DateTime.Now));
            }
        }

        public async Task<Session> GetSessionByUsernameAsync(string username)
        {
            using (var context = new SessionContext())
            {
                return await Task.FromResult(context.Sessions.FirstOrDefault(s => s.Username == username));
            }
        }

        public async Task<bool> CreateOrUpdateSessionAsync(string username, string cookieValue, DateTime expireTime)
        {
            using (var context = new SessionContext())
            {
                var existingSession = context.Sessions.FirstOrDefault(s => s.Username == username);
                
                if (existingSession != null)
                {
                    existingSession.CookieString = cookieValue;
                    existingSession.ExpireTime = expireTime;
                    context.Entry(existingSession).State = EntityState.Modified;
                }
                else
                {
                    context.Sessions.Add(new Session
                    {
                        Username = username,
                        CookieString = cookieValue,
                        ExpireTime = expireTime
                    });
                }
                
                await Task.FromResult(context.SaveChanges());
                return true;
            }
        }

        public async Task<bool> DeleteSessionAsync(string cookieValue)
        {
            using (var context = new SessionContext())
            {
                var session = context.Sessions.FirstOrDefault(s => s.CookieString == cookieValue);
                if (session != null)
                {
                    context.Sessions.Remove(session);
                    await Task.FromResult(context.SaveChanges());
                    return true;
                }
                return false;
            }
        }

        public async Task<bool> DeleteExpiredSessionsAsync()
        {
            using (var context = new SessionContext())
            {
                var expiredSessions = context.Sessions.Where(s => s.ExpireTime <= DateTime.Now).ToList();
                if (expiredSessions.Any())
                {
                    context.Sessions.RemoveRange(expiredSessions);
                    await Task.FromResult(context.SaveChanges());
                    return true;
                }
                return false;
            }
        }
    }
} 