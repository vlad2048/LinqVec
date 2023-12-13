using SmartReactives.Common;

namespace SmartReactives
{
    /// <summary>
    /// This class exposes everything from the Common API of SmartReactives. 
    /// Use this class as a starting point for using SmartReactives.
    /// </summary>
    public static class RX
    {
        /// <summary>
        /// Create a reactive variable. All mutating variables used in reactive expressions should be reactive.
        /// </summary>
        public static RxVar<T> Var<T>(T value) => new(value);

        /// <summary>
        /// Creates a reactive expression. This is an expression that tracks when its value changes, due to changes in underlying inputs.
        /// You can subscribe to these changes through the IObservable interface which RxExpr implements.
        /// </summary>
        public static RxExpr<T> Expr<T>(Func<T> expression) => new(expression);

        public static RxExpr<T> ToExpr<T>(this RxVar<T> v) => Expr(() => v.V);

        /// <summary>
        /// Creates a reactive cache. This is a cache that automatically clears itself when it becomes stale.
        /// </summary>
        public static RxCache<T> Cache<T>(Func<T> expression) => new(expression);

        /// <summary>
        /// Converts a regular list into a reactive list, which can be used in reactive expressions.
        /// </summary>
        public static IList<T> ToReactive<T>(this IList<T> original) => new RxList<T>(original);

        /// <summary>
        /// Converts a regular set into a reactive set, which can be used in reactive expressions.
        /// </summary>
        public static ISet<T> ToReactive<T>(this ISet<T> original) => new RxSet<T>(original);

        /// <summary>
        /// Converts a regular dictionary into a reactive dictionary, which can be used in reactive expressions.
        /// </summary>
        public static IDictionary<TKey, TValue> ToReactive<TKey, TValue>(this IDictionary<TKey, TValue> original) => new RxDictionary<TKey, TValue>(original);
    }
}