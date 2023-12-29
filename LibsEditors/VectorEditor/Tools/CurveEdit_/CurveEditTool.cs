using LinqVec.Tools;
using VectorEditor.Model;

namespace VectorEditor.Tools.CurveEdit_;

sealed class CurveEditTool(Keys shortcut) : ITool<Doc>
{
	public string Name => "E";
	public Bitmap? Icon => Resource.toolicon_CurveEdit;
	public Keys Shortcut => shortcut;

	public Disp Run(ToolEnv<Doc> Env, ToolActions toolActions)
	{
		var d = MkD();

		return d;
	}
}