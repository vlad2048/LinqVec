namespace LoggingConfig.Logging_;

public static class Nst
{
    private static int indent;

    public static IDisposable Log(string str)
    {
        var pad = new string(' ', indent * 4);
        L($"{pad}{str}");
        indent++;
        return new DispAction(() => indent--);
    }

    private class DispAction : IDisposable
    {
        public void Dispose() => action();

        private readonly Action action;

        public DispAction(Action action)
        {
            this.action = action;
        }
    }

    private static void L(string s) => Console.WriteLine(s);
}