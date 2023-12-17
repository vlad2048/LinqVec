using Geom;
using LinqVec.Logic;

namespace LinqVec.Tools.Acts.Delegates;

public delegate MouseMod<O> MouseModStartHot<O, in H>(Pt startPos, H hot);
public delegate MouseMod<O> MouseModStart<O>(Pt startPos);
