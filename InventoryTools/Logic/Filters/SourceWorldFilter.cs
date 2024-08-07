using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class SourceWorldFilter : MultipleChoiceFilter<uint>
    {
        private readonly ExcelCache _excelCache;

        public override List<uint> CurrentValue(FilterConfiguration configuration)
        {
            return configuration.SourceWorlds?.ToList() ?? new List<uint>();
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, List<uint> newValue)
        {
            configuration.SourceWorlds = newValue.Count == 0 ? null : newValue.Distinct().ToHashSet();
        }
        
        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, new List<uint>());
        }
        
        public override int LabelSize { get; set; } = 240;
        public override string Key { get; set; } = "SourceWorlds";
        public override string Name { get; set; } = "Source - Worlds";
        public override string HelpText { get; set; } =
            "This is a list of sources worlds to search in. It will attempt to search for items in any bag of any character/retainer on that world.";
        
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;
        public override List<uint> DefaultValue { get; set; } = new();
        public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.SortingFilter | FilterType.CraftFilter | FilterType.HistoryFilter;
        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            return null;
        }


        private Dictionary<uint, string>? _choices;
        public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
        {
            if (_choices == null)
            {
                _choices = _excelCache.GetWorldSheet().Where(c => c.IsPublic).OrderBy(c =>c.FormattedName).ToDictionary(c => c.RowId, c => c.FormattedName);
            }

            return _choices;
        }

        public override bool HideAlreadyPicked { get; set; } = true;

        public SourceWorldFilter(ILogger<SourceWorldFilter> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
        {
            _excelCache = excelCache;
        }
    }
}