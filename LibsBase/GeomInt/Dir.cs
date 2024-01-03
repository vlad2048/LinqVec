namespace GeomInt;

public enum Dir
{
	Horz,
	Vert,
}

public static class DirUtils
{
	public static Dir Neg(this Dir dir) => dir switch
	{
		Dir.Horz => Dir.Vert,
		Dir.Vert => Dir.Horz,
		_ => throw new ArgumentException()
	};
}
