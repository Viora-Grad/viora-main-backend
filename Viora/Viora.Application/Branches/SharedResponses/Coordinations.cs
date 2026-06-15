using NetTopologySuite.Geometries;

namespace Viora.Application.Branches.SharedResponses;

public sealed record Coordinations(Point Point)
{
    public readonly double Longitude = Point.X;
    public readonly double Latitude = Point.Y;
}
