using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MyMakler.Controllers
{
    /// <summary>
    /// Работа с объявлениями 
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AdsWorkspaceController : ControllerBase
    {
        public AdsWorkspaceController(ILogics logics) 
        {
            _Logics = logics;
        }
        private readonly ILogics _Logics;
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> AddAdvertisement(AdvInput adInput)
        {
            return Ok(await _Logics.TryAddAdvertisement(adInput));
        }

        [HttpPost]
        [Route("AttachPic")]
        public async Task<IActionResult> AttachPicToAdvertisement(IFormFile? file, Guid adId)
        {
            await _Logics.TryAttachPic(file, adId);
            return Ok();
        }


        [HttpDelete]
        [Route("DetachPic")]
        public async Task<IActionResult> DetachPicFromAdvertisement(Guid adId)
        {
            await _Logics.TryDetachPic(adId);
            return Ok();
        }

        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> DeleteAdvertisement(Guid guid)
        {
            await _Logics.TryDeleteAdvertisement(guid);
            return Ok();
        }


        [HttpPut]
        [Route("Edit")]
        public async Task<IActionResult> EditAdvertisement(AdvEdit advEdit)
        {
            await _Logics.TryEditAdvertisement(advEdit);
            return Ok();
        }


        [HttpGet]
        [Route("PersonalAds")]
        [AllowAnonymous]
        public async Task<IActionResult> AdsByUser(Guid guid)
        {
            var result = await _Logics.TryGetPersonalAdsList(guid);
            if (result == null)
                return NoContent();
            return Ok(result);
        }
    }
}
