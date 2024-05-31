using Microsoft.EntityFrameworkCore;
namespace Repos
{
    public partial class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }
        public ApplicationContext()
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfile>().ToTable("user_profiles");
            
            modelBuilder.Entity<UserProfile>().Property(p => p.Id).HasColumnName("_user_id");
            modelBuilder.Entity<UserProfile>().Property(p => p.Name).HasColumnName("_user_name").IsRequired().HasColumnType("varchar(16)");
            modelBuilder.Entity<UserProfile>().Property(p => p.IsAdmin).HasColumnName("is_admin").IsRequired(); ;
            
            modelBuilder.Entity<UserProfile>().HasKey(p => p.Id);
            modelBuilder.Entity<UserProfile>().HasIndex(p => p.Id).IsUnique();
            modelBuilder.Entity<UserProfile>().HasMany(p => p.Ads).WithOne(a => a.User).HasForeignKey(a => a.UserId);


            modelBuilder.Entity<Advertisement>().ToTable("ads");
            
            modelBuilder.Entity<Advertisement>().Property(a => a.Id).HasColumnName("ad_id").IsRequired();
            modelBuilder.Entity<Advertisement>().Property(a => a.Number).HasColumnName("phone_number").IsRequired();
            modelBuilder.Entity<Advertisement>().Property(a => a.UserId).HasColumnName("_user_id").IsRequired();
            modelBuilder.Entity<Advertisement>().Property(a => a.Text).HasColumnName("ad_text").HasMaxLength(512).IsRequired();
            modelBuilder.Entity<Advertisement>().Property(a => a.PicLink).HasColumnName("pic_link").HasDefaultValue("Empty").HasMaxLength(128);
            modelBuilder.Entity<Advertisement>().Property(a => a.Rating).HasColumnName("rating").HasDefaultValue(0);
            modelBuilder.Entity<Advertisement>().Property(a => a.CreationDate).HasColumnName("creation_date").IsRequired();
            modelBuilder.Entity<Advertisement>().Property(a => a.DeletionDate).HasColumnName("deletion_date").IsRequired();

            modelBuilder.Entity<Advertisement>().HasKey(a => a.Id);
            modelBuilder.Entity<Advertisement>().HasIndex(a => a.Id).IsUnique();


            modelBuilder.Entity<User>().ToTable("users");

            modelBuilder.Entity<User>().Property(u => u.Id).HasColumnName("_user_id");
            modelBuilder.Entity<User>().Property(u => u.Login).HasColumnName("user_login").IsRequired().HasColumnType("varchar(16)");
            modelBuilder.Entity<User>().Property(u => u.Password).HasColumnName("user_password").IsRequired().HasColumnType("varchar(16)");
            
            modelBuilder.Entity<User>().HasOne(u => u.Profile).WithOne(p => p._User).HasForeignKey<UserProfile>(p => p.Id);
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Password).IsUnique();


            base.OnModelCreating(modelBuilder);
        }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Advertisement> Ads { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
