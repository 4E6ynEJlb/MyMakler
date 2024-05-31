using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    /// <summary>
    /// Класс для получения запроса на список объявлений
    /// </summary>
    public class GetAllAdsArgs
    {
        /// <summary>
        /// Критерий сортировки
        /// </summary>
        public SortCriteria Criterion { get; set; }
        /// <summary>
        /// Возрастание/убывание
        /// </summary>
        public bool IsASC { get; set; }
        /// <summary>
        /// Ключевое слово
        /// </summary>
        public string? KeyWord { get; set; }
        /// <summary>
        /// Минимальный рейтинг
        /// </summary>
        public int? RatingLow { get; set; }
        /// <summary>
        /// Максимальный рейтинг
        /// </summary>
        public int? RatingHigh { get; set; }
        /// <summary>
        /// Страница
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// Кол-во объявлений на странице
        /// </summary>
        public int PageSize { get; set; }
    }
}
