using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters.Abstract
{
    using System.Collections.Generic;

    public abstract class StringFilter : Filter<string>
    {

        public StringFilter(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override string DefaultValue { get; set; } = "";

        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return CurrentValue(configuration) != "";
        }
        
        public override string CurrentValue(FilterConfiguration configuration)
        {
            return (configuration.GetStringFilter(Key) ?? "").Trim();
        }

        public override void Draw(FilterConfiguration configuration)
        {
            var value = CurrentValue(configuration) ?? "";
            ImGui.SetNextItemWidth(LabelSize);
            if (HasValueSet(configuration))
            {
                ImGui.PushStyleColor(ImGuiCol.Text,ImGuiColors.HealerGreen);
                ImGui.LabelText("##" + Key + "Label", Name + ":");
                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.LabelText("##" + Key + "Label", Name + ":");
            }
            ImGui.SameLine();
            ImGui.SetNextItemWidth(InputSize);
            if (ImGui.InputText("##"+Key+"Input", ref value, 500))
            {
                UpdateFilterConfiguration(configuration, value);
            }
            ImGui.SameLine();
            if (this.ShowOperatorTooltip)
            {
                ImGuiService.HelpMarker(new List<string>()
                {
                    HelpText,
                    "When searching the following operators can be used to compare: ",
                    "",
                    ">, >=, <, <=, =, for numerical comparisons" ,
                    "=, for exact comparisons",
                    "!, for inequality comparisons",
                    "||, search multiple expressions using OR",
                    "&&, search multiple expressions using AND"
                });
            }
            else
            {
                ImGuiService.HelpMarker(HelpText);
            }

            if (HasValueSet(configuration) && ShowReset)
            {
                ImGui.SameLine();
                if (ImGui.Button("Reset##" + Key + "Reset"))
                {
                    ResetFilter(configuration);
                }
            }
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, string newValue)
        {
            configuration.UpdateStringFilter(Key, newValue);
        }
        
        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, DefaultValue);
        }
    }
}