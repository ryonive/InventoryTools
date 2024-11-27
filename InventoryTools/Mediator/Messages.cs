using System;
using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;

using FFXIVClientStructs.FFXIV.Client.Game;
using InventoryTools.Logic;

namespace InventoryTools.Mediator;

public record ToggleGenericWindowMessage(Type windowType) : MessageBase;
public record ToggleUintWindowMessage(Type windowType, uint windowId) : MessageBase;
public record ToggleStringWindowMessage(Type windowType, string windowId) : MessageBase;
public record OpenGenericWindowMessage(Type windowType) : MessageBase;
public record OpenUintWindowMessage(Type windowType, uint windowId) : MessageBase;
public record OpenStringWindowMessage(Type windowType, string windowId) : MessageBase;
public record CloseWindowMessage(Type windowType) : MessageBase;
public record CloseUintWindowMessage(Type windowType, uint windowId) : MessageBase;
public record CloseStringWindowMessage(Type windowType, string windowId) : MessageBase;
public record CloseWindowsByTypeMessage(Type windowType) : MessageBase;
public record CloseWindowsMessage(Type windowType) : MessageBase;
public record OpenSavedWindowsMessage() : MessageBase;
public record UpdateWindowRespectClose(Type windowType, bool newSetting) : MessageBase;
public record ConfigurationWindowEditFilter(FilterConfiguration filter) : MessageBase;
public record ListInvalidatedMessage(FilterConfiguration filterConfiguration) : MessageBase;
public record ListModifiedMessage(FilterConfiguration filterConfiguration) : MessageBase;
public record ListRepositionedMessage(FilterConfiguration filterConfiguration) : MessageBase;
public record ListAddedMessage(FilterConfiguration filterConfiguration) : MessageBase;
public record ListRemovedMessage(FilterConfiguration filterConfiguration) : MessageBase;
public record ListUpdatedMessage(FilterConfiguration FilterConfiguration) : MessageBase;
public record RequestListUpdateMessage(FilterConfiguration FilterConfiguration) : MessageBase;
public record TeamCraftDataImported(List<(uint, uint)> listData) : MessageBase;
public record AddToCraftListMessage(string FilterKey, uint ItemId, uint Quantity, InventoryItem.ItemFlags Flags) : MessageBase;
public record AddToNewCraftListMessage(uint ItemId, uint Quantity, InventoryItem.ItemFlags Flags, bool IsEphemeral) : MessageBase;
public record FocusListMessage(Type windowType, FilterConfiguration FilterConfiguration) : MessageBase;
public record RequestTeleportMessage(uint aetheryteId) : MessageBase;
public record OverlaysRequestRefreshMessage() : MessageBase;
public record ItemSearchRequestedMessage(uint ItemId, InventoryItem.ItemFlags Flags) : MessageBase;
