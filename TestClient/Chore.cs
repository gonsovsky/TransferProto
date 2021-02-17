using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Atoll.TransferService;
using Corallite.Chores;
using Corallite.Chores.Implementation;

namespace TestClient
{
    public class ChoreX
    {
        public ChoreX()
        {
            var profiler = new ChoreProfiler();
            var scheduler = new CustomTaskScheduler();
            Task.Factory.StartNew(() => SomeMethod(), CancellationToken.None, TaskCreationOptions.None, scheduler);
         
            TaskCreationOptions opts = new TaskCreationOptions();
            var overSettings = new Corallite.Chores.ChoreReadOnlySettings(
                "hot", scheduler, opts, TimeSpan.FromSeconds(3), ChoreRetryPolicy.NoRetry, false, false);
            var settingsDefault = new Corallite.Chores.ChoreListReadOnlySettings("hot",1, overSettings);
            var settingsCustom = new Corallite.Chores.ChoreListReadOnlySettings("hot", 1, overSettings);

            var service = new ChoreService(settingsDefault, settingsCustom, 1, profiler);
            var chore = new HotChore();
            
            service.Start();
        }

        public void SomeMethod()
        {
            Misc.Log(DateTime.Now.ToString());
        }
    }

    public sealed class CustomTaskScheduler : TaskScheduler, IDisposable
    {
        private BlockingCollection<Task> tasksCollection = new BlockingCollection<Task>();
        private readonly Thread mainThread = null;
        public CustomTaskScheduler()
        {
            mainThread = new Thread(new ThreadStart(Execute));
            if (!mainThread.IsAlive)
            {
                mainThread.Start();
            }
        }
        private void Execute()
        {
            foreach (var task in tasksCollection.GetConsumingEnumerable())
            {
                TryExecuteTask(task);
            }
        }
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return tasksCollection.ToArray();
        }
        protected override void QueueTask(Task task)
        {
            if (task != null)
                tasksCollection.Add(task);
        }
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }
        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            tasksCollection.CompleteAdding();
            tasksCollection.Dispose();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }


    public class ChoreFactory : IChoreFactory
    {
        public Chore Create(bool isScheduled)
        {
            throw new NotImplementedException();
        }
    }

    public class ChoreProfiler : Corallite.Chores.IChoreProfilingUnit
    {
        public void ServiceProcessingThreadException(Exception exception)
        {
            Misc.Log($"IChoreProfilingUnit.ServiceProcessingThreadException {exception.Message}");
        }

        public void ServiceStartException(Exception exception)
        {
            Misc.Log($"IChoreProfilingUnit.ServiceStartException {exception.Message}");
        }
    }

    public class HotChore : Corallite.Chores.Chore
    {
        public override ChoreResult Execute(IChoreContext context)
        {
            Misc.Log($"Chore.Execute {context.ChoreInstanceId}");
            return ChoreResult.Success();
        }

        protected override void Dispose(bool disposing)
        {
            Misc.Log($"Chore.Dispose");
        }
    }
}
