﻿using System.Collections.Generic;
using System.Linq;
using GW2EIEvtcParser;
using GW2EIEvtcParser.EIData;

namespace GW2EIBuilders.HtmlModels.HTMLCharts
{
    internal class BuffChartDataDto
    {
        public long Id { get; set; }
        public string Color { get; set; }
        public bool Visible { get; set; }
        public List<object[]> States { get; set; }

        private static string GetBuffColor(string name)
        {
            switch (name)
            {

                case "Aegis": return "rgb(102,255,255)";
                case "Fury": return "rgb(255,153,0)";
                case "Might": return "rgb(153,0,0)";
                case "Protection": return "rgb(102,255,255)";
                case "Quickness": return "rgb(255,0,255)";
                case "Regeneration": return "rgb(0,204,0)";
                case "Resistance": return "rgb(255, 153, 102)";
                case "Retaliation": return "rgb(255, 51, 0)";
                case "Resolution": return "rgb(255, 51, 0)";
                case "Stability": return "rgb(153, 102, 0)";
                case "Swiftness": return "rgb(255,255,0)";
                case "Vigor": return "rgb(102, 153, 0)";
                case "Alacrity": return "rgb(0,102,255)";

                case "Glyph of Empowerment": return "rgb(204, 153, 0)";
                case "Sun Spirit": return "rgb(255, 102, 0)";
                case "Banner of Strength": return "rgb(153, 0, 0)";
                case "Banner of Discipline": return "rgb(0, 51, 0)";
                case "Spotter": return "rgb(0,255,0)";
                case "Stone Spirit": return "rgb(204, 102, 0)";
                case "Storm Spirit": return "rgb(102, 0, 102)";
                case "Empower Allies": return "rgb(255, 153, 0)";
                default:
                    return "";
            }

        }

        private BuffChartDataDto(BuffsGraphModel bgm, List<Segment> bChart, PhaseData phase)
        {
            Id = bgm.Buff.ID;
            Visible = (bgm.Buff.Name == "Might" || bgm.Buff.Name == "Quickness" || bgm.Buff.Name == "Vulnerability");
            Color = GetBuffColor(bgm.Buff.Name);
            States = Segment.ToObjectList(bChart, phase.Start, phase.End);
        }

        private static BuffChartDataDto BuildBuffGraph(BuffsGraphModel bgm, PhaseData phase, Dictionary<long, Buff> usedBuffs)
        {
            var bChart = bgm.BuffChart.Where(x => x.End >= phase.Start && x.Start <= phase.End
            ).ToList();
            if (bChart.Count == 0 || (bChart.Count == 1 && bChart.First().Value == 0))
            {
                return null;
            }
            usedBuffs[bgm.Buff.ID] = bgm.Buff;
            return new BuffChartDataDto(bgm, bChart, phase);
        }

        private static void BuildBoonGraphData(List<BuffChartDataDto> list, IReadOnlyList<Buff> listToUse, Dictionary<long, BuffsGraphModel> boonGraphData, PhaseData phase, Dictionary<long, Buff> usedBuffs)
        {
            foreach (Buff buff in listToUse)
            {
                if (boonGraphData.TryGetValue(buff.ID, out BuffsGraphModel bgm))
                {
                    BuffChartDataDto graph = BuildBuffGraph(bgm, phase, usedBuffs);
                    if (graph != null)
                    {
                        list.Add(graph);
                    }
                }
                boonGraphData.Remove(buff.ID);
            }
        }

        public static List<BuffChartDataDto> BuildBoonGraphData(ParsedEvtcLog log, AbstractSingleActor p, PhaseData phase, Dictionary<long, Buff> usedBuffs)
        {
            var list = new List<BuffChartDataDto>();
            var boonGraphData = p.GetBuffGraphs(log).ToDictionary(x => x.Key, x => x.Value);
            BuildBoonGraphData(list, log.StatisticsHelper.PresentBoons, boonGraphData, phase, usedBuffs);
            BuildBoonGraphData(list, log.StatisticsHelper.PresentConditions, boonGraphData, phase, usedBuffs);
            BuildBoonGraphData(list, log.StatisticsHelper.PresentOffbuffs, boonGraphData, phase, usedBuffs);
            BuildBoonGraphData(list, log.StatisticsHelper.PresentSupbuffs, boonGraphData, phase, usedBuffs);
            BuildBoonGraphData(list, log.StatisticsHelper.PresentDefbuffs, boonGraphData, phase, usedBuffs);
            BuildBoonGraphData(list, log.StatisticsHelper.PresentDebuffs, boonGraphData, phase, usedBuffs);
            BuildBoonGraphData(list, log.StatisticsHelper.PresentGearbuffs, boonGraphData, phase, usedBuffs);
            foreach (BuffsGraphModel bgm in boonGraphData.Values)
            {
                BuffChartDataDto graph = BuildBuffGraph(bgm, phase, usedBuffs);
                if (graph != null)
                {
                    list.Add(graph);
                }
            }
            if (p.GetType() == typeof(Player))
            {
                foreach (AbstractSingleActor mainTarget in log.FightData.GetMainTargets(log))
                {
                    boonGraphData = mainTarget.GetBuffGraphs(log);
                    foreach (BuffsGraphModel bgm in boonGraphData.Values.Reverse().Where(x => x.Buff.Name == "Compromised" || x.Buff.Name == "Unnatural Signet" || x.Buff.Name == "Fractured - Enemy" || x.Buff.Name == "Erratic Energy"))
                    {
                        BuffChartDataDto graph = BuildBuffGraph(bgm, phase, usedBuffs);
                        if (graph != null)
                        {
                            list.Add(graph);
                        }
                    }
                }
            }
            list.Reverse();
            return list;
        }
    }
}
