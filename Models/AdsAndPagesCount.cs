global using Repos;
namespace Models
{
    /// <summary>
    /// Список объявлений и количество страниц для возврата
    /// </summary>
    public class AdsAndPagesCount
    {
        public List<Advertisement> Ads { get; set; }
        public int PagesCount { get; set; }
    }
    public enum SortCriteria { Rating, CreationDate }
    public enum RatingChange { up, down }
}
