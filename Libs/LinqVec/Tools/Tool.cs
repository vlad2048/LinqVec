using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using LinqVec.Logic;
using LinqVec.Tools.Events;
using LinqVec.Utils;
using LinqVec.Utils.WinForms_;
using PowMaybe;
using PowRxVar;

namespace LinqVec.Tools;


public enum Trigger
{
	Down,
	Up,
	Click,
	DownRight,
}

public interface IHot;

/*public sealed record Hot<H>(H V) : IHot
{
	//public static IHot Make(H v) => new Hot<H>(v);
}*/

public interface IHotspot
{
	string Name { get; }
	Cursor Cursor { get; }
	bool IsOver(Pt mousePos);
	Trigger Trigger { get; }
	Action<bool>? OnOver => null;
}

public interface IHotspot<H> : IHotspot
{
	Maybe<H> Get(Pt mousePos);
	bool IHotspot.IsOver(Pt mousePos) => Get(mousePos).IsSome();
}



public static class HotspotExt
{
	public static (Task<H>, IDisposable) Choose<H>(
		this Evt evt,
		params IHotspot<H>[] hotspots
	)
	{
		var d = new Disp();
		evt.EnableCursors(hotspots).D(d);
		var task = hotspots
			.Select(hotspot => evt.WhenTrigger(hotspot))
			.Merge()
			.Take(1)
			.TakeUntil(d.WhenDisposed)
			.ToTask();

		var taskDisp = TaskDisp(task, d);
			
		return (taskDisp, d);
	}

	private static async Task<H> TaskDisp<H>(Task<H> task, IDisposable d)
	{
		var res = await task;
		d.Dispose();
		return res;
	}


	private sealed class MappedHotspot<H>(
		string name,
		Cursor cursor,
		Trigger trigger,
		Action<bool>? onOver,
		Func<Pt, Maybe<H>> get
	) : IHotspot<H>
	{
		public string Name => name;
		public Cursor Cursor => cursor;
		public Trigger Trigger => trigger;
		public Maybe<H> Get(Pt mousePos) => get(mousePos);
		public Action<bool>? OnOver => onOver;
		public override string ToString() => Name;
	}

	public static IHotspot<U> Map<T, U>(this IHotspot<T> hotspot, Func<T, U> fun) => new MappedHotspot<U>(
		hotspot.Name,
		hotspot.Cursor,
		hotspot.Trigger,
		hotspot.OnOver,
		p => hotspot.Get(p).Select(fun)
	);

	public static IDisposable EnableCursors(this Evt evt, IHotspot[] hotspots)
	{
		var d = new Disp();

		var active = VarMay.Make<IHotspot>().D(d);

		//L.WriteLine("EnableCursors");
		//Disposable.Create(() => L.WriteLine("EnableCursors.Dispose")).D(d);
		//active.Log(d);

		active.Subscribe(mayHot =>
		{
			if (mayHot.IsSome(out var hot))
			{
				foreach (var h in hotspots) h.OnOver?.Invoke(h == hot);
			}
			else
			{
				foreach (var h in hotspots) h.OnOver?.Invoke(false);
			}
		}).D(d);

		evt.WhenEvt
			.Subscribe(e =>
			{
				var mp = e.GetMayMousePos();
				if (mp.IsNone(out var p)) return;
				foreach (var hotspot in hotspots)
				{
					if (hotspot.IsOver(p))
					{
						active.V = May.Some(hotspot);
						Obs.Timer(TimeSpan.Zero)
							.ObserveOnUI()
							.Subscribe(_ =>
							{
								evt.SetCursor(hotspot.Cursor);
							});
						break;
					}
				}
			}).D(d);
		return d;
	}


	public static Maybe<Trigger> ToTrigger(this IEvtGen<Pt> evt) =>
		evt switch
		{
			MouseBtnEvtGen<Pt> { UpDown: UpDown.Down, Btn: MouseBtn.Left } => May.Some(Trigger.Down),
			MouseBtnEvtGen<Pt> { UpDown: UpDown.Up, Btn: MouseBtn.Left } => May.Some(Trigger.Up),
			MouseBtnEvtGen<Pt> { UpDown: UpDown.Down, Btn: MouseBtn.Right } => May.Some(Trigger.DownRight),
			MouseClickEvtGen<Pt> { Btn: MouseBtn.Left } => May.Some(Trigger.Click),
			_ => May.None<Trigger>()
		};



	private static IObservable<H> WhenTrigger<H>(this Evt evt, IHotspot<H> hotspot) =>
		evt.WhenEvt
			.Where(e => e.ToTrigger().IsSomeAndEqualTo(hotspot.Trigger))
			.Where(e => e.GetMayMousePos().IsSome())
			.Select(e => e.GetMayMousePos().Ensure())
			.Where(e => hotspot.Get(e).IsSome())
			.Select(e => hotspot.Get(e).Ensure());
}



public interface ITool
{
	string Name { get; }
	Keys Shortcut { get; }
	Task Run(IRoDispBase d);
}

public abstract class Tool<M> : ITool
{
	protected ToolEnv Env { get; }
	protected ModelMan<M> MM { get; }

	public string Name => GetType().Name[..^4];
	public abstract Keys Shortcut { get; }

	protected Tool(ToolEnv env, ModelMan<M> mm)
	{
		Env = env;
		MM = mm;
	}

	public abstract Task Run(IRoDispBase d);
}


sealed class NoneTool : ITool
{
	private readonly Action setDefaultCursor;

	public NoneTool(Action setDefaultCursor)
	{
		this.setDefaultCursor = setDefaultCursor;
	}

	public string Name => GetType().Name[..^4];

	public Keys Shortcut => Keys.Escape;

	public Task Run(IRoDispBase d)
	{
		setDefaultCursor();
		return Task.CompletedTask;
	}
}
