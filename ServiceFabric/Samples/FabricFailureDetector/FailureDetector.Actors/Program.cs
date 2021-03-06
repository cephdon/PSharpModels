﻿using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace FailureDetector.Actors
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // This line registers an Actor Service to host your actor class with the Service Fabric runtime.
                // The contents of your ServiceManifest.xml and ApplicationManifest.xml files
                // are automatically populated when you build this project.
                // For more information, see http://aka.ms/servicefabricactorsplatform

                ActorRuntime.RegisterActorAsync<Driver>(
                   (context, actorType) => new ActorService(context, actorType, () => new Driver())).GetAwaiter().GetResult();
                ActorRuntime.RegisterActorAsync<FailureDetector>(
                   (context, actorType) => new ActorService(context, actorType, () => new FailureDetector())).GetAwaiter().GetResult();
                ActorRuntime.RegisterActorAsync<Node>(
                   (context, actorType) => new ActorService(context, actorType, () => new Node())).GetAwaiter().GetResult();
                ActorRuntime.RegisterActorAsync<SafetyMonitor>(
                   (context, actorType) => new ActorService(context, actorType, () => new SafetyMonitor())).GetAwaiter().GetResult();

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
