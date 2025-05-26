using System;
using System.Threading.Tasks;
using WatchZone.Domain.Entities.User;

namespace WatchZone.BusinessLogic.Interface
{
    public interface ISessionRepository
    {
        Task<Session> GetSessionByCookieAsync(string cookieValue);
        Task<Session> GetSessionByUsernameAsync(string username);
        Task<bool> CreateOrUpdateSessionAsync(string username, string cookieValue, DateTime expireTime);
        Task<bool> DeleteSessionAsync(string cookieValue);
        Task<bool> DeleteExpiredSessionsAsync();
    }
} 