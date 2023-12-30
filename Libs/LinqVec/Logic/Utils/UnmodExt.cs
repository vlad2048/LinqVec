/*
using Geom;
using LinqVec.Logic.Structs;

namespace LinqVec.Logic.Utils;

public static class UnmodExt
{
    public static Action ClearMod<O>(this Unmod<O> unmod) => () =>
    {
        if (unmod.IsDisposed) return;
        unmod.ModSet(Mod<O>.Empty);
    };
    public static Action HoverMod<O>(this Unmod<O> unmod, Mod<O> mod) => () =>
    {
        if (unmod.IsDisposed) return;
        unmod.ModSet(mod);
    };
    public static Func<Pt, Action> DragMod<O>(this Unmod<O> unmod, Func<Pt, Mod<O>> mod) => pt =>
    {
        if (unmod.IsDisposed) return () => { };
        unmod.ModSet(mod(pt));
        return unmod.ModFlush;
    };
}
*/