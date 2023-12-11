using Box2D.NetStandard.Dynamics.Bodies;
using Geom;
using LinqVec.Structs;
using LinqVec.Utils;
using PhysicsEditor.Physics_;
using PowBasics.CollectionsExt;

namespace PhysicsEditor.Drawing;

static class PhysDrawExt
{
	public static void DrawBody(this Gfx gfx, Body body)
	{
		var userObj = body.GetFixtureList().UserData;
		var shape = (ShapeGfx)userObj;

		var pts = shape.PtsClosed
			.Select(e => body.GetWorldPoint(e.ToPhysPt()).ToPt())
			.Select(e => new Pt(e.X, -e.Y))
			.SelectToArray(e => e.ToWinPt());

		gfx.Graphics.DrawPolygon(gfx.Pen(C.BodyPen), pts);
	}
}