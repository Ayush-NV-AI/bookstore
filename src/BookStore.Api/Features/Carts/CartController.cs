using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Features.Carts;

/// <summary>Thin HTTP adapter for the cart paths in contracts/orders-cart.openapi.yaml.</summary>
[ApiController]
[Route("api/v1/carts")]
public sealed class CartController(ICartService carts) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CartView), StatusCodes.Status201Created)]
    public async Task<ActionResult<CartView>> Create(CancellationToken cancellationToken)
    {
        var cart = await carts.CreateAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { cartId = cart.Id }, cart);
    }

    [HttpGet("{cartId:guid}")]
    [ProducesResponseType(typeof(CartView), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartView>> Get(Guid cartId, CancellationToken cancellationToken) =>
        Ok(await carts.GetAsync(cartId, cancellationToken));

    [HttpPut("{cartId:guid}/items/{bookId:int}")]
    [ProducesResponseType(typeof(CartView), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CartView>> SetQuantity(
        Guid cartId, int bookId, [FromBody] SetQuantityRequest request, CancellationToken cancellationToken) =>
        Ok(await carts.SetQuantityAsync(cartId, bookId, request.Quantity, cancellationToken));

    [HttpDelete("{cartId:guid}/items/{bookId:int}")]
    [ProducesResponseType(typeof(CartView), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartView>> Remove(Guid cartId, int bookId, CancellationToken cancellationToken) =>
        Ok(await carts.RemoveAsync(cartId, bookId, cancellationToken));
}
