<Query Kind="Program">
  <Namespace>LINQPad.Controls</Namespace>
</Query>

void Main()
{
	var slowDebug = @"C:\dev\big\LinqVec\Demos\LinqVecDemo\bin\Debug\net8.0-windows\LinqVecDemo.exe";
	var slowRelease = @"C:\dev\big\LinqVec\Demos\LinqVecDemo\bin\Release\net8.0-windows\LinqVecDemo.exe";
	var fastDebug = @"C:\dev\big\LinqVec\Demos\LinqVecDocklessDemo\bin\Debug\net8.0-windows\LinqVecDocklessDemo.exe";
	var fastRelease = @"C:\dev\big\LinqVec\Demos\LinqVecDocklessDemo\bin\Release\net8.0-windows\LinqVecDocklessDemo.exe";
	var rawDebug = @"C:\dev\big\LinqVec\Demos\SimpleWinDemo\bin\Debug\net8.0-windows\SimpleWinDemo.exe";
	var rawRelease = @"C:\dev\big\LinqVec\Demos\SimpleWinDemo\bin\Release\net8.0-windows\SimpleWinDemo.exe";
	
	Button Btn(string title, string cmd) => new(title, _ => Util.Cmd(cmd));
	
	Util.HorizontalRun(true, Btn("Debug", slowDebug), Btn("Release", slowRelease)).Dump("Slow");
	Util.HorizontalRun(true, Btn("Debug", fastDebug), Btn("Release", fastRelease)).Dump("Fast");
	Util.HorizontalRun(true, Btn("Debug", rawDebug), Btn("Release", rawRelease)).Dump("Raw");
}
