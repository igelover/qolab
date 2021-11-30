using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Qolab.API.Entities;

namespace Qolab.API.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }


        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("QolabDb");
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                entityType.SetTableName(entityType.DisplayName());
            }

            Seed(modelBuilder);
        }

        private static void Seed(ModelBuilder modelBuilder)
        {
            var article = new Article
            {
                Title = "Test Article",
                Summary = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                Keywords = "test;markdown",
                Content = File.ReadAllText(@".\Data\MarkdownSample.txt"),
                Likes = 123,
                Dislikes = 1
            };
            modelBuilder.Entity<Article>().HasData(article);

            modelBuilder.Entity<Article>(
                entity =>
                {
                    entity.HasMany(a => a.Comments)
                        .WithOne(c => c.Article)
                        .HasForeignKey("ArticleId");
                });

            var comment = new Comment
            {
                Content = "Sed ut perspiciatis, unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam eaque ipsa, quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt, explicabo",
                Likes = 7,
                Dislikes = 1,
                ArticleId = article.Id
            };
            var reply = new Comment
            {
                Content = "Nemo enim ipsam voluptatem, quia voluptas sit, aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos, qui ratione voluptatem sequi nesciunt, neque porro quisquam est, qui dolorem ipsum, quia dolor sit amet consectetur adipisci[ng] velit, sed quia non numquam [do] eius modi tempora inci[di]dunt, ut labore et dolore magnam aliquam quaerat voluptatem.",
                Likes = 5,
                Dislikes = 0,
                ReplyToComentId = comment.Id,
                ArticleId = article.Id
            };
            modelBuilder.Entity<Comment>().HasData(comment);
            modelBuilder.Entity<Comment>().HasData(reply);

            modelBuilder.Entity<Article>(
                entity =>
                {
                    entity.HasMany(a => a.Questions)
                        .WithOne(q => q.Article)
                        .HasForeignKey("ArticleId");
                });

            var question = new Question
            {
                Content = "Mattis nunc sed blandit libero volutpat sed?",
                Likes = 3,
                Dislikes = 0,
                ArticleId = article.Id
            };
            modelBuilder.Entity<Question>().HasData(question);

            modelBuilder.Entity<Question>(
                entity =>
                {
                    entity.HasMany(q => q.Answers)
                        .WithOne(a => a.Question)
                        .HasForeignKey("QuestionId");
                });

            var answer = new Answer
            {
                Content = "Hac habitasse platea dictumst vestibulum rhoncus est pellentesque. Iaculis eu non diam phasellus vestibulum lorem sed risus ultricies.",
                IsAcceptedAnswer = true,
                Likes = 1,
                Dislikes = 1,
                ArticleId = article.Id,
                QuestionId = question.Id
            };
            modelBuilder.Entity<Answer>().HasData(answer);
        }
    }
}
