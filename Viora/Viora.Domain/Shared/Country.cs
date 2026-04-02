using Viora.Domain.Abstractions;

namespace Viora.Domain.Shared;

// TODO follow up with a registery in the application layer to load the data from the database and register the container as a singleton service in the DI containter
public sealed class Country : Entity
{
    public string Name { get; private set; } = default!;
    /// <summary>
    /// Country code resembles the 3 characters of country like USA
    /// </summary>
    public string IsoAlphaThree { get; private set; } = default!;
    public string Nationality { get; private set; } = default!;
}
