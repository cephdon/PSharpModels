﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;

using SmartHome.Interfaces;

using Microsoft.PSharp.Actors;

namespace SmartHome.Actors
{
    public class Thief : Actor, IThief
    {
        private IHouse House;
        private IGarden Garden;
        private IKitchen Kitchen;
        private IBedroom Bedroom;

        private IActorTimer MovementTimer;
        private IActorTimer ActionTimer;

        protected override Task OnActivateAsync()
        {
            if (!(this.StateManager.ContainsStateAsync("CurrentLocation").Result))
            {
                this.StateManager.AddStateAsync("CurrentLocation", Location.Outside);
            }
            
            this.House = ActorProxy.Create<IHouse>(new ActorId(100), "fabric:/FabricSmartHome");
            this.Garden = ActorProxy.Create<IGarden>(new ActorId(101), "fabric:/FabricSmartHome");
            this.Kitchen = ActorProxy.Create<IKitchen>(new ActorId(102), "fabric:/FabricSmartHome");
            this.Bedroom = ActorProxy.Create<IBedroom>(new ActorId(103), "fabric:/FabricSmartHome");

            this.MovementTimer = this.RegisterTimer(HandleMovementTimeout, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5));
            this.ActionTimer = this.RegisterTimer(HandleActionTimeout, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3));

            return base.OnActivateAsync();
        }

        public Task Enter(Location location)
        {
            this.StateManager.SetStateAsync("CurrentLocation", location);
            return new Task(() => { });
        }

        public Task HandleMovementTimeout(object args)
        {
            var previousLocation = this.StateManager.GetStateAsync<Location>("CurrentLocation").Result;
            var taskLocation = this.House.GotoRoom();
            var location = ActorModel.GetResult<Location>(taskLocation);

            ActorModel.Log("[LOG] Thief tries to enter room {0}", location);
            bool canEnter = false;
            if (location == Location.Garden)
            {
                var task = this.Bedroom.TryEnterRoom();
                canEnter = ActorModel.GetResult<bool>(task);
            }
            else if (location == Location.Kitchen)
            {
                var task = this.Bedroom.TryEnterRoom();
                canEnter = ActorModel.GetResult<bool>(task);
            }
            else if (previousLocation == Location.Kitchen &&
                location == Location.Bedroom)
            {
                var task = this.Bedroom.TryEnterRoom();
                canEnter = ActorModel.GetResult<bool>(task);
            }

            if (canEnter)
            {
                ActorModel.Log("[LOG] Thief entered room {0}", location);
                this.StateManager.SetStateAsync("CurrentLocation", location);
            }

            return new Task(() => { });
        }

        public Task HandleActionTimeout(object args)
        {
            var location = this.StateManager.GetStateAsync<Location>("CurrentLocation").Result;
            if (location == Location.Garden)
            {

            }
            else if (location == Location.Kitchen)
            {

            }
            else if (location == Location.Bedroom)
            {
                this.Bedroom.TryToStealMoney();
            }

            return new Task(() => { });
        }

        protected override Task OnDeactivateAsync()
        {
            this.UnregisterTimer(this.MovementTimer);
            this.UnregisterTimer(this.ActionTimer);

            return base.OnDeactivateAsync();
        }
    }
}