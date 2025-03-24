using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchZone.Domain.Model.User;

namespace WatchZone.BusinessLogic.Interface
{
    public interface IAuth
    {
        string UserAuthLogic(UserLoginDTO data);
    }
} 