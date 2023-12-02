using LinqVec.Drawing;
using LinqVec.Structs;
using PowRxVar;

namespace LinqVec.Controls;

sealed record DrawPanelInitNfo(
	IRwVar<Transform> Transform,
	Res Res
);
