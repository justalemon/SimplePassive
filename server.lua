-- The activation of passive mode for specific players.
Activations = {}
-- The activations that override the dictionary above.
Overrides = {}

function Debug(message)
    if GetConvarInt("simplepassive_debug", 0) == 0 then
        return
    end

    print(message)
end

function GetDefaultActivation()
    return GetConvarInt("simplepassive_default", 0) ~= 0
end

function GetPlayer(playerSrc)
    -- do this as it might be passed as an int from C#
    local handle = tonumber(playerSrc)
    -- Best option I could find
    -- Returns nil when the playerSrc is not valid, and a number when valid
    local guid = GetPlayerGuid(handle)

    if guid then
        return handle
    else
        return nil
    end
end

function GetPlayerActivation(playerSrc)
    local player = GetPlayer(playerSrc)

    if player == nil then
        return false
    end

    local override = Overrides[player]
    if override ~= nil then
        return override
    end

    local activation = Activations[player]
    if activation ~= nil then
        return activation
    end

    return GetDefaultActivation()
end

function SetPlayerActivation(playerSrc, activation)
    local player = GetPlayer(playerSrc)

    if player == nil then
        return false
    end

    Activations[player] = activation
    TriggerClientEvent("simplepassive:activationChanged", -1, player, activation)
    Debug("Passive Activation of " .. GetPlayerName(player) .. " (" .. player .. ") is now " .. activation)
    return true
end

function IsPlayerOverridden(playerSrc)
    local player = GetPlayer(playerSrc)
    return player ~= nil and Overrides[player] ~= nil
end

function SetPlayerOverride(playerSrc, override)
    local player = GetPlayer(playerSrc)

    if player == nil then
        return false
    end

    Overrides[player] = override
    TriggerClientEvent("simplepassive:activationChanged", -1, tonumber(player), override)
    Debug("Passive Activation of " .. GetPlayerName(player) .. " (" .. player .. ") is overriden to " .. override)
    return true
end

function ClearPlayerOverride(playerSrc)
    local player = GetPlayer(playerSrc)

    if player == nil then
        return false
    end

    if Overrides[player] == nil then
        return false
    end

    Overrides[player] = nil
    Debug("Passive Override of " .. GetPlayerName(player) .. " (" .. player .. ") was removed")
    return true
end

function OnPlayerDropped()
    local player = tonumber(source)

    TriggerClientEvent("simplepassive:doCleanup", -1, player)
    Overrides[player] = nil
    Activations[player] = nil
end

exports("getActivation", GetPlayerActivation)
exports("setActivation", SetPlayerActivation)
exports("isOverriden", IsPlayerOverridden)
exports("setOverride", SetPlayerOverride)
exports("clearOverride", ClearPlayerOverride)
AddEventHandler("playerDropped", OnPlayerDropped)
