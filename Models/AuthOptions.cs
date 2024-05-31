using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class AuthOptions
    {
        public const string Issuer = "MyMaklerIssuer"; 
        public const string Client = "MyMaclerClient"; 
        const string Key = "OtcheNashBillGeytsIzbaviCodOtBagovBesovskih";   
        public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
    }
}
