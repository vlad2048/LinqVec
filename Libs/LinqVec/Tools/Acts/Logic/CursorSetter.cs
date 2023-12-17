using System.Reactive.Linq;
using LinqVec.Tools.Acts.Structs;
using LinqVec.Utils.Rx;

namespace LinqVec.Tools.Acts.Logic;

static class CursorSetter
{
	public static IDisposable SetCursor(
		this IObservable<Option<HotAct>> curHot,
		Cursor actSetCursor,
		Action<Cursor?> setCursor
	)
	{
		//setCursor(actSetCursor);

		return curHot
			.StartWith(None)
			.Buffer(2, 1)
			.Select(l => new
			{
				Prev = l[0],
				Next = l[1],
			})
			.ObserveOnUI()
			.Subscribe(t =>
			{
				t.Next.IfSome(tNext => setCursor(tNext.Act.Hotspot.Cursor));
				t.Next.IfNone(() => setCursor(actSetCursor));

				/*
				if (t.Prev.IsSome && t.Next.IsNone)
					setCursor(Cursors.Default);
				else
					t.Next.IfSome(tNext => setCursor(tNext.Act.Hotspot.Cursor));
				*/
			});
	}
}