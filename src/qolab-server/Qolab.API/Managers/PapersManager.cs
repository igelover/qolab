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
    public class PapersManager
    {
        private readonly ILogger<PapersManager> _logger;
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;

        public PapersManager(ILogger<PapersManager> logger, IConfiguration configuration, DataContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public async Task<PaperDto?> GetPaperAsync(Guid id)
        {
            var paper = await _context.Papers
                                        .Include(paper => paper.CreatedBy)
                                        .FirstOrDefaultAsync(x => x.Id == id);

            return paper?.ToDto();
        }

        public async Task<IEnumerable<PaperDto>> SearchPapersAsync(string searchTerm)
        {
            var query = @"SELECT p.id, p.title, p.authors, p.publish_year, p.publish_month, p.publish_day, p.url, p.doi,
                          up.id, up.username
                          FROM papers as p
                          LEFT JOIN users as up on p.created_by_id = up.id
                          WHERE to_tsvector(p.title) @@ to_tsquery(@searchTerms)
                          OR to_tsvector(p.abstract) @@ to_tsquery(@searchTerms)
                          OR to_tsvector(p.authors) @@ to_tsquery(@searchTerms);";

            var searchTerms = searchTerm;
            if (!searchTerm.Contains(" | ") && !searchTerm.Contains(" & ")
                && !searchTerm.Contains(":*") && !Regex.IsMatch(searchTerm, @"<?>"))
            {
                searchTerms = string.Join(" | ", searchTerm.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
            }

            using var connection = new NpgsqlConnection(_configuration.GetConnectionString("QolabDb"));
            var result = await connection.QueryAsync<Paper, User, Paper>(query,
                (paper, paperUser) =>
                {
                    paper.CreatedBy = paperUser;
                    return paper;
                },
                new
                {
                    searchTerms
                });

            return result.Select(paper => paper.ToDto());
        }

        public async Task<PaperDto> CreatePaperAsync(PaperDto paperDto)
        {
            var paper = new Paper()
            {
                Title = paperDto.Title,
                Abstract = paperDto.Abstract,
                Authors = string.Join('¦', paperDto.Authors),
                CreatedById = paperDto.CreatedById
            };
            var entry = await _context.Papers.AddAsync(paper);
            await _context.SaveChangesAsync();
            var newPaper = await _context.Papers
                                           .Include(paper => paper.CreatedBy)
                                           .FirstAsync(paper => paper.Id == entry.Entity.Id);
            return newPaper.ToDto();
        }

        public async Task<PaperDto?> UpdatePaperAsync(PaperDto paperDto)
        {
            var paper = _context.Papers.FirstOrDefault(paper => paper.Id == paperDto.Id);
            if (paper == null) return null;

            paper.Title = paperDto.Title;
            paper.Abstract = paperDto.Abstract;
            paper.Authors = string.Join('¦', paperDto.Authors);

            try
            {
                _context.Entry(paper).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return await GetPaperAsync(paperDto.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error while updating paper {paperId}", paperDto.Id);
                throw;
            }
        }

        public async Task<Paper?> DeletePaperAsync(Guid id)
        {
            var paper = _context.Papers.FirstOrDefault(paper => paper.Id == id);
            if (paper == null) return null;

            try
            {
                _context.Papers.Remove(paper);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting paper {paperId}", id);
                throw;
            }
            return paper;
        }
    }
}
