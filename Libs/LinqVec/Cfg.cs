using System.Text.Json.Serialization;

namespace LinqVec;

public record struct CfgLogCmd(
	bool Evt,
	bool UsrEvt,
	bool Hotspot,
	bool CmdEvt,
	bool ModEvt
)
{
	[JsonIgnore]
	public static readonly CfgLogCmd AllLoggingEnabled = new(true, true, true, true, true);
}

public record struct CfgLog(
	bool Disp,
	bool UndoRedo,
	bool CurTool,
	bool EditorState,
	CfgLogCmd LogCmd
)
{
	[JsonIgnore]
	public static readonly CfgLog AllLoggingEnabled = new(true, true, true, true, CfgLogCmd.AllLoggingEnabled);
}

public record struct Cfg(
	CfgLog Log
)
{
	[JsonIgnore]
	public static readonly Cfg AllLoggingEnabled = new(CfgLog.AllLoggingEnabled);
}