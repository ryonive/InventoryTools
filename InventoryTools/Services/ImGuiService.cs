using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CriticalCommonLib;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Images;
using InventoryTools.Services.Interfaces;
using OtterGui.Classes;

namespace InventoryTools.Services;

public class ImGuiService
{
    public ITextureProvider TextureProvider { get; }
    public IIconService IconService { get; }
    public RightClickService RightClickService { get; }
    public IPluginInterfaceService PluginInterfaceService { get; }

    public ImGuiService(ITextureProvider textureProvider, IIconService iconService, RightClickService rightClickService, IPluginInterfaceService pluginInterfaceService)
    {
        TextureProvider = textureProvider;
        IconService = iconService;
        RightClickService = rightClickService;
        PluginInterfaceService = pluginInterfaceService;
    }
    

    public readonly GameIcon TickIcon = new GameIcon()
        {Name = "readycheck", Size = new Vector2(32, 32), Uv0 = new Vector2(0f, 0f), Uv1 = new Vector2(0.5f, 1f)};

    public readonly GameIcon CrossIcon = new GameIcon()
        {Name = "readycheck", Size = new Vector2(32, 32), Uv0 = new Vector2(0.5f, 0f), Uv1 = new Vector2(1f, 1f)};

    public readonly GameIcon CheckboxChecked = new GameIcon()
    {
        Name = "CheckBoxA_hr1", Size = new Vector2(16, 16), Uv0 = new Vector2(0.5f, 0f), Uv1 = new Vector2(1f, 1f)
    };

    public readonly GameIcon CheckboxUnChecked = new GameIcon()
    {
        Name = "CheckBoxA_hr1", Size = new Vector2(16, 16), Uv0 = new Vector2(0f, 0f), Uv1 = new Vector2(0.5f, 1f)
    };

    public void DrawUldIcon(GameIcon gameIcon,  Vector2? size = null)
    {
        DrawUldIcon(gameIcon.Name, size ?? gameIcon.Size, gameIcon.Uv0, gameIcon.Uv1);
    }

    public void DrawUldIcon(string name, Vector2 size, Vector2? uvStart = null, Vector2? uvEnd = null)
    {
        var iconTex = TextureProvider.GetUldIcon(name);
        if (iconTex == null) return;
        if (iconTex.ImGuiHandle == IntPtr.Zero) return;
        if (uvStart.HasValue && uvEnd.HasValue)
        {
            ImGui.Image(iconTex.ImGuiHandle, size, uvStart.Value,
                uvEnd.Value);
        }
        else if (uvStart.HasValue)
        {
            ImGui.Image(iconTex.ImGuiHandle, size, uvStart.Value);
        }
        else
        {
            ImGui.Image(iconTex.ImGuiHandle, size);
        }
    }

    public void DrawIcon(ushort icon, Vector2 size, bool hqIcon = false)
    {
        if (icon <= 65103)
        {
            var iconTex = hqIcon ?  TextureProvider.GetIcon(icon, ITextureProvider.IconFlags.ItemHighQuality) : TextureProvider.GetIcon(icon);
            if (iconTex != null)
            {
                if (iconTex.ImGuiHandle != IntPtr.Zero)
                {
                    ImGui.Image(iconTex.ImGuiHandle, size);
                }
            }
        }
        else
        {
            ImGui.Text("Invalid Icon ID");
        }
    }

    public bool DrawUldIconButton(GameIcon gameIcon, Vector2? size = null)
    {
        return DrawUldIconButton(gameIcon.Name, size ?? gameIcon.Size, gameIcon.Uv0, gameIcon.Uv1);
    }

    public bool DrawUldIconButton(string name, Vector2 size, Vector2? uvStart = null, Vector2? uvEnd = null)
    {
        var iconTex = TextureProvider.GetUldIcon(name);
        if (iconTex != null)
        {
            if (iconTex.ImGuiHandle != IntPtr.Zero)
            {
                if (uvStart.HasValue && uvEnd.HasValue)
                {
                    return ImGui.ImageButton(iconTex.ImGuiHandle, size, uvStart.Value,
                        uvEnd.Value);
                }
                else if (uvStart.HasValue)
                {
                    return ImGui.ImageButton(iconTex.ImGuiHandle, size, uvStart.Value);
                }
                else
                {
                    return ImGui.ImageButton(iconTex.ImGuiHandle, size);
                }
            }
        }

        return false;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void HelpMarker(string helpText, string? imagePath = null, System.Numerics.Vector2? imageSize = null)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(helpText);
            ImGui.PopTextWrapPos();
            if (imagePath != null)
            {
                var sourceIcon = LoadImage(imagePath);
                if (sourceIcon != null)
                {
                    ImGui.Image(sourceIcon.ImGuiHandle, imageSize ??
                                                        new Vector2(200, 200) * ImGui.GetIO().FontGlobalScale);
                }
            }
            ImGui.EndTooltip();
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void VerticalCenter(string text)
    {
        var offset = (ImGui.GetWindowSize().Y - ImGui.CalcTextSize(text).Y) / 2.0f;
        ImGui.SetCursorPosY(offset);
        ImGui.TextUnformatted(text);
    }

    public void CenterElement(float height)
    {
        ImGui.SetCursorPosY((ImGui.GetWindowSize().Y - height) / 2.0f + (ImGui.GetStyle().FramePadding.Y / 2.0f));
    }
    
    public IDalamudTextureWrap? LoadImage(string imageName)
    {
        var assemblyLocation = PluginInterfaceService.AssemblyLocation.DirectoryName!;
        var imagePath = Path.Combine(assemblyLocation, $@"Images\{imageName}.png");
        return TextureProvider.GetTextureFromFile(new FileInfo(imagePath));
    }

    public void WrapTableColumnElements<T>(string windowId, IEnumerable<T> items, float rowSize, Func<T, bool> drawElement)
    {
        using (var wrapTableChild = ImRaii.Child(windowId, new Vector2(ImGui.GetContentRegionAvail().X, rowSize + ImGui.GetStyle().CellPadding.Y + ImGui.GetStyle().ItemSpacing.Y), false))
        {
            if (wrapTableChild.Success)
            {
                var columnWidth = ImGui.GetContentRegionAvail().X * ImGui.GetIO().FontGlobalScale;
                var itemWidth = (rowSize + ImGui.GetStyle().ItemSpacing.X) * ImGui.GetIO().FontGlobalScale;
                var maxItems = itemWidth != 0 ? (int)Math.Floor(columnWidth / itemWidth) : 0;
                maxItems = maxItems == 0 ? 1 : maxItems;
                var enumerable = items.ToList();
                var count = 1;
                for (var index = 0; index < enumerable.Count; index++)
                {
                    using (ImRaii.PushId(index))
                    {
                        if (drawElement.Invoke(enumerable[index]))
                        {

                            if (count % maxItems != 0)
                            {
                                ImGui.SameLine();
                            }

                            count++;
                        }
                    }
                }
            }
        }
    }
}