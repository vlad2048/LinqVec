using System.Text.Json.Serialization;
using Geom;
using LinqVec;

namespace VectorEditor._Model.Interfaces;

[JsonDerivedType(typeof(Curve), typeDiscriminator: "Curve")]
public interface IObj : IId
{
    [JsonIgnore]
    R BoundingBox { get; }
    double DistanceToPoint(Pt pt);
}