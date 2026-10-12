using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Features.Catalog;

/// <summary>Thin HTTP adapter for contracts/catalog.openapi.yaml (Constitution II, III).</summary>
[ApiController]
[Route("api/v1/catalog")]
public sealed class CatalogController(ICatalogService catalog) : ControllerBase
{
    [HttpGet("books")]
    [ProducesResponseType(typeof(PagedResult<BookSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<BookSummary>>> ListBooks(
        [FromQuery] CatalogQuery query, CancellationToken cancellationToken)
    {
        return Ok(await catalog.ListBooksAsync(query, cancellationToken));
    }

    [HttpGet("books/{id:int}")]
    [ProducesResponseType(typeof(BookDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDetail>> GetBook(int id, CancellationToken cancellationToken)
    {
        var book = await catalog.GetBookAsync(id, cancellationToken);
        if (book is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Book not found",
                detail: $"No book with id {id} exists.");
        }

        return Ok(book);
    }

    [HttpGet("genres")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> ListGenres(CancellationToken cancellationToken)
    {
        return Ok(await catalog.ListGenresAsync(cancellationToken));
    }
}
