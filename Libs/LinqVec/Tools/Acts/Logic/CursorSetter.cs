using System.Reactive.Linq;
using LinqVec.Tools.Acts.Structs;

namespace LinqVec.Tools.Acts.Logic;

static class CursorSetter
{
	public static IDisposable SetCursor(this IObservable<Option<HotAct>> curHot, Action<Cursor?> setCursor) =>
		curHot
			.StartWith(None)
			.Buffer(2, 1)
			.Select(l => new
			{
				Prev = l[0],
				Next = l[1],
			})
			
			.Subscribe(t =>
			{
				if (t.Prev.IsSome && t.Next.IsNone)
					setCursor(Cursors.Default);
				else
					t.Next.IfSome(tNext => setCursor(tNext.Act.Hotspot.Cursor));
			});
}