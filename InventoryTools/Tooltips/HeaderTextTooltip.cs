using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Tooltips;

public class HeaderTextTooltip : BaseTooltip
{
    public HeaderTextTooltip(ILogger<HeaderTextTooltip> logger, ItemSheet itemSheet, InventoryToolsConfiguration configuration, IGameGui gameGui) : base(logger, itemSheet, configuration, gameGui)
    {
    }
    public override bool IsEnabled =>
        Configuration.DisplayTooltip && (Configuration.TooltipHeaderLines != 0 ||
                                         Configuration.TooltipDisplayHeader);

    public override unsafe void OnGenerateItemTooltip(NumberArrayData* numberArrayData, StringArrayData* stringArrayData)
    {
        if (!ShouldShow()) return;

        var item = HoverItem;
        if (item != null) {
            var textLines = new List<string>();
            
            TooltipService.ItemTooltipField itemTooltipField;
            var tooltipVisibility = GetTooltipVisibility((int**)numberArrayData);
            if (tooltipVisibility.HasFlag(ItemTooltipFieldVisibility.Description))
            {
                itemTooltipField = TooltipService.ItemTooltipField.ItemDescription;
            }
            else if (tooltipVisibility.HasFlag(ItemTooltipFieldVisibility.Effects))
            {
                itemTooltipField = TooltipService.ItemTooltipField.Effects;
            }
            else if (tooltipVisibility.HasFlag(ItemTooltipFieldVisibility.Levels))
            {
                itemTooltipField = TooltipService.ItemTooltipField.Levels;
            }
            else
            {
                return;
            }
            
            var seStr = GetTooltipString(stringArrayData, itemTooltipField);

            if (seStr != null && seStr.Payloads.Count > 0)
            {
                var newText = "";
                if (Configuration.TooltipHeaderLines != 0)
                {
                    for (int i = 0; i < Configuration.TooltipHeaderLines; i++)
                    {
                        newText += "\n";
                    }
                }
                if (Configuration.TooltipDisplayHeader)
                {
                    newText += "\n[Allagan Tools]";
                }

                if (newText != "")
                {
                    var lines = new List<Payload>()
                    {
                        new UIForegroundPayload((ushort)(Configuration.TooltipColor ?? 1)),
                        new UIGlowPayload(0),
                        new TextPayload(newText),
                        new UIGlowPayload(0),
                        new UIForegroundPayload(0),
                    };
                    foreach (var line in lines)
                    {
                        seStr.Payloads.Add(line);
                    }

                    SetTooltipString(stringArrayData, itemTooltipField, seStr);
                }
            }
        }
    }

    public override uint Order => 0;
}