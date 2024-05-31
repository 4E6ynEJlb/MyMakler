using Microsoft.AspNetCore.Mvc;
using System.Net;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.Swagger.Annotations;
using Microsoft.AspNetCore.Authorization;
namespace MyMakler.Controllers
{
    /// <summary>
    /// Просмотр объявлений и изменение их рейтинга
    /// </summary>
    [ApiController]
    [Route("[controller]")]        
    public class AdsController : ControllerBase
    {
        public AdsController(ILogics logics)
        {
            _Logics = logics;
        }
        private readonly ILogics _Logics;
        [HttpOptions]
        [Route("All")]
        public async Task<IActionResult> GetAllAds(GetAllAdsArgs args)
        {
            var result = await _Logics.TryGetAdsListAndPgCount(args);
            if (result.Ads == null)
                return StatusCode((int)HttpStatusCode.NoContent, result.PagesCount);
            return Ok(result);
        }


        [HttpPut]
        [Route("Rating")]
        [Authorize]
        public async Task<IActionResult> ChangeRating(Guid guid, RatingChange change)
        {
                await _Logics.TryChangeRating(guid, change);
                return Ok();
        }

    }
}
