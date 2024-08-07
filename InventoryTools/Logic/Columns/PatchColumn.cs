using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class PatchColumn : DecimalColumn
{
    private readonly ExcelCache _excelCache;

    public PatchColumn(ILogger<PatchColumn> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
    {
        _excelCache = excelCache;
    }
    public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
    public override decimal? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        return _excelCache.GetItemPatch(item.ItemId);
    }

    public override decimal? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        return _excelCache.GetItemPatch(item.RowId);
    }

    public override decimal? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return _excelCache.GetItemPatch(item.InventoryItem.ItemId);
    }

    public override string Name { get; set; } = "Patch Added";
    public override string RenderName => "Patch";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "Shows the patch in which the item was added.";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}