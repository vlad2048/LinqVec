using System.Reactive.Linq;
using System.Reactive.Subjects;
using LanguageExt.Pretty;
using LinqVec.Controls;
using LinqVec.Logic;
using LinqVec.Structs;
using LinqVec.Tools.Events;
using LinqVec.Tools.Events.Utils;
using LinqVec.Utils.WinForms_;
using ReactiveVars;

namespace LinqVec.Tools;

public sealed class ToolEnv<TDoc> : IDisposable
{
	private readonly Disp d = MkD();
	public void Dispose() => d.Dispose();

	private readonly DrawPanel drawPanel;
	private readonly IRoVar<bool> isPanZoom;
	private readonly Action<ITool<TDoc>> setCurTool;
	private readonly IObservable<IEvt> editorEvt;
	private readonly ISubject<Unit> whenUndoRedo;
	private IObservable<Unit> WhenUndoRedo => whenUndoRedo.AsObservable();

	public ToolEnv(
		Unmod<TDoc> doc,
		IRoVar<ITool<TDoc>> curTool,
		Action<ITool<TDoc>> setCurTool,
		DrawPanel drawPanel,
		ICurs curs,
		IRoVar<bool> isPanZoom,
		IRoVar<Transform> transform,
		IObservable<IEvt> editorEvt
	)
	{
		this.drawPanel = drawPanel;
		this.isPanZoom = isPanZoom;
		this.editorEvt = editorEvt;
		this.setCurTool = setCurTool;
		Doc = doc;
		CurTool = curTool;
		Curs = curs;
		Transform = transform;
		WhenPaint = drawPanel.WhenPaint;
		whenUndoRedo = new Subject<Unit>().D(d);
	}


	internal void TriggerUndoRedo() => whenUndoRedo.OnNext(Unit.Default);

	public Unmod<TDoc> Doc { get; }
	public IRoVar<ITool<TDoc>> CurTool { get; }
	public void SetCurTool(ITool<TDoc> tool) => setCurTool(tool);
	public ICurs Curs { get; }
    public IRoVar<Transform> Transform { get; }
    public IObservable<Gfx> WhenPaint { get; }
    public void Invalidate() => drawPanel.Invalidate();

    public IObservable<IEvt> EditorEvt => editorEvt;
    public Evt GetEvtForTool(ITool<TDoc> tool, bool snap, Disp toolD) =>
	    snap switch
	    {
            false => editorEvt
	            .RestrictToTool(tool, CurTool, isPanZoom)
	            .ToGrid(Transform)
	            .ToEvt(e => Curs.Cursor = e, WhenUndoRedo, toolD),
            true => editorEvt
	            .RestrictToTool(tool, CurTool, isPanZoom)
	            .ToGrid(Transform)
	            .SnapToGrid()
	            .RestrictToGrid()
	            .ToEvt(e => Curs.Cursor = e, WhenUndoRedo, toolD),
		};
}