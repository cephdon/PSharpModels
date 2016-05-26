﻿using Microsoft.ServiceFabric.Actors.Runtime;
using Receiver.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Receiver
{
    public class Receiver : Actor, IReceiver
    {
        public Task<int> GetFinalCount()
        {
            return Task.FromResult(this.StateManager.GetStateAsync<int>("itemCount").Result);
        }

        public Task StartTransaction()
        {
            return this.StateManager.AddStateAsync<int>("itemCount", 0);
        }

        public Task TransmitData(TransactionData.TransactionItems item)
        {
            Console.WriteLine(item.name);
            int count = this.StateManager.GetStateAsync<int>("itemCount").Result;
            count++;
            this.StateManager.SetStateAsync<int>("itemCount", count);
            return Task.FromResult(true);
        }
    }
}
