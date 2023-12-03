namespace LinqVec.Tools.Curve_.Symmetry;

enum SymmetryType
{
	Horz,
	Vert,
	Quad,
}

sealed record SymmetryNfo(
	SymmetryType Type,
	Pt Origin
);