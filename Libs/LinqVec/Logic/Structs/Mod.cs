using ReactiveVars;

namespace LinqVec.Logic.Structs;

public sealed record Mod<T>(
    string Name,
    bool ApplyWhenDone,
    IRoVar<Func<T, T>> Fun
)
{
    public static readonly Mod<T> Empty = new("Empty", false, Var.MakeConst<Func<T, T>>(e => e));
}