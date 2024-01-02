namespace LinqVec;

public record struct CfgLogCmd(
	bool Evt,
	bool UsrEvt,
	bool Hotspot,
	bool CmdEvt,
	bool ModEvt
);

public record struct CfgLog(
	bool Disp,
	bool UndoRedo,
	bool CurTool,
	bool EditorState,
	CfgLogCmd LogCmd
);

public record struct Cfg(
	CfgLog Log
);