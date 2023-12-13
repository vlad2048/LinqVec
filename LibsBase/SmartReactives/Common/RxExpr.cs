using System.Reactive.Subjects;
using SmartReactives.Core;

namespace SmartReactives.Common
{
    /// <summary>
    /// Detects when an expression changes its value and exposes these changes as an <see cref="IObservable{T}" />.
    /// After subscribing to the <see cref="IObservable{T}" /> an initial notification is pushed immediately.
    /// Make sure the provided expression only depends on constant values or reactive objects, such as <see cref="RxVar{T}" /> or <see cref="RxExpr{T}" />.
    /// </summary>
    [Serializable]
    public class RxExpr<T> : IObservable<Func<T>>, IListener
    {
        readonly Func<T> expression;
	    readonly bool weakReference;
	    readonly ISubject<Func<T>> subject = new Subject<Func<T>>();

	    public T V => Evaluate();

		/// <summary>
		/// </summary>
		/// <param name="expression">The expression to monitor for changes</param>
		/// <param name="weakReference">Whether to keep only a weak reference to this reactive expression. 
		/// Passing false means you have to keep a reference to the constructed reactive expresion, in order for subscriptions on it to remain alive.</param>
        public RxExpr(Func<T> expression, bool weakReference = false)
        {
	        this.expression = expression;
	        this.weakReference = weakReference;
        }

        // ReSharper disable once UnusedMember.Local
        /// <summary>
        /// For debugging purposes.
        /// </summary>
        internal IEnumerable<object> Dependents => ReactiveManager.GetDependents(this);

        void IListener.Notify()
        {
            subject.OnNext(Evaluate);
        }

	    public bool StrongReference => !weakReference;

	    public IDisposable Subscribe(IObserver<Func<T>> observer)
        {
            observer.OnNext(Evaluate);
            return subject.Subscribe(observer);
        }

        /// <summary>
        /// Evaluate the expression and return its value.
        /// </summary>
        public T Evaluate()
        {
            return ReactiveManager.Evaluate(this, expression);
        }
    }
}