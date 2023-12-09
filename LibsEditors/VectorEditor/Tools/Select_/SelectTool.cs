using LinqVec.Logic;
using LinqVec.Tools;
using LinqVec.Tools.Events;
using PowRxVar;
using VectorEditor.Model;

namespace VectorEditor.Tools.Select_;


sealed class SelectTool(ToolEnv env, ModelMan<DocModel> mm) : Tool<DocModel>(env, mm)
{
	public override Keys Shortcut => Keys.Q;

	public override IDisposable Run(Action reset)
	{
		var d = new Disp();

		var evt = Env.GetEvtForTool(this)
			.ToGrid(Env.Transform)
			.TrackPos(out var mousePos, d)
			.MakeHot(d)
			.ToEvt(e => Env.Curs.Cursor = e);



		return d;
	}
}
