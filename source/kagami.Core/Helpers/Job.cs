﻿using System;
using FFXIV_ACT_Plugin.Common.Models;

namespace kagami.Helpers
{
    public enum Job
    {
        Unknown = -1,
        ADV = 0,
        GLA = 1,
        PUG = 2,
        MRD = 3,
        LNC = 4,
        ARC = 5,
        CNJ = 6,
        THM = 7,
        CRP = 8,
        BSM = 9,
        ARM = 10,
        GSM = 11,
        LTW = 12,
        WVR = 13,
        ALC = 14,
        CUL = 15,
        MIN = 16,
        BTN = 17,
        FSH = 18,
        PLD = 19,
        MNK = 20,
        WAR = 21,
        DRG = 22,
        BRD = 23,
        WHM = 24,
        BLM = 25,
        ACN = 26,
        SMN = 27,
        SCH = 28,
        ROG = 29,
        NIN = 30,
        MCH = 31,
        DRK = 32,
        AST = 33,
        SAM = 34,
        RDM = 35,
        BLU = 36,
        GNB = 37,
        DNC = 38,
    }

    public static class CombatantExtensions
    {
        public static Job GetJob(
            this Combatant c)
            => (Job)Enum.ToObject(typeof(Job), c.Job);
    }
}
