using ReactiveVars;

namespace LinqVec.Utils.Rx;

static class VarUtils
{
	public static IRwVar<Option<T>> MakeOptionalAutoDisp<T>(Disp d) where T : IDisposable
	{
		var serDisp = new SerDisp().D(d);
		var var = Option<T>.None.Make(d);
		var.Subscribe(optV =>
		{
			var serD = serDisp.GetNewD();
			optV.IfSome(v => v.D(serD));
		}).D(d);
		return var;
	}
}