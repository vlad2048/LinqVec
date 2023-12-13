using System.Collections;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;

namespace SmartReactives;


public interface IDisposeNotification
{
	IObservable<Unit> WhenDisposed { get; }
}


public sealed class Disp : ICollection<IDisposable>, ICancelable, IDisposeNotification
{
	private const string Strings_Core_DISPOSABLES_CANT_CONTAIN_NULL = "Disposables collection can not contain null values.";
	private const int ShrinkThreshold = 64;
	private const int DefaultCapacity = 16;

	private readonly object _gate = new();
	private readonly ISubject<Unit> whenDisposed = new AsyncSubjectReverse<Unit>();
	private bool _disposed;
	private List<IDisposable?> _disposables;
	private int _count;

	public IObservable<Unit> WhenDisposed => whenDisposed.AsObservable();
	public int Count => Volatile.Read(ref _count);
	public bool IsReadOnly => false;
	public IEnumerator<IDisposable> GetEnumerator()
	{
		lock (_gate)
		{
			if (_disposed || _count == 0) return EmptyEnumerator;
			return new CompositeEnumerator(_disposables.ToArray());
		}
	}
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	public bool IsDisposed => Volatile.Read(ref _disposed);
	private static readonly CompositeEnumerator EmptyEnumerator = new(Array.Empty<IDisposable?>());

	private readonly int statsId = DispStats.GetNextDispId();
	private void InitStats(string? dbgExpr = null) => DispStats.DispCreated(statsId, dbgExpr);
	private void DoneStats() => DispStats.DispDisposed(statsId);

	public Disp(string? dbgExpr = null, [CallerFilePath] string srcFile = "", [CallerLineNumber] int srcLine = 0)
	{
		InitStats(dbgExpr ?? $@"{srcFile}:{srcLine}  @ ""new Disp()""");
		_disposables = new List<IDisposable?>();
	}

	public Disp(int capacity)
	{
		InitStats();
		if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
		_disposables = new List<IDisposable?>(capacity);
	}

	public Disp(params IDisposable[] disposables)
	{
		InitStats();
		if (disposables == null) throw new ArgumentNullException(nameof(disposables));
		_disposables = ToList(disposables);
		Volatile.Write(ref _count, _disposables.Count);
	}

	public Disp(IEnumerable<IDisposable> disposables)
	{
		InitStats();
		if (disposables == null) throw new ArgumentNullException(nameof(disposables));
		_disposables = ToList(disposables);
		Volatile.Write(ref _count, _disposables.Count);
	}

	private static List<IDisposable?> ToList(IEnumerable<IDisposable> disposables)
	{
		var capacity = disposables switch
		{
			IDisposable[] a => a.Length,
			ICollection<IDisposable> c => c.Count,
			_ => DefaultCapacity
		};

		var list = new List<IDisposable?>(capacity);
		foreach (var d in disposables)
		{
			if (d == null) throw new ArgumentException(Strings_Core_DISPOSABLES_CANT_CONTAIN_NULL, nameof(disposables));
			list.Add(d);
		}
		return list;
	}

	public void Dispose()
	{
		lock (_gate)
		{
			if (_disposed)
				return;
			Volatile.Write(ref _disposed, true);
		}

		List<IDisposable?>? currentDisposables;

		lock (_gate)
		{
			DoneStats();
			currentDisposables = _disposables;
			whenDisposed.OnNext(Unit.Default);
			whenDisposed.OnCompleted();
			_disposables = null!;
			Volatile.Write(ref _count, 0);
		}

		for (var i = currentDisposables.Count - 1; i >= 0; i--)
			currentDisposables[i]?.Dispose();
	}



	public void Add(IDisposable item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		lock (_gate)
		{
			if (!_disposed)
			{
				_disposables.Add(item);
				Volatile.Write(ref _count, _count + 1);
				return;
			}
		}

		item.Dispose();
	}

	public bool Remove(IDisposable item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));

		lock (_gate)
		{
			if (_disposed) return false;
			var current = _disposables;
			var i = current.IndexOf(item);
			if (i < 0) return false;
			current[i] = null;
			if (current.Capacity > ShrinkThreshold && _count < current.Capacity / 2)
			{
				var fresh = new List<IDisposable?>(current.Capacity / 2);
				fresh.AddRange(current.Where(e => e != null));
				_disposables = fresh;
			}
			Volatile.Write(ref _count, _count - 1);
		}
		item.Dispose();

		return true;
	}

	public void Clear()
	{
		IDisposable?[] previousDisposables;

		lock (_gate)
		{
			if (_disposed) return;
			var current = _disposables;
			previousDisposables = current.ToArray();
			current.Clear();
			Volatile.Write(ref _count, 0);
		}

		for (var i = previousDisposables.Length - 1; i >= 0; i--)
			previousDisposables[i]?.Dispose();
	}

	public bool Contains(IDisposable item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));

		lock (_gate)
		{
			if (_disposed) return false;
			return _disposables.Contains(item);
		}
	}

	public void CopyTo(IDisposable[] array, int arrayIndex)
	{
		if (array == null) throw new ArgumentNullException(nameof(array));
		if (arrayIndex < 0 || arrayIndex >= array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

		lock (_gate)
		{
			if (_disposed) return;

			if (arrayIndex + _count > array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

			var i = arrayIndex;

			foreach (var d in _disposables)
			{
				if (d != null)
					array[i++] = d;
			}
		}
	}



	private sealed class CompositeEnumerator : IEnumerator<IDisposable>
	{
		private readonly IDisposable?[] _disposables;
		private int _index;

		object IEnumerator.Current => _disposables[_index]!;

		public IDisposable Current => _disposables[_index]!;
		public void Dispose()
		{
			var disposables = _disposables;
			Array.Clear(disposables, 0, disposables.Length);
		}

		public CompositeEnumerator(IDisposable?[] disposables)
		{
			_disposables = disposables;
			_index = -1;
		}

		public bool MoveNext()
		{
			var disposables = _disposables;

			for (; ; )
			{
				var idx = ++_index;
				if (idx >= disposables.Length) return false;
				if (disposables[idx] != null)
					return true;
			}
		}
		public void Reset() => _index = -1;
	}









	/// <summary>
	/// Base class for objects that are both an observable sequence as well as an observer.
	/// </summary>
	/// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
	private abstract class SubjectBase<T> : ISubject<T>, IDisposable
	{
		/// <summary>
		/// Indicates whether the subject has observers subscribed to it.
		/// </summary>
		public abstract bool HasObservers { get; }

		/// <summary>
		/// Indicates whether the subject has been disposed.
		/// </summary>
		public abstract bool IsDisposed { get; }

		/// <summary>
		/// Releases all resources used by the current instance of the subject and unsubscribes all observers.
		/// </summary>
		public abstract void Dispose();

		/// <summary>
		/// Notifies all subscribed observers about the end of the sequence.
		/// </summary>
		public abstract void OnCompleted();

		/// <summary>
		/// Notifies all subscribed observers about the specified exception.
		/// </summary>
		/// <param name="error">The exception to send to all currently subscribed observers.</param>
		/// <exception cref="ArgumentNullException"><paramref name="error"/> is <c>null</c>.</exception>
		public abstract void OnError(Exception error);

		/// <summary>
		/// Notifies all subscribed observers about the arrival of the specified element in the sequence.
		/// </summary>
		/// <param name="value">The value to send to all currently subscribed observers.</param>
		public abstract void OnNext(T value);

		/// <summary>
		/// Subscribes an observer to the subject.
		/// </summary>
		/// <param name="observer">Observer to subscribe to the subject.</param>
		/// <returns>Disposable object that can be used to unsubscribe the observer from the subject.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="observer"/> is <c>null</c>.</exception>
		public abstract IDisposable Subscribe(IObserver<T> observer);
	}


	/// <summary>
	/// Represents the result of an asynchronous operation.
	/// The last value before the OnCompleted notification, or the error received through OnError, is sent to all subscribed observers.
	/// </summary>
	/// <typeparam name="T">The type of the elements processed by the subject.</typeparam>
	private sealed class AsyncSubjectReverse<T> : SubjectBase<T>, INotifyCompletion
	{
		private const string Strings_Linq_NO_ELEMENTS = "AsyncSubjectReverse (No elements).";
		#region Fields

		private AsyncSubjectDisposable[] _observers;
		private T? _value;
		private bool _hasValue;
		private Exception? _exception;

		/// <summary>
		/// A pre-allocated empty array indicating the AsyncSubjectReverse has terminated.
		/// </summary>
		private static readonly AsyncSubjectDisposable[] Terminated = new AsyncSubjectDisposable[0];

		/// <summary>
		/// A pre-allocated empty array indicating the AsyncSubjectReverse has been disposed.
		/// </summary>
		private static readonly AsyncSubjectDisposable[] Disposed = new AsyncSubjectDisposable[0];

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a subject that can only receive one value and that value is cached for all future observations.
		/// </summary>
		public AsyncSubjectReverse() => _observers = Array.Empty<AsyncSubjectDisposable>();

		#endregion

		#region Properties

		/// <summary>
		/// Indicates whether the subject has observers subscribed to it.
		/// </summary>
		public override bool HasObservers => Volatile.Read(ref _observers).Length != 0;

		/// <summary>
		/// Indicates whether the subject has been disposed.
		/// </summary>
		public override bool IsDisposed => Volatile.Read(ref _observers) == Disposed;

		#endregion

		#region Methods

		#region IObserver<T> implementation

		/// <summary>
		/// Notifies all subscribed observers about the end of the sequence, also causing the last received value to be sent out (if any).
		/// </summary>
		public override void OnCompleted()
		{
			for (; ; )
			{
				var observers = Volatile.Read(ref _observers);

				if (observers == Disposed)
				{
					_exception = null;
					ThrowDisposed();
					break;
				}

				if (observers == Terminated)
				{
					break;
				}

				if (Interlocked.CompareExchange(ref _observers, Terminated, observers) == observers)
				{
					var hasValue = _hasValue;

					if (hasValue)
					{
						var value = _value;

						foreach (var observer in observers.Reverse())
						{
							var o = observer.Observer;

							if (o != null)
							{
								o.OnNext(value!);
								o.OnCompleted();
							}
						}
					}
					else
					{
						foreach (var observer in observers.Reverse())
						{
							observer.Observer?.OnCompleted();
						}
					}
				}
			}
		}

		/// <summary>
		/// Notifies all subscribed observers about the exception.
		/// </summary>
		/// <param name="error">The exception to send to all observers.</param>
		/// <exception cref="ArgumentNullException"><paramref name="error"/> is <c>null</c>.</exception>
		public override void OnError(Exception error)
		{
			if (error == null)
			{
				throw new ArgumentNullException(nameof(error));
			}

			for (; ; )
			{
				var observers = Volatile.Read(ref _observers);

				if (observers == Disposed)
				{
					_exception = null;
					_value = default;
					ThrowDisposed();
					break;
				}

				if (observers == Terminated)
				{
					break;
				}

				_exception = error;

				if (Interlocked.CompareExchange(ref _observers, Terminated, observers) == observers)
				{
					foreach (var observer in observers.Reverse())
					{
						observer.Observer?.OnError(error);
					}
				}
			}

		}

		/// <summary>
		/// Sends a value to the subject. The last value received before successful termination will be sent to all subscribed and future observers.
		/// </summary>
		/// <param name="value">The value to store in the subject.</param>
		public override void OnNext(T value)
		{
			var observers = Volatile.Read(ref _observers);

			if (observers == Disposed)
			{
				_value = default;
				_exception = null;
				ThrowDisposed();
				return;
			}

			if (observers == Terminated)
			{
				return;
			}

			_value = value;
			_hasValue = true;
		}

		#endregion

		#region IObservable<T> implementation

		/// <summary>
		/// Subscribes an observer to the subject.
		/// </summary>
		/// <param name="observer">Observer to subscribe to the subject.</param>
		/// <returns>Disposable object that can be used to unsubscribe the observer from the subject.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="observer"/> is <c>null</c>.</exception>
		public override IDisposable Subscribe(IObserver<T> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException(nameof(observer));
			}

			var disposable = default(AsyncSubjectDisposable);
			for (; ; )
			{
				var observers = Volatile.Read(ref _observers);

				if (observers == Disposed)
				{
					_value = default;
					_exception = null;
					ThrowDisposed();

					break;
				}

				if (observers == Terminated)
				{
					var ex = _exception;

					if (ex != null)
					{
						observer.OnError(ex);
					}
					else
					{
						if (_hasValue)
						{
							observer.OnNext(_value!);
						}

						observer.OnCompleted();
					}

					break;
				}

				disposable ??= new AsyncSubjectDisposable(this, observer);

				var n = observers.Length;
				var b = new AsyncSubjectDisposable[n + 1];

				Array.Copy(observers, 0, b, 0, n);

				b[n] = disposable;

				if (Interlocked.CompareExchange(ref _observers, b, observers) == observers)
				{
					return disposable;
				}
			}

			return Disposable.Empty;
		}

		private void Unsubscribe(AsyncSubjectDisposable observer)
		{
			for (; ; )
			{
				var a = Volatile.Read(ref _observers);
				var n = a.Length;

				if (n == 0)
				{
					break;
				}

				var j = Array.IndexOf(a, observer);

				if (j < 0)
				{
					break;
				}

				AsyncSubjectDisposable[] b;

				if (n == 1)
				{
					b = Array.Empty<AsyncSubjectDisposable>();
				}
				else
				{
					b = new AsyncSubjectDisposable[n - 1];

					Array.Copy(a, 0, b, 0, j);
					Array.Copy(a, j + 1, b, j, n - j - 1);
				}

				if (Interlocked.CompareExchange(ref _observers, b, a) == a)
				{
					break;
				}
			}
		}

		/// <summary>
		/// A disposable connecting the AsyncSubjectReverse and an IObserver.
		/// </summary>
		private sealed class AsyncSubjectDisposable : IDisposable
		{
			private AsyncSubjectReverse<T> _subject;
			private volatile IObserver<T>? _observer;

			public AsyncSubjectDisposable(AsyncSubjectReverse<T> subject, IObserver<T> observer)
			{
				_subject = subject;
				_observer = observer;
			}

			public IObserver<T>? Observer => _observer;

			public void Dispose()
			{
				var observer = Interlocked.Exchange(ref _observer, null);
				if (observer == null)
				{
					return;
				}

				_subject.Unsubscribe(this);
				_subject = null!;
			}
		}

		#endregion

		#region IDisposable implementation

		private static void ThrowDisposed() => throw new ObjectDisposedException(string.Empty);

		/// <summary>
		/// Unsubscribe all observers and release resources.
		/// </summary>
		public override void Dispose()
		{
			if (Interlocked.Exchange(ref _observers, Disposed) != Disposed)
			{
				_exception = null;
				_value = default;
				_hasValue = false;
			}
		}

		#endregion

		#region Await support

		/// <summary>
		/// Gets an awaitable object for the current AsyncSubjectReverse.
		/// </summary>
		/// <returns>Object that can be awaited.</returns>
		public AsyncSubjectReverse<T> GetAwaiter() => this;

		/// <summary>
		/// Specifies a callback action that will be invoked when the subject completes.
		/// </summary>
		/// <param name="continuation">Callback action that will be invoked when the subject completes.</param>
		/// <exception cref="ArgumentNullException"><paramref name="continuation"/> is <c>null</c>.</exception>
		public void OnCompleted(Action continuation)
		{
			if (continuation == null)
			{
				throw new ArgumentNullException(nameof(continuation));
			}

			//
			// [OK] Use of unsafe Subscribe: this type's Subscribe implementation is safe.
			//
			Subscribe/*Unsafe*/(new AwaitObserver(continuation));
		}

		private sealed class AwaitObserver : IObserver<T>
		{
			private readonly SynchronizationContext? _context;
			private readonly Action _callback;

			public AwaitObserver(Action callback)
			{
				_context = SynchronizationContext.Current;
				_callback = callback;
			}

			public void OnCompleted() => InvokeOnOriginalContext();

			public void OnError(Exception error) => InvokeOnOriginalContext();

			public void OnNext(T value) { }

			private void InvokeOnOriginalContext()
			{
				if (_context != null)
				{
					//
					// No need for OperationStarted and OperationCompleted calls here;
					// this code is invoked through await support and will have a way
					// to observe its start/complete behavior, either through returned
					// Task objects or the async method builder's interaction with the
					// SynchronizationContext object.
					//
					_context.Post(static c => ((Action)c!)(), _callback);
				}
				else
				{
					_callback();
				}
			}
		}

		/// <summary>
		/// Gets whether the AsyncSubjectReverse has completed.
		/// </summary>
		public bool IsCompleted => Volatile.Read(ref _observers) == Terminated;

		/// <summary>
		/// Gets the last element of the subject, potentially blocking until the subject completes successfully or exceptionally.
		/// </summary>
		/// <returns>The last element of the subject. Throws an InvalidOperationException if no element was received.</returns>
		/// <exception cref="InvalidOperationException">The source sequence is empty.</exception>
		public T GetResult()
		{
			if (Volatile.Read(ref _observers) != Terminated)
			{
				using var e = new ManualResetEventSlim(initialState: false);

				//
				// [OK] Use of unsafe Subscribe: this type's Subscribe implementation is safe.
				//
				Subscribe/*Unsafe*/(new BlockingObserver(e));

				e.Wait();
			}

			if (_exception != null)
				throw _exception;

			if (!_hasValue)
			{
				throw new InvalidOperationException(Strings_Linq_NO_ELEMENTS);
			}

			return _value!;
		}

		private sealed class BlockingObserver : IObserver<T>
		{
			private readonly ManualResetEventSlim _e;

			public BlockingObserver(ManualResetEventSlim e) => _e = e;

			public void OnCompleted() => Done();

			public void OnError(Exception error) => Done();

			public void OnNext(T value) { }

			private void Done() => _e.Set();
		}

		#endregion

		#endregion
	}
}












static class DispStats
{
	public static void SetBP(int id) => bpId = id;

	public static Action? OnBPHit { get; set; }

	public static bool Log()
	{
		L("");
		var arr = map.Values.OrderBy(e => e.Id).ToArray();
		var title = arr.Length switch
		{
			0 => "All Disps released",
			_ => $"{arr.Length} unreleased Disp{(arr.Length == 1 ? "" : "s")}"
		};
		L(title);
		L(new string('=', title.Length));
		foreach (var item in arr)
			L($"{item}");
		L("");
		return arr.Length == 0;
	}




	private sealed record VarNfo(int Id, string? Expr)
	{
		public override string ToString() => $"[{Id}, {ExprStr}]";
		private string ExprStr => Expr ?? "_";
	}

	private static readonly ConcurrentDictionary<int, VarNfo> map = new();
	private static int? bpId;
	private static int dispIdIdx;

	internal static int GetNextDispId() => Interlocked.Increment(ref dispIdIdx);
	internal static void ClearForTests()
	{
		dispIdIdx = 0;
		map.Clear();
	}


	internal static void DispCreated(int id, string? dbgExpr)
	{
		if (bpId == id) OnBPHit?.Invoke();
		if (map.ContainsKey(id)) throw new ArgumentException("Invalid Disp.id");
		map[id] = new VarNfo(id, dbgExpr);
	}

	internal static void DispDisposed(int id)
	{
		if (!map.ContainsKey(id)) throw new ArgumentException("Invalid Disp.id");
		map.TryRemove(id, out _);
	}

	private static void L(string s) => Console.WriteLine(s);
}








public static class VarDbg
{
	/// <summary>
	/// Call this at the end of your program to check if you forgot to call Dispose() on some Disps
	/// </summary>
	/// <param name="pauseOnIssue">if true, then in case of an issue, wait for a key press before exiting</param>
	/// <returns>true if there were undisposed Disps</returns>
	public static bool CheckForUndisposedDisps(bool pauseOnIssue = false)
	{
		DisposeExtensions.DisposeExitD();
		var isOk = DispStats.Log();
		if (pauseOnIssue && !isOk)
			Console.ReadKey();
		return !isOk;
	}

	/// <summary>
	/// If a call to CheckForUndisposedDisps prints some undisposed Disps, you can use their printed allocId to breakpoint when they are created the next time the program is run
	/// </summary>
	/// <param name="allocId">Disp allocId as printed by CheckForUndisposedDisps</param>
	public static void BreakpointOnDispAlloc(int allocId)
	{
		DispStats.SetBP(allocId);
		DispStats.OnBPHit = Debugger.Break;
	}

	/// <summary>
	/// Clears the undisposed Disps counters <br/>
	/// Call this on tests [SetUp] to make them independent
	/// </summary>
	public static void ClearUndisposedCountersForTest() => DispStats.ClearForTests();
}




static class DisposeExtensions
{
	private static readonly Lazy<Disp> exitD = new(() => new Disp());

	internal static void DisposeExitD()
	{
		if (exitD.IsValueCreated)
			exitD.Value.Dispose();
	}

	static DisposeExtensions()
	{
		AppDomain.CurrentDomain.ProcessExit += (_, _) =>
		{
			DisposeExitD();
		};
	}
}

