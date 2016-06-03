﻿//-----------------------------------------------------------------------
// <copyright file="ActorBase.cs">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// 
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//      EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//      MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//      IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//      CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//      TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//      SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.PSharp;
using Microsoft.PSharp.Actors;
using Microsoft.PSharp.Actors.Bridge;
using ServiceFabricModel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.Actors.Runtime
{
    public abstract class ActorBase
    {
        //
        // Summary:
        //     Gets the stateful service replica that is hosting the actor.
        //public ActorService ActorService { get; }
        //
        // Summary:
        //     Gets the name of the application that contains the actor service that is hosting
        //     this actor.
        public string ApplicationName { get; }
        //
        // Summary:
        //     Gets the identity of this actor with the actor service.
        public ActorId Id { get; internal set; }

        //
        // Summary:
        //     Gets the Uri of the actor service that is hosting this actor.
        public Uri ServiceUri { get; }

        //
        // Summary:
        //     Gets the event for the specified event interface.
        //
        // Type parameters:
        //   TEvent:
        //     Event interface type.
        //
        // Returns:
        //     Returns Event that represents the specified interface.
        //protected TEvent GetEvent<TEvent>();
        //
        // Summary:
        //     Gets the actor reminder with specified reminder name.
        //
        // Parameters:
        //   reminderName:
        //     Name of the reminder to get.
        //
        // Returns:
        //     An Microsoft.ServiceFabric.Actors.Runtime.IActorReminder that represents an actor
        //     reminder.
        //protected IActorReminder GetReminder(string reminderName);
        //
        // Summary:
        //     Override this method to initialize the members, initialize state or register
        //     timers. This method is called right after the actor is activated and before any
        //     method call or reminders are dispatched on it.
        //
        // Returns:
        //     A System.Threading.Tasks.Task that represents outstanding OnActivateAsync operation.
        protected virtual Task OnActivateAsync()
        {
            return Task.FromResult(true);
        }
        //
        // Summary:
        //     Override this method to release any resources including unregistering the timers.
        //     This method is called right before the actor is deactivated.
        //
        // Returns:
        //     A System.Threading.Tasks.Task that represents outstanding OnDeactivateAsync operation.
        protected virtual Task OnDeactivateAsync()
        {
            return Task.FromResult(true);
        }
        //
        // Summary:
        //     Registers the specified reminder with actor.
        //
        // Parameters:
        //   reminderName:
        //     Name of the reminder to register.
        //
        //   state:
        //     State associated with reminder.
        //
        //   dueTime:
        //     A System.TimeSpan representing the amount of time to delay before firing the
        //     reminder. Specify negative one (-1) milliseconds to prevent reminder from firing.
        //     Specify zero (0) to fire the reminder immediately.
        //
        //   period:
        //     The time interval between firing of reminders. Specify negative one (-1) milliseconds
        //     to disable periodic firing.
        //
        // Returns:
        //     A task that represents the asynchronous registration operation. The value of
        //     TResult parameter is an Microsoft.ServiceFabric.Actors.Runtime.IActorReminder
        //     that represents the actor reminder that was registered.
        //[AsyncStateMachine(typeof(< RegisterReminderAsync > d__3))]
        //[DebuggerStepThrough]
        protected Task<IActorReminder> RegisterReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            var reminders = ActorModel.GetReminders(ActorModel.Runtime.GetCurrentMachine());
            var reminder = reminders.SingleOrDefault(val => ((ActorReminder)val).Name.Equals(reminderName));

            var task = new ActorCompletionTask<IActorReminder>();
            var actorCompletionMachine = task.ActorCompletionMachine;
            if (reminder != null)
            {
                ActorModel.Runtime.SendEvent(actorCompletionMachine,
                    new ActorCompletionMachine.SetResultRequest(reminder));
            }
            else
            {
                ActorModel.Runtime.CreateMachine(typeof(ActorReminderMachine), reminderName,
                    new ReminderMachine.InitEvent(ActorModel.Runtime.GetCurrentMachine(),
                    actorCompletionMachine, reminderName, null));
            }

            return task;
        }
        //
        // Summary:
        //     Registers a Timer for the actor.
        //
        // Parameters:
        //   asyncCallback:
        //     Callback to invoke when timer fires.
        //
        //   state:
        //     State to pass into timer callback.
        //
        //   dueTime:
        //     TimeSpan when actor timer is first due.
        //
        //   period:
        //     TimeSpan for subsequent actor timer invocation.
        //
        // Returns:
        //     Returns IActorTimer object.
        protected IActorTimer RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
        {
            MachineId timer = ActorModel.Runtime.CreateMachine(typeof(TimerMachine),
                new TimerMachine.InitEvent(ActorModel.Runtime.GetCurrentMachine(),
                asyncCallback, state));
            return new ActorTimer(dueTime, period, ActorModel.Runtime.GetCurrentMachine(), timer);
        }

        //
        // Summary:
        //     Gets the actor reminder with specified reminder name.
        //
        // Parameters:
        //   reminderName:
        //     Name of the reminder to get.
        //
        // Returns:
        //     An Microsoft.ServiceFabric.Actors.Runtime.IActorReminder that represents an actor
        //     reminder.
        protected IActorReminder GetReminder(string reminderName)
        {
            var reminders = ActorModel.GetReminders(ActorModel.Runtime.GetCurrentMachine());
            var reminder = reminders.SingleOrDefault(val => ((ActorReminder)val).Name.Equals(reminderName));

            return (IActorReminder)reminder;
        }

        //
        // Summary:
        //     Unregisters the specified reminder with actor.
        //
        // Parameters:
        //   reminder:
        //     The actor reminder to unregister.
        //
        // Returns:
        //     A task that represents the asynchronous unregister operation.
        //
        // Exceptions:
        //   T:System.Fabric.FabricException:
        //     When the specified reminder is not registered with actor.
        //[AsyncStateMachine(typeof(< UnregisterReminderAsync > d__0))]
        //[DebuggerStepThrough]
        protected Task UnregisterReminderAsync(IActorReminder reminder)
        {
            var reminders = ActorModel.GetReminders(ActorModel.Runtime.GetCurrentMachine());
            var reminderToBeRemoved = reminders.SingleOrDefault(val
                => ((ActorReminder)val).Name.Equals(reminder.Name));
            if (reminderToBeRemoved != null)
            {
                reminderToBeRemoved.Dispose();
            }

            var task = new ActorCompletionTask<object>();
            var actorCompletionMachine = task.ActorCompletionMachine;
            ActorModel.Runtime.SendEvent(actorCompletionMachine,
                new ActorCompletionMachine.SetResultRequest(true));
            return task;
        }
        //
        // Summary:
        //     Unregisters a Timer previously set on this actor.
        //
        // Parameters:
        //   timer:
        //     IActorTimer representing timer that needs to be unregistered..
        protected void UnregisterTimer(IActorTimer timer)
        {
            ((ActorTimer)timer).Dispose();
        }
    }
}
