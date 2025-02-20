﻿using System.Collections.Generic;
using System.Linq;
using GW2EIEvtcParser.EIData;
using GW2EIEvtcParser.Extensions;
using GW2EIEvtcParser.ParsedData;
using static GW2EIEvtcParser.EncounterLogic.EncounterCategory;

namespace GW2EIEvtcParser.EncounterLogic
{
    internal class UnknownFightLogic : FightLogic
    {
        public UnknownFightLogic(int triggerID) : base(triggerID)
        {
            Extension = "boss";
            Icon = "https://wiki.guildwars2.com/images/d/d2/Guild_emblem_004.png";
            EncounterCategoryInformation.Category = FightCategory.UnknownEncounter;
            EncounterCategoryInformation.SubCategory = SubFightCategory.UnknownEncounter;
        }

        protected override HashSet<int> GetUniqueNPCIDs()
        {
            return new HashSet<int>();
        }

        internal override void ComputeFightTargets(AgentData agentData, List<CombatItem> combatItems, IReadOnlyDictionary<uint, AbstractExtensionHandler> extensions)
        {
            int id = GetTargetsIDs().First();
            AgentItem agentItem = agentData.GetNPCsByID(id).FirstOrDefault();
            // Trigger ID is not NPC
            if (agentItem == null)
            {
                agentItem = agentData.GetGadgetsByID(id).FirstOrDefault();
                if (agentItem != null)
                {
                    _targets.Add(new NPC(agentItem));
                }
            }
            else
            {
                _targets.Add(new NPC(agentItem));
            }
            //
            TargetAgents = new HashSet<AgentItem>(_targets.Select(x => x.AgentItem));
            NonPlayerFriendlyAgents = new HashSet<AgentItem>(_nonPlayerFriendlies.Select(x => x.AgentItem));
            TrashMobAgents = new HashSet<AgentItem>(_trashMobs.Select(x => x.AgentItem));
        }
    }
}
