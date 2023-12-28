namespace LinqVec;

public record struct CfgLogCmd(
	bool CurStateHotspot,
	bool Evt,
	bool DbgEvt
);

public record struct CfgLog(
	bool UndoRedo,
	bool CurTool,
	CfgLogCmd LogCmd
);

public record struct Cfg(
	CfgLog Log
);