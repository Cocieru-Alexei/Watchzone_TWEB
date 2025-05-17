using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WatchZone.Domain.Entities.User
{
    public class URegisterData
    {
        public string Credential { get; set; }
        public string Password { get; set; }
        public string RegisterIp { get; set; }
        public DateTime RegisterDateTime { get; set; }
    }
}
