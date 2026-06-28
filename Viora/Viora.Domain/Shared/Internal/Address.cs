namespace Viora.Domain.Shared.Internal;

/// <summary>
/// The address for the Branch, Country Id is allowed here because maybe branches could be in many countries
/// </summary>
/// <param name="Number"></param>
/// <param name="Street"></param>
/// <param name="City"></param>
/// <param name="State"></param>
/// <param name="CountryId"></param>
/// <param name="PostalCode"></param>
public record Address(int Number, string Street, string City, string State, Guid CountryId, int PostalCode)
{
    public string Value => $"{Number} {Street}, {City}, {State}";
}