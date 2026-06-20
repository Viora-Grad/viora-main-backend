using NetTopologySuite.Geometries;

namespace Viora.Application.Branches.SharedResponses;

public sealed record Coordinates
{
    public Coordinates(Point point)
    {
        Longitude = point.X;
        Latitude = point.Y;
    }
    public double Longitude { set; get; }
    public double Latitude { set; get; }
}
