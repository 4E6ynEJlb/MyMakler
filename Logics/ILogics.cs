using Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LogicsLib.Logics;

namespace LogicsLib
{
    public interface ILogics
    {
        /// <summary>
        /// Инициализация бд тестовыми значениями
        /// </summary>
        /// <returns></returns>
        Task InitializeDb();
        /// <summary>
        /// Проверка наличия логина и пароля в бд и возврат ИД пользователя
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        Task<Guid> TryLogin(LoginModel loginModel);
        /// <summary>
        /// Регистрация пользователя
        /// </summary>
        /// <param name="registerModel">Данные пользователя</param>
        /// <returns></returns>
        Task<Guid> TryRegisterUser(RegisterModel registerModel);
        /// <summary>
        /// Добавление проофиля пользователя
        /// </summary>
        /// <param name="userProfile>Профиль пользователя</param>
        /// <returns></returns>
        Task TryAddUserProfile(ProfileInput profileInput);
        /// <summary>
        /// Поиск пользователя по имени (LIKE)
        /// </summary>
        /// <param name="name">Часть имени</param>
        /// <returns></returns>
        Task<List<UserProfile>> TrySearchUserProfile(string name);
        /// <summary>
        /// Список пользователей
        /// </summary>
        /// <param name="pageNumber">Номер страницы</param>
        /// <param name="pageSize">Количество пользователей на странице</param>
        /// <returns></returns>
        Task<List<UserProfile>> TryGetUserProfilesList(int pageNumber, int pageSize);
        /// <summary>
        /// Количество страниц при заданном размере
        /// </summary>
        /// <param name="pageSize">Количество пользователей на странице</param>
        /// <returns></returns>
        Task<int> TryGetUserProfilesPagesCount(int pageSize);
        /// <summary>
        /// Удаление профиля пользователя по ИД
        /// </summary>
        /// <param name="guid">ИД</param>
        /// <returns></returns>
        Task TryDeleteUserProfile(Guid guid);
        /// <summary>
        /// Изменение пользователя
        /// </summary>
        /// <param name="profileInput">Обновленный пользовательсккий профиль</param>
        /// <returns></returns>
        Task TryEditUserProfile(ProfileInput profileInput);
        /// <summary>
        /// Добавление объявления
        /// </summary>
        /// <param name="adInput">Информация об объявлении</param>
        /// <returns></returns>
        Task<Guid> TryAddAdvertisement(AdvInput adInput);
        /// <summary>
        /// Прикрепление фото к объявлению
        /// </summary>
        /// <param name="file">Фото</param>
        /// <param name="adId">ИД объявления</param>
        /// <returns></returns>
        Task TryAttachPic(IFormFile file, Guid adId);
        /// <summary>
        /// Удаление картинки из объявления
        /// </summary>
        /// <param name="adId">ИД объявления</param>
        /// <returns></returns>
        Task TryDetachPic(Guid adId);
        /// <summary>
        /// Удаление объявления
        /// </summary>
        /// <param name="guid">ИД объявления</param>
        /// <returns></returns>
        Task TryDeleteAdvertisement(Guid guid);
        /// <summary>
        /// Редактирование объявления 
        /// </summary>
        /// <param name="advEdit">Обновленное объявление</param>
        /// <returns></returns>
        Task TryEditAdvertisement(AdvEdit advEdit);
        /// <summary>
        /// Сортированный список объявлений с необязательным поиском по тексту и фильтром по рейтингу
        /// </summary>
        /// <param name="args">Аргументы для запроса</param>
        /// <returns></returns>
        Task<AdsAndPagesCount> TryGetAdsListAndPgCount(GetAllAdsArgs args);
        /// <summary>
        /// Поиск объявлений конкретного пользователя
        /// </summary>
        /// <param name="guid">ИД пользователя</param>
        /// <returns></returns>
        Task<List<Advertisement>> TryGetPersonalAdsList(Guid guid);
        /// <summary>
        /// Изменение рейтинга объявления на 1
        /// </summary>
        /// <param name="guid">ИД объявления</param>
        /// <param name="change">+/-</param>
        /// <returns></returns>
        Task TryChangeRating(Guid guid, RatingChange change);
        /// <summary>
        /// См. DetachedPicsService
        /// </summary>
        /// <param name="token"></param>
        void DeleteDetachedPics(CancellationToken token);
        /// <summary>
        /// См. OldAdsService
        /// </summary>
        /// <param name="token"></param>
        void RemoveOldAds(CancellationToken token);
    }
}
