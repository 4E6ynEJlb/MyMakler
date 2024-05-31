using Swashbuckle.AspNetCore.Filters;

namespace Models
{
    /// <summary>
    /// Пример объекта для запроса на список объявлений
    /// </summary>
    public class GetAllAdsArgsExample : IExamplesProvider<GetAllAdsArgs>
    {
        public GetAllAdsArgs GetExamples()
        {
            return new GetAllAdsArgs
            {
                Criterion = SortCriteria.CreationDate,
                IsASC = true,
                KeyWord = null,
                Page = 1,
                PageSize = 10,
                RatingHigh = null,
                RatingLow = null
            };
        }
    }
}
