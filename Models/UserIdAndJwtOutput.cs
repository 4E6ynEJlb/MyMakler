using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class UserIdAndJwtOutput
    {
        public string JwtToken { get; set; }
        public Guid userId {  get; set; }
        public UserIdAndJwtOutput(string jwtToken, Guid userId)
        {
            JwtToken = jwtToken;
            this.userId = userId;
        }
    }
}
