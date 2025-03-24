using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchZone.BusinessLogic.Core;
using WatchZone.BusinessLogic.Interface;
using WatchZone.Domain.Model.User;

namespace WatchZone.BusinessLogic.BL_Struct
{
    public class AuthBL : UserApi, IAuth
    {
        public string UserAuthLogic(UserLoginDTO data)
        {
            throw new NotImplementedException();
        }
    }
} 