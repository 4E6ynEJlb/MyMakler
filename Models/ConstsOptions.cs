using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    /// <summary>
    /// Класс для считывания констант из конфига
    /// </summary>
    public class ConstsOptions
    {
        public const string ConstsConfiguration = "ConstsConfiguration";
        /// <summary>
        /// Макс. кол-во объявлений на пользователя (не админа)
        /// </summary>
        public int AdsMaxCount { get; set; }
        /// <summary>
        /// Время жизни объявлений
        /// </summary>
        public int AdLifeDays { get; set; }
        /// <summary>
        /// Кол-во тиков между циклами очистки в сервисах
        /// </summary>
        public int TicksCount { get; set; }
        /// <summary>
        /// Шаблон ссылки к картинкам
        public string LinkTemplate { get; set; }
        /// <summary>
        /// Директория для картинок
        /// </summary>
        public string PicsDirectory { get; set; }
    }
}
