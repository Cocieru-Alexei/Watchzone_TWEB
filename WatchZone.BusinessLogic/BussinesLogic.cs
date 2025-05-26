using WatchZone.BusinessLogic.Interface;
using WatchZone.BusinessLogic.BL_Struct;
using WatchZone.BusinessLogic.BL_Struct.Basket;
using WatchZone.BusinessLogic.Services;
using WatchZone.BusinessLogic.Repository;

namespace WatchZone.BusinessLogic
{
    public class BussinesLogic
    {
        private readonly IErrorHandler _errorHandler;
        private readonly IAuth _authService;
        private readonly IListingService _listingService;
        private readonly IUserService _userService;
        private readonly IBasketService _basketService;
        private readonly ICartService _cartService;
        private readonly OrderService _orderService;
        private readonly IReviewService _reviewService;
        private readonly IUserRepository _userRepository;
        private readonly ISessionRepository _sessionRepository;

        public BussinesLogic()
        {
            // Initialize repositories first
            _userRepository = new UserRepository();
            _sessionRepository = new SessionRepository();
            
            // Initialize services with dependency injection pattern
            _errorHandler = new ErrorHandlerBL();
            _authService = new AuthBL(_errorHandler, _userRepository, _sessionRepository);
            _listingService = new ListingServiceBL(_errorHandler);
            _userService = new UserServiceBL(_errorHandler, _userRepository);
            _cartService = new CartServiceBL(_errorHandler);
            _orderService = new OrderService(_errorHandler);
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

        public ICartService GetCartService()
        {
            return _cartService;
        }

        public IReviewService GetReviewService()
        {
            return _reviewService;
        }

        public IUserRepository GetUserRepository()
        {
            return _userRepository;
        }

        public ISessionRepository GetSessionRepository()
        {
            return _sessionRepository;
        }
    }
}
