namespace BookStore.Api.Infrastructure;

/// <summary>Base for business-rule failures that map to a Problem Details response (research 002 R2).</summary>
public abstract class DomainException(string message) : Exception(message);

/// <summary>Maps to 404 — the cart, book or order does not exist.</summary>
public sealed class NotFoundException(string message) : DomainException(message);

/// <summary>Maps to 409 — the request conflicts with current state (e.g. not enough stock).</summary>
public sealed class ConflictException(string message) : DomainException(message);

/// <summary>Maps to 400 — the request is well-formed but breaks a business rule (e.g. empty cart).</summary>
public sealed class DomainValidationException(string message) : DomainException(message);
