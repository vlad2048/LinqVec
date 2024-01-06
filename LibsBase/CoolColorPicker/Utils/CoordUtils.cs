namespace CoolColorPicker.Utils;

static class CoordUtils
{
	public static Point ProjectMouseOntoRectangle(Point pt, Rectangle rect)
	{
		if (rect.Contains(pt))
			return pt;

		var rank = Enum.GetValues<Side>()
			.Select(side => CheckSide(pt, rect, side))
			.Where(e => e.Intersect && !double.IsNaN(e.Dist))
			.MinBy(e => e.Dist);
		if (rank == null) throw new ArgumentException();

		return rank.Pos.ToPoint();
	}


	private enum Side
	{
		Top, Right, Bottom, Left
	}

	private record Rank(bool Intersect, Vector Pos, double Dist);

	private static Rank CheckSide(Point pt, Rectangle rect, Side side)
	{
		var (left, right, top, bottom) = (rect.Left, rect.Right - 1, rect.Top, rect.Bottom - 1);
		var p = Vector.FromPoint(pt);
		var p2 = new Vector((left + right) / 2.0, (top + bottom) / 2.0);
		var (q, q2) = side switch
		{
			Side.Top => (new Vector(right, top), new Vector(left, top)),
			Side.Bottom => (new Vector(left, bottom), new Vector(right, bottom)),
			Side.Left => (new Vector(left, top), new Vector(left, bottom)),
			Side.Right => (new Vector(right, bottom), new Vector(right, top)),
			_ => throw new ArgumentException()
		};

		var intersect = LineSegementsIntersect(p, p2, q, q2, out var pos);
		var dist = intersect switch
		{
			true => (pos - p).Length,
			false => 0
		};
		return new Rank(intersect, pos, dist);
	}

	private const double Epsilon = 1e-10;
	private static bool IsZero(this double v) => v < Epsilon;

	/// <summary>
	/// Test whether two line segments intersect. If so, calculate the intersection point.
	/// <see cref="http://stackoverflow.com/a/14143738/292237"/>
	/// </summary>
	/// <param name="p">Vector to the start point of p.</param>
	/// <param name="p2">Vector to the end point of p.</param>
	/// <param name="q">Vector to the start point of q.</param>
	/// <param name="q2">Vector to the end point of q.</param>
	/// <param name="intersection">The point of intersection, if any.</param>
	/// <param name="considerCollinearOverlapAsIntersect">Do we consider overlapping lines as intersecting?
	/// </param>
	/// <returns>True if an intersection point was found.</returns>
	private static bool LineSegementsIntersect(
		Vector p,
		Vector p2,
		Vector q,
		Vector q2,
		out Vector intersection,
		bool considerCollinearOverlapAsIntersect = false
	)
	{
		intersection = new Vector();

		var r = p2 - p;
		var s = q2 - q;
		var rxs = r.Cross(s);
		var qpxr = (q - p).Cross(r);

		// If r x s = 0 and (q - p) x r = 0, then the two lines are collinear.
		if (rxs.IsZero() && qpxr.IsZero())
		{
			// 1. If either  0 <= (q - p) * r <= r * r or 0 <= (p - q) * s <= * s
			// then the two lines are overlapping,
			if (considerCollinearOverlapAsIntersect)
				if ((0 <= (q - p) * r && (q - p) * r <= r * r) || (0 <= (p - q) * s && (p - q) * s <= s * s))
					return true;

			// 2. If neither 0 <= (q - p) * r = r * r nor 0 <= (p - q) * s <= s * s
			// then the two lines are collinear but disjoint.
			// No need to implement this expression, as it follows from the expression above.
			return false;
		}

		// 3. If r x s = 0 and (q - p) x r != 0, then the two lines are parallel and non-intersecting.
		if (rxs.IsZero() && !qpxr.IsZero())
			return false;

		// t = (q - p) x s / (r x s)
		var t = (q - p).Cross(s) / rxs;

		// u = (q - p) x r / (r x s)

		var u = (q - p).Cross(r) / rxs;

		// 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1
		// the two line segments meet at the point p + t r = q + u s.
		if (!rxs.IsZero() && t is >= 0 and <= 1 && u is >= 0 and <= 1)
		{
			// We can calculate the intersection point using either t or u.
			intersection = p + t * r;

			// An intersection was found.
			return true;
		}

		// 5. Otherwise, the two line segments are not parallel but do not intersect.
		return false;
	}

	private class Vector
	{
		// ReSharper disable MemberCanBePrivate.Local
		public double X { get; }
		public double Y { get; }
		// ReSharper restore MemberCanBePrivate.Local

		// Constructors.
		public Vector(double x, double y) { X = x; Y = y; }
		public Vector() : this(double.NaN, double.NaN) { }
		public override string ToString() => $"{X:F3},{Y:F3}";

		public static Vector FromPoint(Point pt) => new(pt.X, pt.Y);
		public Point ToPoint() => new((int)X, (int)Y);
		public double Length => Math.Sqrt(this * this);

		public static Vector operator -(Vector v, Vector w)
		{
			return new Vector(v.X - w.X, v.Y - w.Y);
		}

		public static Vector operator +(Vector v, Vector w)
		{
			return new Vector(v.X + w.X, v.Y + w.Y);
		}

		public static double operator *(Vector v, Vector w)
		{
			return v.X * w.X + v.Y * w.Y;
		}

		public static Vector operator *(Vector v, double mult)
		{
			return new Vector(v.X * mult, v.Y * mult);
		}

		public static Vector operator *(double mult, Vector v)
		{
			return new Vector(v.X * mult, v.Y * mult);
		}

		public double Cross(Vector v)
		{
			return X * v.Y - Y * v.X;
		}

		public override bool Equals(object? obj)
		{
			if (obj is not Vector v) return false;
			return IsZero(X - v.X) && IsZero(Y - v.Y);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(X, Y);
		}

		public static bool operator ==(Vector? left, Vector? right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Vector? left, Vector? right)
		{
			return !Equals(left, right);
		}
	}
}