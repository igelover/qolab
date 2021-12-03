﻿using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Qolab.API.Data;
using Qolab.API.Entities;
using Qolab.API.Models;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;

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

        private (int? publishYear, int? publishMonth, int? publishDay) GetPublishDateInfo(string? publishDate)
        {
            int publishYear = 0;
            int publishMonth = 0;
            int publishDay = 0;
            var publishDateParts = publishDate?.Split('-');
            if (publishDateParts?.Length > 0)
            {
                _ = int.TryParse(publishDateParts[0], out publishYear);
            }
            if (publishDateParts?.Length > 1)
            {
                _ = int.TryParse(publishDateParts[1], out publishMonth);
            }
            if (publishDateParts?.Length > 2)
            {
                _ = int.TryParse(publishDateParts[2], out publishDay);
            }

            return (publishYear > 0 ? publishYear: null, publishMonth > 0 ? publishMonth : null, publishDay > 0 ? publishDay : null);
        }

        public async Task<PaperDto> CreatePaperAsync(PaperDto paperDto)
        {
            var (publishYear, publishMonth, publishDay) = GetPublishDateInfo(paperDto.PublishDate);
            
            var paper = new Paper()
            {
                Title = paperDto.Title,
                Abstract = paperDto.Abstract,
                Authors = string.Join('¦', paperDto.Authors),
                PublishYear = publishYear,
                PublishMonth = publishMonth,
                PublishDay = publishDay,
                Url = paperDto.Url,
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

            var (publishYear, publishMonth, publishDay) = GetPublishDateInfo(paperDto.PublishDate);

            paper.Title = paperDto.Title;
            paper.Abstract = paperDto.Abstract;
            paper.Authors = string.Join('¦', paperDto.Authors);
            paper.PublishYear = publishYear;
            paper.PublishMonth = publishMonth;
            paper.PublishDay = publishDay;
            paper.Url = paperDto.Url;

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

        public async Task<PaperDto?> ImportFromArxivAsync(string url)
        {
            var existingPaper = _context.Papers
                                        .Include(paper => paper.CreatedBy)
                                        .FirstOrDefault(p => p.Url == url);

            var parts = url.Split('/');
            var arxivUrl = $"http://export.arxiv.org/api/query?id_list={parts[parts.Length - 1]}";
            var client = new HttpClient();
            var response = await client.GetStringAsync(arxivUrl);
            var xmlReader = XmlReader.Create(new StringReader(response));
            var feed = SyndicationFeed.Load(xmlReader);

            if (feed.Items.Any())
            {
                var item = feed.Items.First();
                var newPaper = new PaperDto
                {
                    Title = item.Title.Text,
                    Authors = item.Authors.Select(a => a.Name),
                    Abstract = item.Summary.Text,
                    PublishDate = item.PublishDate.ToString("yyyy-MM-dd"),
                    Url = url,
                    CreatedById = Guid.Parse("00000000-0000-0000-0000-000000000001")
                };

                if (existingPaper is null)
                {
                    return await CreatePaperAsync(newPaper);
                }
                else
                {
                    newPaper.Id = existingPaper.Id;
                    return await UpdatePaperAsync(newPaper);
                }
            }
            return null;
        }
    }
}
