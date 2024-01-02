using LogLib.Writers;
using PowBasics.CollectionsExt;
using System.Reflection;
using LinqVec.Utils.Json;
using LogLib.Interfaces;
using PowBasics.Json_;

namespace LinqVec.Logging;


public static class LogVecConKeeper
{
	public static readonly ConWriter<IWriteSer> Instance = ConWriter<IWriteSer>.Instance;

	public static Action<ITxtWriter> ReplayWithCustomFunctions<T>(Type customType, string filename) where T : IWriteSer
	{
		var gens = VecJsoner.Vec.Load<GenNfo<T>[]>(filename);
		return w =>
		{
			foreach (var gen in gens)
				ReflectionUtils.CallCustomWrite(customType, gen, w);
		};
	}
}




file static class ReflectionUtils
{
	private static MethodInfo[] GetWriteFuns(Type customType) => customType.GetMethods().Where(e => e.Name == "Color" && e.GetParameters().Length > 0).ToArray();

	public static void CallCustomWrite<T>(Type customType, GenNfo<T> gen, ITxtWriter w) where T : IWriteSer
	{
		var method = PickCorrectWrite(GetWriteFuns(customType), gen);
		method.Invoke(null, [gen.Src, w]);
	}

	private static MethodInfo PickCorrectWrite<T>(MethodInfo[] writeFuns, GenNfo<T> gen) where T : IWriteSer
	{
		var srcType = gen.Src.GetType();
		Type[] srcTypes = [
			srcType,
			.. srcType.GetInterfaces()
		];
		var meths = writeFuns.WhereToArray(e => srcTypes.Any(f => f.Name == e.GetParameters()[0].ParameterType.Name));
		switch (meths.Length)
		{
			case > 1:
				throw new ArgumentException();
			case 1:
				return meths[0];
		}

		throw new ArgumentException();
	}
}
