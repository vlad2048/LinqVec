using LinqVec.Drawing;
using LinqVec.Structs;
using ReactiveVars;

namespace LinqVec.Controls;

sealed record DrawPanelInitNfo(
	IRwVar<Transform> Transform,
	Res Res
);
