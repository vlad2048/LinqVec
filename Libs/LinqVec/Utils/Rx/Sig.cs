using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace LinqVec.Utils.Rx;


static class Sig
{
    public static (Action<T>, IObservable<T>) Make<T>()
    {
        ISubject<T> when = new AsyncSubject<T>();
        return (
            v =>
            {
                when.OnNext(v);
                when.OnCompleted();
            },
            when.AsObservable()
        );
    }
}
