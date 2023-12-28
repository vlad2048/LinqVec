using System.Reactive.Linq;
using ReactiveVars;

namespace UILib.Utils;

public static class Enabler
{
	public static void Disables(this IRoVar<bool> isDisabled, Disp d, params ToolStripItem[] items) =>
		isDisabled.Subscribe(isOff =>
		{
			foreach (var item in items)
				item.Enabled = !isOff;
		}).D(d);

	public static IDisposable Enables<T>(this IRoVar<Option<T>> mayVar, params ToolStripItem[] items) =>
		mayVar.Select(e => e.IsSome).Subscribe(on =>
		{
			foreach (var item in items)
				item.Enabled = on;
		});
}