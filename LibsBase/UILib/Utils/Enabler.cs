﻿using System.Reactive.Linq;
using PowMaybe;
using PowRxVar;

namespace UILib.Utils;

public static class Enabler
{
	public static IDisposable Enables<T>(this IRoMayVar<T> mayVar, params ToolStripItem[] items) =>
		mayVar.Select(e => e.IsSome()).Subscribe(on =>
		{
			foreach (var item in items)
				item.Enabled = on;
		});
}