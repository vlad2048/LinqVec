using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using PowRxVar;

namespace LinqVec.Tools.Curve_.Model;

public sealed class CurveModel : IDisposable
{
    private readonly Disp d = new();
    public void Dispose() => d.Dispose();

    private readonly ISubject<Unit> whenChanged;

    public List<CurvePt> Points { get; } = new();
    public IObservable<Unit> WhenChanged => whenChanged.AsObservable();

    public CurveModel()
    {
        whenChanged = new Subject<Unit>().D(d);
    }

    public void AddClickPoint(Pt p)
    {
        Points.Add(CurvePt.MakeClick(p));
        whenChanged.OnNext(Unit.Default);
    }

    public void UpdateHandles(Pt hRight)
    {
        if (Points.Count == 0) throw new ArgumentException();
        var idx = Points.Count - 1;
        Points[idx] = Points[idx].UpdateHandles(hRight);
        whenChanged.OnNext(Unit.Default);
    }
}