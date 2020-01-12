using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OmniCore.Model.Enumerations;
using OmniCore.Model.Exceptions;
using OmniCore.Model.Interfaces.Platform.Common;

namespace OmniCore.Eros
{
    public abstract class ErosTask : ITask
    {
        private readonly CancellationTokenSource TaskCancellationSource = new CancellationTokenSource();
        private readonly ISubject<ITask> CannotCancelSubject = new AsyncSubject<ITask>();
        private readonly ISubject<ITask> StartedSubject = new AsyncSubject<ITask>();
        private readonly ISubject<ITask> FinishedSubject = new AsyncSubject<ITask>();
        private readonly ISubject<Exception> FailedSubject = new AsyncSubject<Exception>();
        private readonly ISubject<ITask> CanceledSubject = new AsyncSubject<ITask>();
        private readonly ISubject<ITask> MadeRedundantSubject = new AsyncSubject<ITask>();
        private readonly ISubject<ITask> ResultLinkedSubject = new Subject<ITask>();
        private readonly ISubject<ITask> RescheduledSubject = new Subject<ITask>();

        public ErosTask()
        {
        }

        private bool CanCancelInternal = true;

        public bool CanCancel
        {
            get => CanCancelInternal;
            set
            {
                if (!value)
                {
                    if (CanCancelInternal)
                    {
                        CannotCancelSubject.OnNext(this);
                        CannotCancelSubject.OnCompleted();
                        CanCancelInternal = false;
                    }
                }
                else
                {
                    throw new OmniCoreWorkflowException(FailureType.Internal, "Cancellable state cannot be reverted");
                }
            }
        }

        public async Task Run()
        {

            StartedSubject.OnNext(this);
            StartedSubject.OnCompleted();

            try
            {
                await ExecuteRequest(TaskCancellationSource.Token);
            }
            catch (OperationCanceledException oe)
            {
                CanceledSubject.OnNext(this);
                CanceledSubject.OnCompleted();
            }
            catch (Exception e)
            {
                FailedSubject.OnNext(e);
                FailedSubject.OnCompleted();
            }

            FinishedSubject.OnNext(this);
            FinishedSubject.OnCompleted();
        }

        protected abstract Task ExecuteRequest(CancellationToken cancellationToken);

        public void RequestCancellation()
        {
            TaskCancellationSource.Cancel();
        }

        public IObservable<ITask> WhenCannotCancel()
        {
            return CannotCancelSubject;
        }

        public IObservable<ITask> WhenStarted()
        {
            return StartedSubject;
        }

        public IObservable<ITask> WhenFinished()
        {
            return FinishedSubject;
        }

        public IObservable<Exception> WhenFailed()
        {
            return FailedSubject;
        }

        public IObservable<ITask> WhenCanceled()
        {
            return CanceledSubject;
        }

        public IObservable<ITask> WhenMadeRedundant()
        {
            return MadeRedundantSubject;
        }

        public IObservable<ITask> WhenResultLinked()
        {
            return ResultLinkedSubject;
        }

        public IObservable<ITask> WhenRescheduled()
        {
            return RescheduledSubject;
        }
        public void Dispose()
        {
            TaskCancellationSource?.Dispose();
        }

    }
}
