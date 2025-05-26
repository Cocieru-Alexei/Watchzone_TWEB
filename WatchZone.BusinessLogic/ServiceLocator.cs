using WatchZone.BusinessLogic.Interface;
using WatchZone.BusinessLogic.BL_Struct;
using WatchZone.BusinessLogic.Services;

namespace WatchZone.BusinessLogic
{
    /// <summary>
    /// Service Locator pattern to centralize business logic instantiation
    /// This is a better approach than manual instantiation in controllers
    /// while maintaining compatibility with legacy .NET Framework MVC
    /// </summary>
    public static class ServiceLocator
    {
        private static BussinesLogic _businessLogic;
        
        /// <summary>
        /// Gets the centralized business logic instance
        /// </summary>
        public static BussinesLogic BusinessLogic
        {
            get
            {
                if (_businessLogic == null)
                {
                    _businessLogic = new BussinesLogic();
                }
                return _businessLogic;
            }
        }

        /// <summary>
        /// Gets the Auth service
        /// </summary>
        public static IAuth GetAuthService()
        {
            return BusinessLogic.GetAuthService();
        }

        /// <summary>
        /// Gets the Listing service
        /// </summary>
        public static IListingService GetListingService()
        {
            return BusinessLogic.GetListingService();
        }

        /// <summary>
        /// Gets the User service
        /// </summary>
        public static IUserService GetUserService()
        {
            return BusinessLogic.GetUserService();
        }

        /// <summary>
        /// Gets the Cart service
        /// </summary>
        public static ICartService GetCartService()
        {
            return BusinessLogic.GetCartService();
        }

        /// <summary>
        /// Gets the Error Handler service
        /// </summary>
        public static IErrorHandler GetErrorHandler()
        {
            return BusinessLogic.GetErrorHandler();
        }

        /// <summary>
        /// Gets the Order service
        /// </summary>
        public static OrderService GetOrderService()
        {
            return BusinessLogic.GetOrderService();
        }

        /// <summary>
        /// Gets the Review service
        /// </summary>
        public static IReviewService GetReviewService()
        {
            return BusinessLogic.GetReviewService();
        }

        /// <summary>
        /// Resets the service locator (useful for testing)
        /// </summary>
        public static void Reset()
        {
            _businessLogic = null;
        }
    }
} 