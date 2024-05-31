using Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repos
{
    public class User
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public Guid Id { get; set; }
        public UserProfile Profile { get; set; }
        
    }
}
