﻿using BrightIdeasSoftware;
using LanguageExt.Pretty;
using LinqVec.Logic;
using LinqVec.Tools;

namespace LinqVec;

[Flags]
public enum EditorLogicCaps
{
	SupportLayoutPane = 1,
}

public abstract class EditorLogic<TDoc>
{
	public abstract EditorLogicCaps Caps { get; }

	public abstract ITool<TDoc>[] Tools { get; }

	public abstract TDoc LoadOrCreate(Option<string> file);

	public abstract void Init(
		VecEditor<TDoc> vecEditor,
		Unmod<TDoc> doc,
		Disp d
	);

	public virtual void SetupLayoutPane(
		TreeListView tree,
		IObservable<Option<TDoc>> doc,
		Disp d
	) => throw new NotImplementedException();
}