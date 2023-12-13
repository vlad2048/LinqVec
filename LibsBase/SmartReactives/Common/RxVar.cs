using SmartReactives.Core;

namespace SmartReactives.Common
{
    /// <summary>
    /// A variable which can be used in an <see cref="RxExpr{T}" /> and other reactive objects such as <see cref="RxCache{T}" />
    /// </summary>
    public class RxVar<T>
    {
        T value;

        public RxVar(T value = default(T))
        {
            this.value = value;
        }

        /// <summary>
        /// The value
        /// </summary>
        public T V
        {
            get
            {
                ReactiveManager.WasRead(this);
                return value;
            }
            set
            {
                this.value = value;
                ReactiveManager.WasChanged(this);
            }
        }

        // ReSharper disable once UnusedMember.Local
        /// <summary>
        /// For debugging purposes.
        /// </summary>
        internal IEnumerable<object> Dependents => ReactiveManager.GetDependents(this);

        /// <summary>
        /// Set a new value, but only raise a change if the new value does not equal the existing one.
        /// </summary>
        public void SetValueIfChanged(T newValue)
        {
            if (!Equals(V, newValue))
            {
                V = newValue;
            }
        }

        public static implicit operator T(RxVar<T> reactive)
        {
            return reactive.V;
        }
    }
}