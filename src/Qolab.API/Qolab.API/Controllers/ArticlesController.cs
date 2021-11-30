using Microsoft.AspNetCore.Mvc;
using Qolab.API.Managers;
using Qolab.API.Models;

namespace Qolab.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly ArticlesManager _manager;

        public ArticlesController(ArticlesManager manager)
        {
            _manager = manager;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleDto>> GetArticle(Guid id)
        {
            var article = await _manager.GetArticleAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            return article;
        }
    }
}
