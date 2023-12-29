using System.Text;
using PowBasics.CollectionsExt;
using Shouldly;

namespace PtrLib.Tests.TestSupport;

static class CheckExt
{
	public static void CheckHistory<T>(
		this PtrBase<T> ptr,
		T[] expUndosExt,
		T[] expRedos,
		T expVModded
	)
	{
		var actUndosExt = ptr.Undoer.StackUndoExt;
		var actRedos = ptr.Undoer.StackRedo;
		var actVModded = ptr.VModded;

		var sb = new StringBuilder();
		sb.Append("CheckHistory(");
		sb.Append($"[{actUndosExt.JoinText(", ")}]");
		sb.Append($", [{actRedos.JoinText(", ")}]");
		sb.Append($", {actVModded}");
		sb.Append(")");
		Console.WriteLine(sb.ToString());

		CollectionAssert.AreEqual(expUndosExt, actUndosExt);
		CollectionAssert.AreEqual(expRedos, actRedos);
		actVModded.ShouldBe(expVModded);
	}
}