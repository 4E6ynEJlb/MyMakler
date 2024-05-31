using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    /// <summary>
    /// Класс для создания объявления
    /// </summary>
    public class AdvInput
    {
        public int Number { get; set; }
        public Guid UserId { get; set; }
        public string Text { get; set; }
    }
}
