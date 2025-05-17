using WatchZone.BusinessLogic.Interface.Repositories;
namespace WatchZone.BusinessLogic
{
    public class BussinesLogic
    {
        public ISession GetSessionBL()
        {
            return new SessionBL();
        }
    }
}
