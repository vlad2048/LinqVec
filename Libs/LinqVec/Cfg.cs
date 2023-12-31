namespace LinqVec;

public record struct CfgLogCmd(
	bool RunEvt,
	bool CmdEvt
);

public record struct CfgLog(
	bool UndoRedo,
	bool CurTool,
	bool EditorState,
	CfgLogCmd LogCmd
);

public record struct Cfg(
	CfgLog Log
);