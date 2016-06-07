﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace SmartHome.Interfaces
{
    public interface IHouse : IActor
    {
        Task<Location> GotoRoom();
    }
}
