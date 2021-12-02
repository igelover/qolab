using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Qolab.API.Data;
using Qolab.API.Entities;
using Qolab.API.Models;
using System.Text.RegularExpressions;
using static Qolab.API.Models.Enums;

namespace Qolab.API.Managers
{
    public class ArticlesManager
    {
        private readonly ILogger<ArticlesManager> _logger;
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;

        public ArticlesManager(ILogger<ArticlesManager> logger, IConfiguration configuration, DataContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public async Task<ArticleDto?> GetArticleAsync(Guid id)
        {
            var article = await _context.Articles
                                  .Include(article => article.Paper)
                                  .Include(article => article.CreatedBy)
                                  .Include(article => article.Comments)
                                      .ThenInclude(comment => comment.CreatedBy)
                                  .Include(article => article.Questions)
                                  .ThenInclude(question => question.CreatedBy)
                                  .Include(article => article.Questions)
                                      .ThenInclude(question => question.Answers)
                                      .ThenInclude(answer => answer.CreatedBy)
                                  .FirstOrDefaultAsync(x => x.Id == id);

            return article?.ToDto();
        }

        public async Task<IEnumerable<ArticleShortDto>> SearchArticlesAsync(string searchTerm)
        {
            var query = @"SELECT a.id, a.title, a.summary, a.tags, a.likes, a.dislikes, a.last_updated,
                          p.id, p.title, p.authors, p.publish_year, p.publish_month, p.publish_day, p.url, p.doi,
                          u.id, u.username
                          FROM articles as a
                          INNER JOIN users as u on a.created_by_id = u.id
                          LEFT JOIN papers as p on a.paper_id = p.id
                          WHERE to_tsvector(a.title) @@ to_tsquery(@searchTerms)
                          OR to_tsvector(a.summary) @@ to_tsquery(@searchTerms)
                          OR to_tsvector(a.tags) @@ to_tsquery(@searchTerms)
                          OR to_tsvector(a.content) @@ to_tsquery(@searchTerms);";

            var searchTerms = searchTerm;
            if (!searchTerm.Contains(" | ") && !searchTerm.Contains(" & ")
                && !searchTerm.Contains(":*") && !Regex.IsMatch(searchTerm, @"<?>"))
            {
                searchTerms = string.Join(" | ", searchTerm.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
            }

            using var connection = new NpgsqlConnection(_configuration.GetConnectionString("QolabDb"));
            var result = await connection.QueryAsync<Article, Paper, User, Article>(query,
                (article, paper, user) =>
                {
                    article.Paper = paper;
                    article.CreatedBy = user;
                    return article;
                },
                new
                {
                    searchTerms
                });

            return result.Select(article => article.ToShortDto());
        }

        public async Task<ArticleDto> CreateArticleAsync(ArticleDto articleDto)
        {
            var article = new Article()
            {
                Title = articleDto.Title,
                Summary = articleDto.Summary,
                Tags = string.Join('¦', articleDto.Tags),
                Content = articleDto.Content,
                CreatedById = Guid.Parse(articleDto.CreatedBy)
            };
            var entry = await _context.Articles.AddAsync(article);
            await _context.SaveChangesAsync();
            var newArticle = await _context.Articles
                                           .Include(article => article.CreatedBy)
                                           .FirstAsync(article => article.Id == entry.Entity.Id);
            return newArticle.ToDto();
        }

        public async Task<Article?> UpdateArticleAsync(ArticleDto articleDto)
        {
            var article = _context.Articles.FirstOrDefault(article => article.Id == articleDto.Id);
            if (article is not null)
            {
                article.Title = articleDto.Title;
                article.Summary = articleDto.Summary;
                article.Tags = string.Join('¦', articleDto.Tags);
                article.Content = articleDto.Content;

                _context.Entry(article).State = EntityState.Modified;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError("Error while updating the article", ex);
                    throw;
                }
            }
            return article;
        }

        public async Task<Article?> DeleteArticleAsync(Guid id)
        {
            var article = _context.Articles.FirstOrDefault(article => article.Id == id);
            if (article is not null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
            }
            return article;
        }

        public async Task<Article?> VoteArticleAsync(Guid id, Vote vote)
        {
            var article = _context.Articles.FirstOrDefault(article => article.Id == id);
            if (article is not null)
            {
                _context.Entry(article).State = EntityState.Modified;
                try
                {
                    switch (vote)
                    {
                        case Vote.UpVote:
                            article.Likes += 1;
                            break;
                        case Vote.DownVote:
                            article.Dislikes += 1;
                            break;
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError($"Error while {(vote == Vote.UpVote ? "up-voting" : "down-voting")} the article", ex);
                    throw;
                }
            }
            return article;
        }
    }
}
