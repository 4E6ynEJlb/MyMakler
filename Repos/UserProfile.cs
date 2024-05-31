using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
namespace Repos
{
    /// <summary>
    /// Модель пользователя в бд
    /// </summary>
    public class UserProfile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
        public User _User { get; set; }
        public ICollection<Advertisement> Ads { get; set; }
        public UserProfile()
        {
            Ads = new List<Advertisement>();
        }
    }
}
