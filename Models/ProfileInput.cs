using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ProfileInput
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }        
    }
}
