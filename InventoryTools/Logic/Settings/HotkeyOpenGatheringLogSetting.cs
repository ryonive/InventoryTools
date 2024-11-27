using Dalamud.Game.ClientState.Keys;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Classes;

namespace InventoryTools.Logic.Settings
{
    public class HotkeyOpenGatheringLogSetting : HotKeySetting
    {
        public HotkeyOpenGatheringLogSetting(ILogger<HotkeyOpenGatheringLogSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }

        public override ModifiableHotkey DefaultValue { get; set; } = new(VirtualKey.NO_KEY);

        public override ModifiableHotkey CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.OpenGatheringLogHotKey ?? new ModifiableHotkey();
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, ModifiableHotkey newValue)
        {
            configuration.OpenGatheringLogHotKey = newValue;
        }

        public override string Key { get; set; } = "OpenGatheringLogHotKey";
        public override string Name { get; set; } = "Open Gathering Log Hotkey";

        public override string HelpText { get; set; } =
            "The hotkey to open the gathering log for an item when hovering it.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Hotkeys;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
        public override string Version => "1.11.0.2";
    }
}