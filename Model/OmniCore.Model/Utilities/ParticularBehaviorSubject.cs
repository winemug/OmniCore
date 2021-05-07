using System;
using System.Reactive.Subjects;

namespace OmniCore.Model.Utilities
{
    public class ParticularBehaviorSubject<T> : ISubject<T>
    {
        private T CurrentValue;
        private readonly BehaviorSubject<T> EncapsulatedBehaviorSubject;

        public ParticularBehaviorSubject(T value)
        {
            CurrentValue = value;
            EncapsulatedBehaviorSubject = new BehaviorSubject<T>(value);
        }

        public void OnCompleted()
        {
            EncapsulatedBehaviorSubject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            EncapsulatedBehaviorSubject.OnError(error);
        }

        public void OnNext(T value)
        {
            if (value == null && CurrentValue == null)
                return;
            if (value != null && value.Equals(CurrentValue))
                return;

            CurrentValue = value;
            EncapsulatedBehaviorSubject.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return EncapsulatedBehaviorSubject.Subscribe(observer);
        }
    }
}