namespace LinqVec.Tools.Curve_.Model;

public sealed record CurvePt(
    Pt P,
    Pt HLeft,
    Pt HRight
)
{
    public static CurvePt MakeClick(Pt p) => new(p, p, p);

    public CurvePt UpdateHandles(Pt hRight)
    {
        var u = hRight - P;
        return new(
            P,
            P - u,
            P + u
        );
    }

    public bool HasHandles => HLeft != P || HRight != P;
}