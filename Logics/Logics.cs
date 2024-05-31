global using Repos;
global using Models;
using Microsoft.EntityFrameworkCore;
using Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Runtime;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace LogicsLib
{
    public class Logics : ILogics
    {
        public Logics(ApplicationContext context, IOptions<ConstsOptions> constsOptions, ILogger<Logics> logger)
        {
            Context = context;
            var constsValues = constsOptions.Value;            
            AdsMaxCount = constsValues.AdsMaxCount;
            AdLifeDays = constsValues.AdLifeDays;
            TicksCount = constsValues.TicksCount;
            LinkTemplate = constsValues.LinkTemplate;
            PicsDirectory = constsValues.PicsDirectory;
            _Logger = logger;
        }
        private ILogger _Logger;
        private readonly ApplicationContext Context;
        private readonly int AdsMaxCount;
        private readonly int AdLifeDays ;
        private readonly int TicksCount ;
        private readonly string PicsDirectory;
        private readonly string LinkTemplate;
        
        public async Task InitializeDb()
        {
            Guid guid1 = await TryRegisterUser(new RegisterModel() { Login = "login111111", Password = "password111111" });
            Guid guid2 = await TryRegisterUser(new RegisterModel() { Login = "login222222", Password = "password222222" });
            await TryAddUserProfile(new ProfileInput() { Id = guid1, Name = "name1", IsAdmin = true });
            await TryAddUserProfile(new ProfileInput() { Id = guid2, Name = "name2", IsAdmin = false });
            await TryAddAdvertisement(new AdvInput() { Number = 77846123, Text = "Продам гараж недорого", UserId = guid1 });
            await TryAddAdvertisement(new AdvInput() { Number = 32164877, Text = "Куплю негараж дорого", UserId = guid2 });
        }
        public async void DeleteDetachedPics(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    DirectoryInfo drinfo = new DirectoryInfo(PicsDirectory);
                    FileInfo[] picsInfo = drinfo.GetFiles();

                    foreach (FileInfo picInfo in picsInfo)
                    {
                        if (await Context.Ads.Where(a => EF.Functions.Like(a.PicLink, $"%/{picInfo.Name}")).CountAsync(token) == 0)
                        {
                            picInfo.Delete();
                            _Logger.LogDebug("Pic has been deleted");
                        }
                        if (token.IsCancellationRequested)
                            break;
                    }
                    _Logger.LogDebug("End of deletion");

                    await Task.Delay(TicksCount, token);
                }
                catch
                {
                    _Logger.LogError("Deletion error");
                }
            }
        }
        public async void RemoveOldAds(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {

                    var removingAds = await Context.Ads.Where(a => a.DeletionDate <= DateTime.Now).ToListAsync(token);
                    foreach (Advertisement removingAd in removingAds)
                    {
                        Context.Ads.Remove(removingAd);
                        _Logger.LogDebug("Ad has been removed");
                        if (token.IsCancellationRequested)
                            break;
                    }
                    await Context.SaveChangesAsync(token);
                    _Logger.LogDebug("End of removation");

                    await Task.Delay(TicksCount, token);
                }
                catch
                {
                    _Logger.LogError("Removation error");
                }
            }
        }
        public async Task<Guid> TryLogin(LoginModel loginModel)
        {
            User user = await Context.Users.FirstOrDefaultAsync(u => u.Login == loginModel.Login && u.Password == loginModel.Password);
            if (user == null)
                throw new DoesNotExistException(typeof(User));
            return user.Id;
        }
        public async Task<Guid> TryRegisterUser(RegisterModel registerModel)
        {
            User user = new User()
            {
                Id = Guid.NewGuid(),
                Login = registerModel.Login,
                Password = registerModel.Password
            };
            Context.Users.Add(user);
            await Context.SaveChangesAsync();

            return user.Id;
        }
        public async Task TryAddUserProfile(ProfileInput profileInput)
        {
            UserProfile userProfile = new UserProfile()
            {
                Id = profileInput.Id,
                Name = profileInput.Name,
                IsAdmin = profileInput.IsAdmin
            };
            Context.UserProfiles.Add(userProfile);
            await Context.SaveChangesAsync();
        }
        public async Task<List<UserProfile>> TrySearchUserProfile(string name)
        {
            List<UserProfile> result;
            result = await Context.UserProfiles.Where(u => EF.Functions.Like(u.Name, $"%{name}%")).ToListAsync();

            return result;
        }
        public async Task<List<UserProfile>> TryGetUserProfilesList(int pageNumber, int pageSize)
        {
            List<UserProfile> usersList;
            int pagesCount = await Context.UserProfiles.CountAsync();
            if (pagesCount == 0)
                pagesCount = 1;
            else pagesCount = pagesCount / pageSize + ((pagesCount % pageSize) == 0 ? 0 : 1);
            if (pageNumber > pagesCount || pageNumber < 1)
                throw new InvalidPageException();
            usersList = await Context.UserProfiles.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return usersList;
        }
        public async Task<int> TryGetUserProfilesPagesCount(int pageSize)
        {
            int pagesCount = await Context.UserProfiles.CountAsync();

            if (pagesCount == 0)
                return 1;
            return pagesCount / pageSize + ((pagesCount % pageSize) == 0 ? 0 : 1);
        }
        public async Task TryDeleteUserProfile(Guid guid)
        {
            if (await Context.UserProfiles.FirstOrDefaultAsync(u => u.Id == guid) == null)
                throw new DoesNotExistException(typeof(UserProfile));
            UserProfile user = new UserProfile { Id = guid };
            Context.UserProfiles.Attach(user);
            Context.UserProfiles.Remove(user);
            await Context.SaveChangesAsync();

        }
        public async Task TryEditUserProfile(ProfileInput profileInput)
        {
            if (await Context.UserProfiles.FirstOrDefaultAsync(u => u.Id == profileInput.Id) == null)
                throw new DoesNotExistException(typeof(UserProfile));
            UserProfile userProfile = new UserProfile()
            {
                Id = profileInput.Id,
                Name = profileInput.Name,
                IsAdmin = profileInput.IsAdmin
            };
            Context.UserProfiles.Update(userProfile);
            await Context.SaveChangesAsync();

        }
        public async Task<Guid> TryAddAdvertisement(AdvInput adInput)
        {
            Guid adId = Guid.NewGuid();
            int thisUserAdsCount = await Context.Ads.Where(a => a.UserId == adInput.UserId).CountAsync();
            bool isThisUserAdmin = (await Context.UserProfiles.FirstOrDefaultAsync(u => u.Id == adInput.UserId)).IsAdmin;
            if (thisUserAdsCount >= AdsMaxCount && !isThisUserAdmin)
            {
                throw new TooManyAdsException();
            }
            if (await Context.UserProfiles.FirstOrDefaultAsync(u => u.Id == adInput.UserId) == null)
                throw new DoesNotExistException(typeof(UserProfile));
            Advertisement ad = new Advertisement()
            {
                User = null,
                Id = adId,
                Rating = 0,
                PicLink = "Empty",
                UserId = adInput.UserId,
                Text = adInput.Text,
                Number = adInput.Number,
                CreationDate = DateTime.Now,
                DeletionDate = DateTime.Now.AddDays(AdLifeDays),
            };
            Context.Ads.Add(ad);
            await Context.SaveChangesAsync();

            return adId;
        }
        public async Task TryAttachPic(IFormFile file, Guid adId)
        {
            Advertisement ad = await Context.Ads.FirstOrDefaultAsync(a => a.Id == adId);
            if (file == null)
                throw new EmptyFileException();
            if (!file.ContentType.Contains("image"))
                throw new InvalidFileFormatException();
            if (ad == null)
                throw new DoesNotExistException(typeof(Advertisement));
            using (FileStream fS = new FileStream(PicsDirectory + "\\" + file.FileName, FileMode.Create))
            {
                await file.CopyToAsync(fS);
            }
            ad.PicLink = LinkTemplate + file.FileName;
            Context.Ads.Update(ad);
            await Context.SaveChangesAsync();

        }
        public async Task TryDetachPic(Guid adId)
        {
            Advertisement ad = await Context.Ads.FirstOrDefaultAsync(a => a.Id == adId);
            if (ad == null)
                throw new DoesNotExistException(typeof(Advertisement));
            ad.PicLink = "Empty";
            Context.Ads.Update(ad);
            await Context.SaveChangesAsync();

        }
        public async Task TryDeleteAdvertisement(Guid guid) 
        {
            Advertisement ad = new Advertisement { Id = guid };
            if (await Context.Ads.FirstOrDefaultAsync(a => a.Id == guid) == null)
                throw new DoesNotExistException(typeof(Advertisement));
            Context.Ads.Attach(ad);
            Context.Ads.Remove(ad);
            await Context.SaveChangesAsync();

        }
        public async Task TryEditAdvertisement(AdvEdit advEdit) 
        {
            Advertisement adInDB = await Context.Ads.FirstOrDefaultAsync(a => a.Id == advEdit.Id);
            if (adInDB == null)
                throw new DoesNotExistException(typeof(Advertisement));
            adInDB.Number = advEdit.Number;
            adInDB.Text = advEdit.Text;
            Context.Ads.Update(adInDB);
            await Context.SaveChangesAsync();

        }
        public async Task<AdsAndPagesCount> TryGetAdsListAndPgCount(GetAllAdsArgs args) 
        {
            int? ratingHigh = args.RatingHigh;
            int? ratingLow = args.RatingLow;
            string keyWord = args.KeyWord;
            bool isASC = args.IsASC;
            SortCriteria criterion = args.Criterion;
            int pageNumber = args.Page;
            int pageSize = args.PageSize;

            if (ratingHigh.HasValue && ratingLow.HasValue && ratingLow > ratingHigh)
                (ratingLow, ratingHigh) = (ratingHigh, ratingLow);
            IQueryable<Advertisement> ads;
            List<Advertisement> adsList;
            int pagesCount = 0;
            switch (criterion)
            {
                case SortCriteria.Rating:
                    ads = (isASC ? Context.Ads.OrderBy(a => a.Rating) : Context.Ads.OrderByDescending(a => a.Rating));
                    break;
                default:
                    ads = (isASC ? Context.Ads.OrderBy(a => a.CreationDate) : Context.Ads.OrderByDescending(a => a.CreationDate));
                    break;
            }
            if (keyWord != null)
                ads = ads.Where(a => EF.Functions.Like(a.Text, $"%{keyWord}%"));
            if (ratingLow.HasValue)
            {
                ads = ads.Where(a => a.Rating >= ratingLow);
            }
            if (ratingHigh.HasValue)
            {
                ads = ads.Where(a => a.Rating <= ratingHigh);
            }
            pagesCount = await ads.CountAsync();
            if (pagesCount > 0)
                pagesCount = pagesCount / pageSize + ((pagesCount % pageSize) == 0 ? 0 : 1);
            else
                pagesCount = 1;
            if (pageNumber > pagesCount || pageNumber < 1)
                throw new InvalidPageException();
            ads = ads.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            adsList = await ads.ToListAsync();

            return new AdsAndPagesCount { Ads = adsList, PagesCount = pagesCount };
        }
        public async Task<List<Advertisement>> TryGetPersonalAdsList(Guid guid)
        {
            List<Advertisement> adsList;
            if (await Context.UserProfiles.FirstOrDefaultAsync(u => u.Id == guid) == null)
                throw new DoesNotExistException(typeof(UserProfile));
            adsList = await Context.Ads.Where(a => a.UserId == guid).ToListAsync();

            return adsList;
        }
        public async Task TryChangeRating(Guid guid, RatingChange change) 
        {
            Advertisement? ad = await Context.Ads.FirstOrDefaultAsync(a => a.Id == guid);
            if (ad == null)
                throw new DoesNotExistException(typeof(Advertisement));
            switch (change)
            {
                case RatingChange.up:
                    ad.Rating++;
                    break;
                default:
                    ad.Rating--;
                    break;
            }
            Context.Ads.Update(ad);
            await Context.SaveChangesAsync();
        }        
    }
}
