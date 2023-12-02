﻿using System.Reactive;
using System.Reactive.Disposables;
using PowRxVar;

namespace LinqVec.Utils.WinForms_;

public interface ICurs
{
	IDisposable SetToolCurs(Cursor curs);
}

public class Ctrl(Control ctrl) : ICurs
{
	public Cursor Cursor
	{
		get => ctrl.Cursor;
		set => ctrl.Cursor = value;
	}

	public IObservable<Unit> WhenSizeChanged => ctrl.Events().ClientSizeChanged.ToUnit();

	public PtInt Sz => new(ctrl.Width, ctrl.Height);

	public IDisposable SetToolCurs(Cursor curs)
	{
		Cursor = curs;
		return Disposable.Create(() => Cursor = Cursors.Default);
	}
}