using WatchZone.BusinessLogic.Interface;
using WatchZone.BusinessLogic.Interface.Repositories;
using WatchZone.BusinessLogic.BL_Struct;
using WatchZone.BusinessLogic.BL_Struct.Basket;
using WatchZone.BusinessLogic.Services;

namespace WatchZone.BusinessLogic
{
    public class BussinesLogic
    {
        private readonly IErrorHandler _errorHandler;
        private readonly IAuth _authService;
        private readonly ISession _sessionService;
        private readonly IListingService _listingService;
        private readonly IUserService _userService;
        private readonly IBasketService _basketService;
        private readonly OrderService _orderService;
        private readonly IReviewService _reviewService;

        public BussinesLogic()
        {
            // Initialize services with dependency injection pattern
            _errorHandler = new ErrorHandlerBL();
            _authService = new AuthBL(_errorHandler);
            _sessionService = new SessionBL();
            _listingService = new ListingServiceBL(_errorHandler);
            _userService = new UserServiceBL(_errorHandler);
            _orderService = new OrderService();
            _reviewService = new ReviewServiceBL(_errorHandler);
            // _basketService would be initialized here when repositories are implemented
        }

        public IErrorHandler GetErrorHandler()
        {
            return _errorHandler;
        }

        public IAuth GetAuthService()
        {
            return _authService;
        }

        public ISession GetSessionBL()
        {
            return _sessionService;
        }

        public IListingService GetListingService()
        {
            return _listingService;
        }

        public IUserService GetUserService()
        {
            return _userService;
        }

        public IBasketService GetBasketService()
        {
            return _basketService;
        }

        public OrderService GetOrderService()
        {
            return _orderService;
        }

        public IReviewService GetReviewService()
        {
            return _reviewService;
        }
    }
}
