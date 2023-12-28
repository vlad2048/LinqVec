namespace LinqVec.Panes.DocPaneLogic_;

public sealed record DocPaneLoadInfo<TDoc>(
	TDoc Doc,
	string Filename
) where TDoc : class;