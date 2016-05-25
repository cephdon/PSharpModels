﻿using ServiceFabricModel;
using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.PSharp;
using Microsoft.PSharp.Actors;
using Microsoft.PSharp.Actors.Bridge;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Microsoft.ServiceFabric.Actors
{
    public class ActorProxy
    {
        private static readonly ProxyFactory<Actor> ProxyFactory;
        private static Dictionary<ActorId, Object> IdMap;

        static ActorProxy()
        {
            ProxyFactory = new ProxyFactory<Actor>(new HashSet<string> { "Microsoft.ServiceFabric.Actors" });
            IdMap = new Dictionary<ActorId, object>();
        }

        public static TActorInterface Create<TActorInterface>(ActorId actorId, string applicationName = null,
            string serviceName = null) where TActorInterface : IActor
        {
            if (IdMap.ContainsKey(actorId))
            {
                return (TActorInterface)IdMap[actorId];
            }

            if (ActorModel.Runtime == null)
            {
                ActorModel.Initialize(PSharpRuntime.Create());
            }

            string assemblyPath = Assembly.GetEntryAssembly().Location + "\\..\\..\\..\\..";

            Type proxyType = ProxyFactory.GetProxyType(typeof(TActorInterface),
                typeof(FabricActorMachine), assemblyPath);
            var res = (TActorInterface)Activator.CreateInstance(proxyType);
            IdMap.Add(actorId, res);

            return res;
        }
    }
}
