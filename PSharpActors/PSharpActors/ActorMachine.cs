﻿//-----------------------------------------------------------------------
// <copyright file="ActorMachine.cs">
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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.PSharp.Actors
{
    /// <summary>
    /// A P# actor machine.
    /// </summary>
    public abstract class ActorMachine : Machine
    {
        #region events

        /// <summary>
        /// The actor initialization event.
        /// </summary>
        public class InitEvent : Event
        {
            public object ClassInstance;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="classInstance">ClassInstance</param>
            public InitEvent(object classInstance)
            {
                this.ClassInstance = classInstance;
            }
        }

        /// <summary>
        /// The actor invocation event.
        /// </summary>
        public class ActorEvent : Event
        {
            public Type MethodClass;
            public string MethodName;
            public object ClassInstance;
            public object[] Parameters;
            public Action<object> SetResultAction;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="methodClass">MethodClass</param>
            /// <param name="methodName">MethodName</param>
            /// <param name="classInstance">ClassInstance</param>
            /// <param name="parameters">Parameters</param>
            /// <param name="tcs">TaskCompletionSource</param>
            public ActorEvent(Type methodClass, string methodName, object classInstance,
                object[] parameters, Action<object> setResultAction)
            {
                this.MethodClass = methodClass;
                this.MethodName = methodName;
                this.ClassInstance = classInstance;
                this.Parameters = parameters;
                this.SetResultAction = setResultAction;
            }
        }

        #endregion

        #region fields

        /// <summary>
        /// Map from task ids to result tasks.
        /// </summary>
        IDictionary<int, object> ResultTaskMap;

        #endregion

        #region states

        [Start]
        [OnEventDoAction(typeof(InitEvent), nameof(OnInitEvent))]
        [OnEventDoAction(typeof(ActorEvent), nameof(OnActorEvent))]
        private class Init : MachineState { }

        #endregion

        #region actions

        private void OnInitEvent()
        {
            var initEvent = this.ReceivedEvent as InitEvent;

            try
            {
                this.Initialize(initEvent);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Environment.Exit(Environment.ExitCode);
            }            
        }

        protected abstract void Initialize(InitEvent initEvent);

        private void OnActorEvent()
        {
            var e = (this.ReceivedEvent as ActorEvent);
            MethodInfo mi = e.MethodClass.GetMethod(e.MethodName);
            try
            {
                // TODO: check if we can associate this task with the
                // dummy task returned to the user
                object result = mi.Invoke(e.ClassInstance, e.Parameters);
                e.SetResultAction(result);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                Environment.Exit(Environment.ExitCode);
            }
        }

        #endregion
    }
}
