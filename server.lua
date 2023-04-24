-- The activation of passive mode for specific players.
Activations = {}
-- The activations that override the dictionary above.
Overrides = {}

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
    local activation = not (not activation)

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
    local override = not (not override)

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

function OnPlayerInitialized()
    local player = GetPlayer(source)

    if player == nil then
        return false
    end

    Activations[player] = GetDefaultActivation()

    for _, otherPlayerSrc in ipairs(GetActivePlayers()) do
        local otherPlayer = GetPlayer(otherPlayerSrc)

        if otherPlayer ~= nil then
            TriggerClientEvent("simplepassive:activationChanged", player, otherPlayer, GetPlayerActivation(otherPlayer));
        end
    end

    Debug("Player " .. GetPlayerName(player) .. " (" .. player .. ") received all passive activations")
end

function SetPassiveSelf(activation)
    local player = GetPlayer(source)
    local activation = not (not activation)

    if player == nil then
        return false
    end

    if not IsPlayerAceAllowed(player, "simplepassive.changeself") then
        return
    end

    if Overrides[player] ~= nil then
        return
    end

    Activations[player] = activation
    TriggerClientEvent("simplepassive:activationChanged", -1, player, activation)

    Debug("Player " .. GetPlayerName(player) .. " (" .. player .. ") set it's own activation to " .. activation)
end

function OnOverrideCommand(source, args, raw)
    if #args < 2 then
        print("You need to specify the Player ID and desired Activation!")
        return
    end

    local player = GetPlayer(args[1])

    if player == nil then
        print("The Player specified it's not valid!")
        return
    end

    local activation = tonumber(args[2])

    if activation == nil then
        print("The activation needs to be 0 or 1!")
        return
    end

    SetPlayerOverride(player, activation)
end

function OnClearCommand(source, args, raw)
    if args[1] == nil then
        Debug("You need to specify the ID of a Player!")
        return
    end

    local player = GetPlayer(args[1])

    if player == nil then
        Debug("The Player specified is not valid.");
        return
    end

    if ClearPlayerOverride(player) then
        print("The Override of " .. GetPlayerName(player) .. " (" .. player .. ") was cleared!")
    else
        print("Player " .. GetPlayerName(player) .. " (" .. player .. ") does not has an override set")
    end
end

function OnOverridesCommand(source, args, raw)
    if #Overrides == 0 then
        print("There are no Passive Mode Overrides in place.")
        return
    end

    for player, activation in pairs(Activations) do
        print("\t" .. player .. " set to " .. activation)
    end
end

function OnToggleCommand(source, args, raw)
    local player = GetPlayer(source)

    if player == nil then
        print("This command can only be used by players on the server")
        return
    end

    if Overrides[player] ~= nil then
        print("Your Passive Mode Activation has been overriden, you can't change it")
        return
    end

    if not IsPlayerAceAllowed(source, "simplepassive.changeself") then
        print("You are not allowed to change your passive mode activation")
        return
    end

    local opposite = not GetPlayerActivation(player)
    Activations[player] = opposite
    TriggerClientEvent("simplepassive:activationChanged", -1, player, opposite)
    print("Player " .. GetPlayerName(player) .. " (" .. player .. ") set it's activation to " .. opposite)
end

exports("getActivation", GetPlayerActivation)
exports("setActivation", SetPlayerActivation)
exports("isOverriden", IsPlayerOverridden)
exports("setOverride", SetPlayerOverride)
exports("clearOverride", ClearPlayerOverride)
AddEventHandler("playerDropped", OnPlayerDropped)
AddEventHandler("simplepassive:initialized", OnPlayerInitialized)
AddEventHandler("simplepassive:setPassive", SetPassiveSelf)
RegisterCommand("passiveoverride", OnOverrideCommand, true)
RegisterCommand("passiveclear", OnClearCommand, true)
RegisterCommand("passiveoverrides", OnOverridesCommand, true)
RegisterCommand("passivetoggle", OnToggleCommand, false)
