using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repos
{
    /// <summary>
    /// Модель объявления в бд
    /// </summary>
    public class Advertisement
    {
        public Guid Id { get; set; }
        public int Number { get; set; }//телефон
        public Guid UserId { get; set; }
        public UserProfile User { get; set; }
        public string Text { get; set; }
        public string PicLink { get; set; }//ссылка на картинку
        public int Rating { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DeletionDate { get; set; }
    }
}
