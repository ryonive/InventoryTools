using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;

using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Widgets;

namespace InventoryTools.Logic.Columns
{
    public class BestInSlotColumn : IntegerColumn
    {
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IInventoryMonitor _inventoryMonitor;
        private readonly ClassJobCategorySheet _classJobCategorySheet;

        public BestInSlotColumn(ILogger<BestInSlotColumn> logger, ImGuiService imGuiService, ICharacterMonitor characterMonitor, IInventoryMonitor inventoryMonitor, ClassJobCategorySheet classJobCategorySheet) : base(logger, imGuiService)
        {
            _characterMonitor = characterMonitor;
            _inventoryMonitor = inventoryMonitor;
            _classJobCategorySheet = classJobCategorySheet;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Tools;

        private ClippedSelectableCombo<KeyValuePair<ulong, Character>>? _characters;
        private ulong? _selectedCharacter;

        public override IFilterEvent? DrawFooterFilter(ColumnConfiguration columnConfiguration, FilterTable table)
        {
            ImGui.SameLine();
            var characterDictionary = _characterMonitor.Characters;
            var currentCharacterId = _characterMonitor.ActiveCharacterId;
            var allCharacters = characterDictionary.Where(c => c.Value.Name != "" && (c.Value.OwnerId == currentCharacterId || c.Key == currentCharacterId)).ToList();
            var currentCharacterName = _selectedCharacter == null
                ? ""
                : characterDictionary.ContainsKey(_selectedCharacter.Value)
                    ? characterDictionary[_selectedCharacter.Value].Name
                    : "";
            _characters = new ClippedSelectableCombo<KeyValuePair<ulong, Character>>("BestInSlotCharacterSelect", "BiS Character", 200,allCharacters, character => character.Value.NameWithClassAbv);
            if (_characters.Draw(currentCharacterName, out var selected))
            {
                _selectedCharacter = allCharacters[selected].Key;
                return new RefreshFilterEvent();
            }

            return null;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            var inventoryItem = searchResult.InventoryItem;
            if (inventoryItem != null)
            {
                if (inventoryItem.SortedCategory == InventoryCategory.CharacterEquipped ||
                    inventoryItem.SortedCategory == InventoryCategory.RetainerEquipped ||
                    inventoryItem.SortedCategory == InventoryCategory.Armoire ||
                    inventoryItem.SortedCategory == InventoryCategory.GlamourChest || inventoryItem.InGearSet)
                {
                    return null;
                }
            }

            var item = searchResult.Item;
            if (item.EquipSlotCategory != null && CanCurrentJobEquip(item.Base.ClassJobCategory.RowId) && CanUse(item.Base.LevelEquip))
            {
                var equippedItem = GetEquippedItem(item);
                if (equippedItem != null)
                {
                    if (item.EquipSlotCategory?.SimilarSlots(equippedItem.Item) ?? false)
                    {
                        return (int)item.Base.LevelItem.RowId - (int)equippedItem.Item.Base.LevelItem.RowId;
                    }
                }

                return (int)item.Base.LevelItem.RowId;
            }
            return null;
        }

        public override string Name { get; set; } = "Relative Item Level";
        public override float Width { get; set; } = 80;

        public override string HelpText { get; set; } =
            "Shows the relative item level of either your items or your retainers items compared to the item shown. This will show a drop down in the filter that lets you pick which character you are comparing against. A negative value indicates it's worse, a positive indicates it's better.";

        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

        public bool CanUse(uint itemLevel)
        {
            if (_selectedCharacter != null)
            {
                if (_characterMonitor.Characters.ContainsKey(_selectedCharacter.Value))
                {
                    var character = _characterMonitor.Characters[_selectedCharacter.Value];
                    if (character.OwnerId != 0)
                    {
                        return character.Level >= itemLevel;
                    }
                }
            }
            if (_characterMonitor.ActiveCharacterId != 0)
            {
                var activeCharacter = _characterMonitor.ActiveCharacter;
                if (activeCharacter != null)
                {
                    return activeCharacter.Level >= itemLevel;
                }
            }

            return false;
        }

        public bool CanCurrentJobEquip(uint classJobCategory)
        {
            if (_selectedCharacter != null)
            {
                if (_characterMonitor.Characters.ContainsKey(_selectedCharacter.Value))
                {
                    var character = _characterMonitor.Characters[_selectedCharacter.Value];
                    if (character.OwnerId != 0)
                    {
                        if(_classJobCategorySheet.IsItemEquippableBy(classJobCategory, character.ClassJob))
                        {
                            return true;
                        }

                        return false;
                    }
                }
            }
            if (_characterMonitor.ActiveCharacterId != 0)
            {
                var activeCharacter = _characterMonitor.ActiveCharacter;
                if (activeCharacter != null)
                {
                    if(_classJobCategorySheet.IsItemEquippableBy(classJobCategory, activeCharacter.ClassJob))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public InventoryItem? GetEquippedItem(ItemRow comparingItem)
        {
            if (_selectedCharacter != null)
            {
                if (_characterMonitor.Characters.ContainsKey(_selectedCharacter.Value))
                {
                    var character = _characterMonitor.Characters[_selectedCharacter.Value];
                    if (character.OwnerId != 0)
                    {
                        var equipped = _inventoryMonitor.GetSpecificInventory(character.CharacterId,InventoryCategory.RetainerEquipped);
                        return equipped.FirstOrDefault(c => c.Item.EquipSlotCategory?.SimilarSlots(comparingItem) ?? false);
                    }
                }
            }
            if (_characterMonitor.ActiveCharacterId != 0)
            {
                var equipped = _inventoryMonitor.GetSpecificInventory(_characterMonitor.ActiveCharacterId,
                    InventoryCategory.CharacterEquipped);
                return equipped.FirstOrDefault(c => c.Item.EquipSlotCategory?.SimilarSlots(comparingItem) ?? false);
            }

            return null;
        }
    }
}