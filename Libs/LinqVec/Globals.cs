global using LanguageExt;
global using LanguageExt.Common;
global using static LanguageExt.Prelude;
global using Unit = LanguageExt.Unit;

global using Obs = System.Reactive.Linq.Observable;
global using Disp = System.Reactive.Disposables.CompositeDisposable;
global using static LinqVec.Utils.CommonMakers;
global using L = LinqVec.Utils.Logger;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("LINQPadQuery")]
[assembly:InternalsVisibleTo("LinqVec.Tests")]
