﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YGOProAnalyticsServer.DTOs.Interfaces
{
    public interface IEffectMonsterContainer
    {
        List<MonsterCardDTO> EffectMonsters { get; set; }
    }
}
