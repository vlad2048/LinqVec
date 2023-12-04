using System.Numerics;
using PowRxVar;
using System.Windows.Forms;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.World;
using LinqVec.Structs;
using LinqVec.Utils.WinForms_;
using PhysicsEditor.Drawing;
using PhysicsEditor.Physics_;
using LinqVec.Tools;

namespace PhysicsEditor.Tools.Play_;

public sealed class PlayTool : Tool
{
	public override string Name => "curve";
	public override Keys Shortcut => Keys.F1;

	public override (Tool, IDisposable) Init(ToolEnv env)
	{
		var d = new Disp();

		var world = new World(new Vector2(0, -10));
		world.MakeBody(
			BodyType.Static,
			new Pt(0, -10),
			Shape.MakeBox(50, 10)
		);
		var box = world.MakeBody(
			BodyType.Dynamic,
			new Pt(0, 4),
			Shape.MakeBox(1, 1, opt =>
			{
				opt.Density = 1;
				opt.Friction = 0.3f;
			})
		);

		world.Step();

		//L.WriteLine($"body.pos = {box.Position}");


		ShowFps(env.WhenPaint).D(d);

		env.WhenPaint
			.Subscribe(gfx =>
			{
				var bodies = world.GetBodies();
				foreach (var body in bodies)
				{
					gfx.DrawBody(body);
				}
			}).D(d);


		Obs.Interval(TimeSpan.FromSeconds(1) / 60)
			.ObserveOnUI()
			.Subscribe(_ =>
			{
				world.Step();
				env.Invalidate();
			}).D(d);
		

		return (this, d);
	}


	private static readonly TimeSpan FpsInterval = TimeSpan.FromSeconds(3);

	private static IDisposable ShowFps(IObservable<Gfx> whenPaint)
	{
		var lastTime = DateTime.MinValue;
		var cnt = 0;
		return whenPaint.Subscribe(_ =>
		{
			var now = DateTime.Now;
			var delta = now - lastTime;
			cnt++;
			if (delta > FpsInterval)
			{
				var fps = cnt / delta.TotalSeconds;
				L.WriteLine($"fps: {fps:F2}");
				cnt = 0;
				lastTime = now;
			}
		});
	}
}