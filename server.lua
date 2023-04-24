-- The activation of passive mode for specific players.
Activations = {}
-- The activations that override the dictionary above.
Overrides = {}

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
