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
                        if (!dataContext.Articles.Any())
                        {
                            Seed(dataContext);
                        }
                    }
                    catch (Exception ex)
                    {
                        //Log error
                        throw;
                    }
                }
            }
            return app;
        }

        public static void Seed(DataContext dataContext)
        {
            var articleId = Guid.NewGuid();
            var comment = new Comment
            {
                Content = "Sed ut perspiciatis, unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam eaque ipsa, quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt, explicabo",
                Likes = 7,
                Dislikes = 1
            };
            var reply = new Comment
            {
                Content = "Nemo enim ipsam voluptatem, quia voluptas sit, aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos, qui ratione voluptatem sequi nesciunt, neque porro quisquam est, qui dolorem ipsum, quia dolor sit amet consectetur adipisci[ng] velit, sed quia non numquam [do] eius modi tempora inci[di]dunt, ut labore et dolore magnam aliquam quaerat voluptatem.",
                Likes = 5,
                Dislikes = 0,
                ReplyToComentId = comment.Id
            };
            var question = new Question
            {
                Content = "Mattis nunc sed blandit libero volutpat sed?",
                Likes = 3,
                Dislikes = 0,
                Answers = new List<Answer>
                {
                    new Answer
                    {
                        Content = "Hac habitasse platea dictumst vestibulum rhoncus est pellentesque. Iaculis eu non diam phasellus vestibulum lorem sed risus ultricies.",
                        IsAcceptedAnswer = true,
                        Likes = 1,
                        Dislikes = 1,
                        ArticleId = articleId
                    }
                }
            };

            var article = new Article
            {
                Id = articleId,
                Title = "Test Article",
                Summary = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                Keywords = "test;markdown",
                Content = File.ReadAllText(@".\Data\MarkdownSample.txt"),
                Likes = 123,
                Dislikes = 1,
                Comments = new List<Comment> { comment, reply},
                Questions = new List<Question> { question },

            };

            dataContext.Articles.Add(article);

            dataContext.SaveChanges();
        }
    }
}
