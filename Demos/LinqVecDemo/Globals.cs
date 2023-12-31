global using LanguageExt;
global using LanguageExt.Common;
global using static LanguageExt.Prelude;
global using Unit = LanguageExt.Unit;

global using Obs = System.Reactive.Linq.Observable;
global using Disp = System.Reactive.Disposables.CompositeDisposable;
global using static ReactiveVars.DispMaker;

global using L = ReactiveVars.ReactiveVarsLogger;
global using LC = LinqVec.Utils.ColoredLogger;
global using LT = LinqVec.Utils.ThreadLogger;


global using TDoc = VectorEditor._Model.Doc;
global using TState = VectorEditor.EditorState;
