using System.Numerics;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.World;

namespace PhysicsEditor.Physics_;

public class ShapeOpt
{
	public float Density { get; set; }
	public float Friction { get; set; } = 0.2f;
	public bool IsSensor { get; set; }
	public float Restitution { get; set; }

	private ShapeOpt() {}

	internal static ShapeOpt Build(Action<ShapeOpt>? optFun)
	{
		var opt = new ShapeOpt();
		optFun?.Invoke(opt);
		return opt;
	}
}

public record ShapeGfx(params Pt[] Pts)
{
	public Pt[] PtsClosed => Pts.Append(Pts[0]).ToArray();
}

public static class Shape
{
	public static FixtureDef MakeBox(float halfWidth, float halfHeight, Action<ShapeOpt>? optFun = null)
	{
		var bShape = new PolygonShape();
		bShape.SetAsBox(halfWidth, halfHeight);
		var opt = ShapeOpt.Build(optFun);
		var bFixture = new FixtureDef
		{
			density = opt.Density,
			friction = opt.Friction,
			isSensor = opt.IsSensor,
			restitution = opt.Restitution,
			shape = bShape,
			userData = new ShapeGfx(
				new Pt(-halfWidth, -halfHeight),
				new Pt(halfWidth, -halfHeight),
				new Pt(halfWidth, halfHeight),
				new Pt(-halfWidth, halfHeight)
			)
		};
		return bFixture;
	}
}

public static class WorldExt
{
	private const float TimeStep = 1f / 60f;
	private const int VelocityIterations = 6;
	private const int PositionIterations = 2;

	public static void Step(this World world) => world.Step(TimeStep, VelocityIterations, PositionIterations);

	public static Body MakeBody(
		this World world,
		BodyType type,
		Pt pos,
		FixtureDef shape
	)
	{
		var bodyDef = new BodyDef
		{
			type = type,
			position = pos.ToPhysPt()
		};
		var body = world.CreateBody(bodyDef);
		var fixture = body.CreateFixture(shape);
		return body;
	}

	public static Body[] GetBodies(this World world)
	{
		var list = new List<Body>();
		var body = world.GetBodyList();
		while (body != null)
		{
			list.Add(body);
			body = body.GetNext();
		}
		return list.ToArray();
	}
}


public static class GeomConv
{
	public static Vector2 ToPhysPt(this Pt p) => new(p.X, p.Y);
	public static Pt ToPt(this Vector2 p) => new(p.X, p.Y);
}