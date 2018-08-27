using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WallpaperLens.Utilities
{
    public class Scheduler
    {
        //keep a list of scheduled actions
        private Dictionary<Action, CancellationTokenSource> _scheduledActions = new Dictionary<Action, CancellationTokenSource>();

        //schedule an action
        public async Task Run(Action action, TimeSpan period)
        {
            if (_scheduledActions.ContainsKey(action)) return; //this action is already scheduled, get out

            var cancellationTokenSrc = new CancellationTokenSource(); //create cancellation toket so we can abort later when we want

            _scheduledActions.Add(action, cancellationTokenSrc); //save it all into our disctionary

            //main scheduling loop
            Task task = null;
            while (!cancellationTokenSrc.IsCancellationRequested)
            {
                if (task == null || task.IsCompleted) //skip if previous invocation is still running (our requirements allow that)
                    task = Task.Run(action);

                await Task.Delay(period, cancellationTokenSrc.Token);
            }
        }

        public void Stop(Action action)
        {
            if (!_scheduledActions.ContainsKey(action)) return; //this method has not been scheduled, get out
            _scheduledActions[action].Cancel(); //stop the task
            _scheduledActions.Remove(action); //remove from the collection
        }
    }
}
