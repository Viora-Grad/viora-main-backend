using Viora.Domain.Abstractions;

namespace Viora.Domain.Shared;

public sealed class Country(Guid id, string name, string isoAlphaThree, string nationality) : Entity(id)
{
    public string Name { get; private set; } = name;
    /// <summary>
    /// Country code resembles the 3 characters of country like USA
    /// </summary>
    public string IsoAlphaThree { get; private set; } = isoAlphaThree;
    public string Nationality { get; private set; } = nationality;
}
