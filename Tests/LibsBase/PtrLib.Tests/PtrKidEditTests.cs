using PtrLib.Tests.TestSupport;

namespace PtrLib.Tests;

class PtrKidEditTests : TestBase
{
	[Test]
	public void _0_Edit()
	{
		var dad = new PtrDad<string>("abE", D);
		dad.CheckHistory(["abE"], [], "abE");

		using (var kid = (PtrBase<char>)dad.Edit('E', Funs.ReplN(2), Funs.DelN(2), MkD()))
		{
			dad.CheckHistory(["abE"], [], "ab");
			kid.CheckHistory(['E'], [], 'E');

			using (kid.ModSet(MkMod<char>(true, _ => 'F')))
			{
				dad.CheckHistory(["abE"], [], "ab");
				kid.CheckHistory(['E'], [], 'F');
			}
			dad.CheckHistory(["abE", "abF"], [], "ab");
			kid.CheckHistory(['E', 'F'], [], 'F');
		}

		dad.CheckHistory(["abE", "abF"], [], "abF");
	}
}


file static class Funs
{
	public static Func<string, char, string> ReplN(int n) => (str, c) =>
	{
		var list = str.ToList();
		list[n] = c;
		return new string(list.ToArray());
	};
	public static Func<string, char, string> DelN(int n) => (str, _) => new string([.. str.Take(n), .. str.Skip(n + 1)]);
}