using System;

namespace WatchZone.BusinessLogic.Interface
{
    public interface IErrorHandler
    {
        void LogError(Exception exception, string additionalInfo = null);
        void LogWarning(string message, string additionalInfo = null);
        void LogInfo(string message, string additionalInfo = null);
        bool HandleError(Exception exception, out string userFriendlyMessage);
    }
} 