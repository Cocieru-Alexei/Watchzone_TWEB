using System;
using System.Diagnostics;
using WatchZone.BusinessLogic.Interface;

namespace WatchZone.BusinessLogic.BL_Struct
{
    public class ErrorHandlerBL : IErrorHandler
    {
        public void LogError(Exception exception, string additionalInfo = null)
        {
            try
            {
                var message = $"ERROR: {exception.Message}";
                if (!string.IsNullOrEmpty(additionalInfo))
                    message += $" | Additional Info: {additionalInfo}";
                
                message += $" | Stack Trace: {exception.StackTrace}";
                
                // Log to debug output (in production, you'd log to a file, database, or logging service)
                Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
                
                // In production, also log to a persistent store
                // Example: File, Database, Event Log, etc.
            }
            catch
            {
                // Logging shouldn't throw exceptions
            }
        }

        public void LogWarning(string message, string additionalInfo = null)
        {
            try
            {
                var logMessage = $"WARNING: {message}";
                if (!string.IsNullOrEmpty(additionalInfo))
                    logMessage += $" | Additional Info: {additionalInfo}";
                
                Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {logMessage}");
            }
            catch
            {
                // Logging shouldn't throw exceptions
            }
        }

        public void LogInfo(string message, string additionalInfo = null)
        {
            try
            {
                var logMessage = $"INFO: {message}";
                if (!string.IsNullOrEmpty(additionalInfo))
                    logMessage += $" | Additional Info: {additionalInfo}";
                
                Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {logMessage}");
            }
            catch
            {
                // Logging shouldn't throw exceptions
            }
        }

        public bool HandleError(Exception exception, out string userFriendlyMessage)
        {
            try
            {
                LogError(exception);
                
                // Convert technical exceptions to user-friendly messages
                switch (exception)
                {
                    case UnauthorizedAccessException _:
                        userFriendlyMessage = "You don't have permission to perform this action.";
                        return true;
                    case ArgumentException _:
                        userFriendlyMessage = "Invalid input provided. Please check your data and try again.";
                        return true;
                    case TimeoutException _:
                        userFriendlyMessage = "The operation timed out. Please try again later.";
                        return true;
                    default:
                        userFriendlyMessage = "An unexpected error occurred. Please try again later.";
                        return false;
                }
            }
            catch
            {
                userFriendlyMessage = "A system error occurred.";
                return false;
            }
        }
    }
} 