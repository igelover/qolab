using Microsoft.EntityFrameworkCore;
using Qolab.API.Entities;

namespace Qolab.API.Data
{
    public static class MigrationManager
    {
        public static WebApplication MigrateDatabase(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                using (var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>())
                {
                    try
                    {
                        dataContext.Database.Migrate();
                        if (!dataContext.Users.Any())
                        {
                            Seed(dataContext);
                        }
                    }
                    catch (Exception ex)
                    {
                        app.Logger.LogError("Error while migrating and seeding the DB.", ex);
                        throw;
                    }
                }
            }
            return app;
        }

        public static void Seed(DataContext dataContext)
        {
            var admin = new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Username = "admin" };
            var alice = new User { Id = Guid.Parse("8509b08d-d290-4e1f-95e8-f47c8ad2828e"), Username = "alice" };
            var bob = new User { Id = Guid.Parse("2e1ab555-0a05-4921-adbd-0d01d1a0340f"), Username = "bob" };
            var charlie = new User { Id = Guid.Parse("37889ca9-0bfc-426b-832e-5a9ed0edf98f"), Username = "charlie" };
            var eve = new User { Id = Guid.Parse("c747437e-16eb-4ad4-b68f-4915875f3b72"), Username = "eve" };

            dataContext.Users.AddRange(new List<User> { admin, alice, bob, charlie, eve });

            var paperId = Guid.Parse("1601da1c-f7d9-4a18-acf6-eda0289a3bb3");
            var paper = new Paper
            {
                Id = paperId,
                Title = "Test Paper",
                Authors = "John Doe, Groucho Marx University",
                Abstract = "Ullamcorper a lacus vestibulum sed arcu non. Volutpat consequat mauris nunc congue nisi vitae. Eget magna fermentum iaculis eu non diam phasellus vestibulum lorem. A pellentesque sit amet porttitor eget dolor. Et molestie ac feugiat sed lectus vestibulum mattis. Feugiat scelerisque varius morbi enim nunc. Nibh venenatis cras sed felis eget velit aliquet sagittis id. Magna fringilla urna porttitor rhoncus dolor purus non. Eu tincidunt tortor aliquam nulla facilisi. Felis eget velit aliquet sagittis id consectetur purus ut faucibus. Sit amet massa vitae tortor condimentum lacinia quis vel. Netus et malesuada fames ac turpis egestas integer. Tellus in metus vulputate eu scelerisque. Ut diam quam nulla porttitor massa id neque aliquam vestibulum.",
                PublishYear = 2021,
                PublishMonth = 12,
                Url = "https://gitlab.com/qworld/qjam/2021/-/issues/10",
                CreatedBy = charlie
            };

            dataContext.Papers.Add(paper);

            var articleId = Guid.Parse("95015b9f-1556-4df8-94be-6e4a94452196");
            var comment = new Comment
            {
                Content = "Sed ut perspiciatis, unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam eaque ipsa, quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt, explicabo",
                Likes = 7,
                Dislikes = 1,
                CreatedBy = eve
            };
            var reply = new Comment
            {
                Content = "Nemo enim ipsam voluptatem, quia voluptas sit, aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos, qui ratione voluptatem sequi nesciunt, neque porro quisquam est, qui dolorem ipsum, quia dolor sit amet consectetur adipisci[ng] velit, sed quia non numquam [do] eius modi tempora inci[di]dunt, ut labore et dolore magnam aliquam quaerat voluptatem.",
                Likes = 5,
                Dislikes = 0,
                ReplyToCommentId = comment.Id,
                CreatedBy = bob
            };
            var question = new Question
            {
                Content = "Mattis nunc sed blandit libero volutpat sed?",
                Likes = 3,
                Dislikes = 0,
                CreatedBy = charlie,
                Answers = new List<Answer>
                {
                    new Answer
                    {
                        Content = "Hac habitasse platea dictumst vestibulum rhoncus est pellentesque. Iaculis eu non diam phasellus vestibulum lorem sed risus ultricies.",
                        IsAcceptedAnswer = true,
                        Likes = 1,
                        Dislikes = 1,
                        ArticleId = articleId,
                        CreatedBy = eve
                    }
                }
            };

            var article1 = new Article()
            {
                Id = articleId,
                Title = "Test Article",
                Summary = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                Tags = "test¦markdown",
                Content = File.ReadAllText(@"./Data/Seed/Article1.txt"),
                Likes = 123,
                Dislikes = 1,
                Comments = new List<Comment> { comment, reply},
                Questions = new List<Question> { question },
                Paper = paper,
                CreatedBy = alice
            };

            var article2 = new Article()
            {
                Title = "Vectors: One Dimensional lists",
                Summary = "Vectors are very useful to describe the state of a system, as we will see in the main tutorial.",
                Tags = "math¦vectors¦python",
                Content = File.ReadAllText(@"./Data/Seed/Article2.txt"),
                CreatedBy = bob
            };

            dataContext.Articles.AddRange(article1, article2);

            dataContext.SaveChanges();
        }
    }
}
