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

function GetPlayerActivation(playerId)
    local override = Overrides[playerId]
    if override ~= nil then
        return override
    end

    local activation = Activations[playerId]
    if activation ~= nil then
        return activation
    end

    return GetDefaultActivation()
end

function SetPlayerActivation(playerSrc, activation)
    -- do this as it might be passed as an int from C#
    playerSrc = tostring(playerSrc)

    -- Best option I could find
    -- Returns nil when the playerSrc is not valid, and a number when valid
    if GetPlayerGuid(playerSrc) == nil then
        return false
    end

    Activations[playerSrc] = activation
    TriggerClientEvent("simplepassive:activationChanged", -1, tonumber(playerSrc), activation)
    Debug("Passive Activation of " .. GetPlayerName(playerSrc) .. " (" .. playerSrc .. ") is now " .. activation)
    return true
end

function IsPlayerOverridden(playerSrc)
    -- do this as it might be passed as an int from C#
    playerSrc = tostring(playerSrc)
    return Overrides[playerSrc] ~= nil
end

function SetPlayerOverride(playerSrc, override)
    -- do this as it might be passed as an int from C#
    playerSrc = tostring(playerSrc)

    -- Best option I could find
    -- Returns nil when the playerSrc is not valid, and a number when valid
    if GetPlayerGuid(playerSrc) == nil then
        return false
    end

    Overrides[playerSrc] = override
    TriggerClientEvent("simplepassive:activationChanged", -1, tonumber(playerSrc), override)
    Debug("Passive Activation of " .. GetPlayerName(playerSrc) .. " (" .. playerSrc .. ") is overriden to " .. override)
    return true
end

exports("getActivation", GetPlayerActivation)
exports("setActivation", SetPlayerActivation)
exports("isOverriden", IsPlayerOverridden)
exports("setOverride", SetPlayerOverride)
