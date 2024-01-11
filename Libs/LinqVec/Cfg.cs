using LogLib;

namespace LinqVec;

public record struct CfgLogCmd(
	bool Hotspot,
	bool Drag,
	bool Evt,
	bool Usr,
	bool Cmd,
	bool Mod
);

public record struct CfgLog(
	bool Disp,
	bool UndoRedo,
	bool CurTool,
	bool EditorState,
	bool DisableLogTicker,
	TimeLogType TimeLogType,
	CfgLogCmd LogCmd
);

public record struct Cfg(
	CfgLog Log,
	bool DisableRepeatLastMouseMove
);