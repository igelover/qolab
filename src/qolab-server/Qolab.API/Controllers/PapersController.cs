using Microsoft.AspNetCore.Mvc;
using Qolab.API.Managers;
using Qolab.API.Models;

namespace Qolab.API.Controllers
{
    /// <summary>
    /// Papers controller, handles all operations related to papers and its properties
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PapersController : ControllerBase
    {
        private readonly PapersManager _manager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="manager">The <see cref="PapersManager"/> that tals to the DB</param>
        public PapersController(PapersManager manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Gets a paper by its internal ID
        /// </summary>
        /// <param name="id">The paper internal ID</param>
        /// <returns>The full paper object</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<PaperDto>> GetPaper([FromRoute] Guid id)
        {
            var paper = await _manager.GetPaperAsync(id);

            if (paper is null)
            {
                return NotFound();
            }

            return paper;
        }

        /// <summary>
        /// Performs a Full Text Search on the paper Title, Abstract and Authors
        /// </summary>
        /// <param name="searchTerm">The term to search</param>
        /// <returns>A collection of papers matching the search criteria</returns>
        /// <remarks><a href="https://www.compose.com/articles/mastering-postgresql-tools-full-text-search-and-phrase-search/">Full Text Search reference</a></remarks>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaperDto>>> SearchPapers([FromQuery] string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return BadRequest();
            }

            var result = await _manager.SearchPapersAsync(searchTerm);
            return Ok(result);
        }

        /// <summary>
        /// Createas a new paper entry on the DB
        /// </summary>
        /// <param name="paper">The paper to create</param>
        /// <returns>The paper created</returns>
        [HttpPost]
        public async Task<ActionResult<PaperDto>> CreatePaper([FromBody] PaperDto paper)
        {
            var result = await _manager.CreatePaperAsync(paper);
            return CreatedAtAction(nameof(GetPaper), new { id = result.Id }, result);
        }

        /// <summary>
        /// Updates Title, Abstract and Authors of an existing paper
        /// </summary>
        /// <param name="id">The paper internal ID</param>
        /// <param name="paper">The paper to update</param>
        /// <returns>The updated paper object</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<PaperDto>> UpdatePaper([FromRoute] Guid id, [FromBody] PaperDto paper)
        {
            if (id != paper.Id)
            {
                return BadRequest();
            }

            var result = await _manager.UpdatePaperAsync(paper);
            return result is null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Deletes an existing paper
        /// </summary>
        /// <param name="id">The paper internal ID</param>
        /// <returns>An HTTP 204 No Content code</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaper([FromRoute] Guid id)
        {
            // TODO: Add validation, only admin can delete papers
            var result = await _manager.DeletePaperAsync(id);
            return result is null ? NotFound() : NoContent();
        }
    }
}
