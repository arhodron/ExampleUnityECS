﻿using Unity.Entities;

namespace Game.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class CollisionSystemGroup : ComponentSystemGroup
    {

    }
}