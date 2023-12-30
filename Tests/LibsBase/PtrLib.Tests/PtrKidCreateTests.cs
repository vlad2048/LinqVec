using PtrLib.Tests.TestSupport;
using Shouldly;

namespace PtrLib.Tests;

class PtrKidCreateTests : TestBase
{
	[Test]
	public void _0_Create_Twice_Without_DisposeFirst()
	{
		DisableDispCheck();

		var dad = new PtrDad<string>("ab", D);
		dad.CheckHistory(["ab"], [], "ab");

		using (var kid = (PtrBase<char>)dad.Create('K', Funs.SetCharN(2), _ => true, MkD()))
		{
			dad.CheckHistory(["ab"], [], "ab");
			kid.CheckHistory(['K'], [], 'K');

			Should.Throw<ObjectDisposedException>(() => (PtrBase<char>)dad.Create('L', Funs.SetCharN(3), _ => true, MkD()));
		}
	}

	[Test]
	public void _1_Create_Twice_With_DisposeFirst()
	{
		var dad = new PtrDad<string>("ab", D);
		dad.CheckHistory(["ab"], [], "ab");

		using (var kid = (PtrBase<char>)dad.Create('K', Funs.SetCharN(2), _ => true, MkD()))
		{
			dad.CheckHistory(["ab"], [], "ab");
			kid.CheckHistory(['K'], [], 'K');
		}
		dad.CheckHistory(["ab"], [], "ab");

		using (var kid = (PtrBase<char>)dad.Create('L', Funs.SetCharN(3), _ => true, MkD()))
		{
			dad.CheckHistory(["ab"], [], "ab");
			kid.CheckHistory(['L'], [], 'L');
		}
		dad.CheckHistory(["ab"], [], "ab");
	}

	[Test]
	public void _2_Create_Twice_With_CommitFirst()
	{
		var dad = new PtrDad<string>("ab", D);
		dad.CheckHistory(["ab"], [], "ab");

		var kid = (PtrBase<char>)dad.Create('K', Funs.SetCharN(2), _ => true, MkD());
		dad.CheckHistory(["ab"], [], "ab");
		kid.CheckHistory(['K'], [], 'K');

		((IPtrCommit<char>)kid).Commit();
		kid.IsDisposed.ShouldBe(true);

		dad.CheckHistory(["ab", "abK"], [], "abK");

		using (var kid2 = (PtrBase<char>)dad.Create('L', Funs.SetCharN(3), _ => true, MkD()))
		{
			dad.CheckHistory(["ab", "abK"], [], "abK");
			kid2.CheckHistory(['L'], [], 'L');
		}
		dad.CheckHistory(["ab", "abK"], [], "abK");
	}
}


file static class Funs
{
	public static Func<string, char, string> SetCharN(int n) => (str, c) => (str.Length <= n) switch {
		true => str.PadRight(n + 1).Repl(n, c),
		false => str.Repl(n, c),
	};


	private static string Repl(this string str, int n, char c)
	{
		var list = str.ToList();
		list[n] = c;
		return new string(list.ToArray());
	}
}