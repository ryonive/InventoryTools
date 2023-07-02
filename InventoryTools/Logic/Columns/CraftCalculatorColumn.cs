using System;
using System.Collections.Generic;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class CraftCalculatorColumn : IntegerColumn, IDisposable
{
    public override ColumnCategory ColumnCategory => ColumnCategory.Tools;
    public Dictionary<uint, uint>? _craftable;
    public CraftCalculator? _craftCalculator;
    public override int? CurrentValue(InventoryItem item)
    {
        return CurrentValue(item.Item);
    }

    public override int? CurrentValue(ItemEx item)
    {
        if (_craftable == null) return 0;
        return (int?)(_craftable.ContainsKey(item.RowId) ? _craftable[item.RowId] : 0);
    }

    public override int? CurrentValue(SortingResult item)
    {
        return CurrentValue(item.InventoryItem);
    }

    public override string Name { get; set; } = "Craft Calculator";
    public override float Width { get; set; } = 80;

    public override string HelpText { get; set; } =
        "This will calculate the total amount of an item that could be crafted based on the items within your character and retainers.";

    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    public override FilterType AvailableIn { get; } = Logic.FilterType.GameItemFilter;

    public override IFilterEvent? DrawFooterFilter(FilterConfiguration configuration, FilterTable filterTable)
    {
        ImGui.SameLine();
        
        if (_craftCalculator == null || !_craftCalculator.IsRunning)
        {
            if (ImGui.Button("Calculate Crafts"))
            {
                if (_craftCalculator == null)
                {
                    _craftCalculator = new CraftCalculator();
                    _craftCalculator.CraftingResult += CraftCalculatorOnCraftingResult;
                }

                var items = new List<CriticalCommonLib.Models.InventoryItem>();
                var playerBags = PluginService.InventoryMonitor.GetSpecificInventory(PluginService.CharacterMonitor.ActiveCharacterId,
                    InventoryCategory.CharacterBags);
                var crystalBags = PluginService.InventoryMonitor.GetSpecificInventory(PluginService.CharacterMonitor.ActiveCharacterId,
                    InventoryCategory.Crystals);
                var currencyBags = PluginService.InventoryMonitor.GetSpecificInventory(PluginService.CharacterMonitor.ActiveCharacterId,
                    InventoryCategory.Currency);
                var inventories = PluginService.InventoryMonitor.Inventories;
                foreach (var characterId in inventories)
                {
                    var character = PluginService.CharacterMonitor.GetCharacterById(characterId.Key);
                    if (character != null)
                    {
                        if (character.OwnerId == PluginService.CharacterMonitor.ActiveCharacterId)
                        {
                            foreach (var inventoryCategory in characterId.Value.GetAllInventories())
                            {
                                foreach (var inventory in inventoryCategory)
                                {
                                    if (inventory != null)
                                    {
                                        items.Add(inventory);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var item in playerBags)
                {
                    items.Add(item);
                }

                foreach (var item in crystalBags)
                {
                    items.Add(item);
                }

                foreach (var item in currencyBags)
                {
                    items.Add(item);
                }
                _craftCalculator.SetAvailableItems(items);                
                foreach (var item in filterTable.RenderItems)
                {
                    _craftCalculator.AddItemId(item.RowId);
                }
                _craftCalculator.StartProcessing();
            }
        }
        else if (_craftCalculator.IsRunning)
        {
            if (ImGui.Button("Stop Calculating Crafts"))
            {
                _craftCalculator.CancelProcessing();
            }
        }


        return null;
    }

    private void CraftCalculatorOnCraftingResult(object? sender, CraftingResultEventArgs e)
    {
        if (_craftable == null)
        {
            _craftable = new Dictionary<uint, uint>();
        }
        _craftable[e.ItemId] = e.CraftableQuantity ?? 0;
    }

    public override void Dispose()
    {
        if (!base.Disposed)
        {
            if (_craftCalculator != null)
            {
                _craftCalculator.CraftingResult -= CraftCalculatorOnCraftingResult;
                _craftCalculator.Dispose();
            }            
            base.Dispose();
        }
    }
}