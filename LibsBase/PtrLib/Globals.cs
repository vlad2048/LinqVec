﻿global using LanguageExt;
global using LanguageExt.Common;
global using static LanguageExt.Prelude;
global using Unit = LanguageExt.Unit;

global using Obs = System.Reactive.Linq.Observable;
global using Disp = System.Reactive.Disposables.CompositeDisposable;
global using static ReactiveVars.DispMaker;

global using LR = ReactiveVars.ReactiveVarsLogger;

using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("PtrLib.Tests")]