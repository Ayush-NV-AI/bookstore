using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Features.Orders;

/// <summary>Thin HTTP adapter for the order paths in contracts/orders-cart.openapi.yaml.</summary>
[ApiController]
[Route("api/v1/orders")]
public sealed class OrdersController(IOrderService orders) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderView), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderView>> Place([FromBody] PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        // [Required] guarantees both values are present once model validation has passed.
        var order = await orders.PlaceAsync(request.CartId!.Value, request.CustomerEmail!, cancellationToken);
        return CreatedAtAction(nameof(Get), new { orderId = order.Id }, order);
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderView), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderView>> Get(Guid orderId, CancellationToken cancellationToken) =>
        Ok(await orders.GetAsync(orderId, cancellationToken));

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderView>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<OrderView>>> List(
        [FromQuery] OrderLookupQuery query, CancellationToken cancellationToken) =>
        Ok(await orders.ListByEmailAsync(query.CustomerEmail!, cancellationToken));
}
