namespace SmartReactives.Core
{
	/// <summary>
	/// A weak edge in the dependency graph of <see cref="ReactiveManager"/>.
	/// </summary>
	class WeakDependency : IDependency
	{
        public long NotificationsHad { get; }

        readonly WeakReference nodeReference;
        public IListener? Value => nodeReference.Target as IListener;

        public WeakDependency(long notificationsHad, IListener dependent)
        {
            NotificationsHad = notificationsHad;
            nodeReference = new WeakReference(dependent);
        }

        public override string ToString()
        {
            return Value?.ToString() ?? "empty reference";
        }
    }
}