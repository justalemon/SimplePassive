-- The activations sent by the server
Activations = {}
-- Whether the next set of collision changes should be printed on the console
PrintCollisionChanges = false
-- The last known player vehicle
LastVehicle = nil
-- The last known trailer/hooked vehicle
LastHooked = nil

function GetHookedVehicle(vehicle)
    if not DoesEntityExist(vehicle) and not IsEntityAVehicle(vehicle) then
        return
    end

    local hasTrailer, trailer = GetVehicleTrailerVehicle(vehicle)
    if hasTrailer then
        return trailer
    end

    local hasHooked, hooked = GetVehicleAttachedToCargobob(vehicle)
    if hasHooked then
        return hooked
    end

    local hasTowed, towed = GetEntityAttachedToTowTruck(vehicle)
    if hasTowed then
        return towed
    end

    return 0
end

function DrawDebugMarker(entity)
    if not DoesEntityExist(entity) or not IsEntityOnScreen(entity) then
        return
    end

    local pos = GetEntityCoords(entity, false)
    local _, x, y = GetScreenCoordFromWorldCoord(pos.x, pos.y, pos.z)
    BeginTextCommandDisplayText("CELL_EMAIL_BCON")
    AddTextComponentSubstringPlayerName(tostring(entity))
    SetTextColour(255, 66, 198, 255)
    SetTextJustification(0)
    EndTextCommandDisplayText(x, y)
end

function DisableCollisionsThisFrame(one, two)
    SetEntityNoCollisionEntity(one, two, true)
    SetEntityNoCollisionEntity(two, one, true)

    if PrintCollisionChanges then
        print("Disabled collisions between " .. one .. " and " .. two .. " .");
    end
end

function SetAlpha(entity, alpha)
    if alpha >= 255 then
        ResetEntityAlpha(entity)
    else
        SetEntityAlpha(entity, alpha, 0)
    end
end

function GetLocalPlayerActivation()
    local playerId = PlayerId()
    local player = GetPlayerServerId(playerId)
    return GetPlayerActivation(player)
end

function GetPlayerActivation(player)
    local activation = Activations[player]

    if activation ~= nil then
        return activation
    else
        return GetDefaultActivation()
    end
end

function SetLocalPlayerActivation(activation)
    -- TODO: Add proper boolean checks
    Debug("Requesting server to change the activation to " .. activation)
    TriggerServerEvent("simplepassive:setPassive", activation)
end

function Initialize()
    TriggerServerEvent("simplepassive:initialized")
end

function HandleCollisions()
    while true do
        local localPlayer = PlayerId()
        local localPed = PlayerPedId()
        local localVehicle = GetVehiclePedIsIn(localPed, false)
        local localHooked = GetHookedVehicle(localVehicle)
        local localActivation = GetLocalPlayerActivation()

        local debug = GetConvarInt("simplepassive_debug", 0) ~= 0

        local setInvincible = GetConvarInt("simplepassive_makeinvincible", 0) ~= 0
        local disableCombat = GetConvarInt("simplepassive_disablecombat", 0) ~= 0

        SetEntityInvincible(localPed, setInvincible and localActivation)
        if localVehicle then
            SetEntityInvincible(localVehicle, setInvincible and localActivation)
        end
        if localHooked then
            SetEntityInvincible(localHooked, setInvincible and localActivation)
        end
        if setInvincible then
            if LastVehicle ~= localVehicle then
                if LastVehicle then
                    local inDriverSeat = GetPedInVehicleSeat(LastVehicle, -1)
                    if not inDriverSeat or inDriverSeat ~= localPed then
                        SetEntityInvincible(LastVehicle, false)
                    end
                end
                LastVehicle = localVehicle
            end
            if LastHooked ~= localHooked then
                if LastHooked then
                    local inDriverSeat = GetPedInVehicleSeat(LastHooked, -1)
                    if not inDriverSeat then
                        SetEntityInvincible(LastHooked, false)
                    end
                end
                LastHooked = localHooked
            end
        end

        DisablePlayerFiring(localPlayer, localActivation and disableCombat)

        if localActivation and disableCombat then
            DisableControlAction(0, 45, true) -- INPUT_RELOAD
            DisableControlAction(0, 263, true) -- INPUT_MELEE_ATTACK1
            DisableControlAction(0, 264, true) -- INPUT_MELEE_ATTACK2
            DisableControlAction(0, 140, true) -- INPUT_MELEE_ATTACK_LIGHT
            DisableControlAction(0, 141, true) -- INPUT_MELEE_ATTACK_HEAVY
            DisableControlAction(0, 142, true) -- INPUT_MELEE_ATTACK_ALTERNATE
            DisableControlAction(0, 143, true) -- INPUT_MELEE_BLOCK
            DisableControlAction(0, 24, true) -- INPUT_ATTACK
            DisableControlAction(0, 257, true) -- INPUT_ATTACK2
            DisableControlAction(0, 69, true) -- INPUT_VEH_ATTACK
            DisableControlAction(0, 70, true) -- INPUT_VEH_ATTACK2
            DisableControlAction(0, 91, true) -- INPUT_VEH_PASSENGER_AIM
            DisableControlAction(0, 92, true) -- INPUT_VEH_PASSENGER_ATTACK
            DisableControlAction(0, 114, true) -- INPUT_VEH_FLY_ATTACK
            DisableControlAction(0, 331, true) -- INPUT_VEH_FLY_ATTACK2
        end

        if debug then
            DrawDebugMarker(localPed)
            DrawDebugMarker(localVehicle)
            DrawDebugMarker(localHooked)
        end

        for _, otherPlayer in ipairs(GetActivePlayers()) do
            if otherPlayer == localPlayer then
                goto continue
            end

            local otherPed = GetPlayerPed(otherPlayer)
            local otherVehicle = GetVehiclePedIsIn(otherPed, false)
            local otherHooked = GetHookedVehicle(otherVehicle)
            local otherActivation = GetPlayerActivation(GetPlayerServerId(otherPlayer))
            local shouldDisableCollisions = otherActivation or localActivation

            if shouldDisableCollisions then
                if otherVehicle and IsPedInVehicle(otherVehicle, localPed, false) and GetPedInVehicleSeat(otherVehicle, -1) ~= localPed then
                    goto continue
                end

                local alpha = 255

                if shouldDisableCollisions and not GetIsTaskActive(otherPed, 2) and otherVehicle ~= localVehicle then
                    alpha = GetConvarInt("simplepassive_alpha", 200)
                end

                SetAlpha(otherPed, alpha)
                SetAlpha(otherVehicle, alpha)
                SetAlpha(otherHooked, alpha)

                DisableCollisionsThisFrame(localPed, otherPed)
                DisableCollisionsThisFrame(localPed, otherVehicle)
                DisableCollisionsThisFrame(localPed, otherHooked)

                DisableCollisionsThisFrame(localVehicle, otherPed)
                DisableCollisionsThisFrame(localVehicle, otherVehicle)
                DisableCollisionsThisFrame(localVehicle, otherHooked)

                DisableCollisionsThisFrame(localHooked, otherPed)
                DisableCollisionsThisFrame(localHooked, otherVehicle)
                DisableCollisionsThisFrame(localHooked, otherHooked)

                if debug then
                    DrawDebugMarker(otherPed, 100, 75, 80)
                    DrawDebugMarker(otherVehicle, 100, 75, 80)
                    DrawDebugMarker(otherHooked, 100, 75, 80)
                end

                DisableCamCollisionForEntity(otherPed)

                if otherVehicle then
                    DisableCamCollisionForEntity(otherVehicle)
                end

                if otherHooked then
                    DisableCamCollisionForEntity(otherHooked)
                end
            end

            ::continue::
        end

        if debug then
            local debugText = "Passive Players: "

            for _, playerId in ipairs(GetActivePlayers()) do
                local player = GetPlayerServerId(playerId)
                local activation = GetPlayerActivation(player)

                -- fallback for race conditions
                if activation == nil then
                    activation = GetDefaultActivation()
                end

                debugText = debugText .. " " .. player .. " " .. tostring(activation)
            end

            BeginTextCommandDisplayText("CELL_EMAIL_BCON")
            AddTextComponentSubstringPlayerName(debugText)
            SetTextScale(1, 0.5)
            SetTextColour(255, 255, 255, 255)
            EndTextCommandDisplayText(0, 0)
        end

        PrintCollisionChanges = false

        Citizen.Wait(0)
    end
end

function OnActivationChanged(playerId, activation)
    -- TODO: Add proper boolean checks

    local player = tonumber(playerId)

    if player == nil then
        return
    end

    Activations[player] = activation

    Debug("Received Passive Activation of " .. player .. " (" .. tostring(activation) .. ")")

    local localPlayer = PlayerId()

    if player == GetPlayerServerId(localPlayer) then
        local shouldDisableCombat = GetConvarInt("simplepassive_disablecombat", 0) ~= 0
        SetPlayerCanDoDriveBy(localPlayer, (not activation and shouldDisableCombat) or not shouldDisableCombat)
    end
end

function OnDoCleanup(player)
    Activations[player] = nil
end

function OnPrintTickCommand(source, args, raw)
    if GetConvarInt("simplepassive_debug", 0) ~= 0 then
        PrintCollisionChanges = true
    end
end

exports("getActivation", GetLocalPlayerActivation)
exports("setActivation", SetLocalPlayerActivation)
Citizen.CreateThread(Initialize)
Citizen.CreateThread(HandleCollisions)
RegisterNetEvent("simplepassive:activationChanged", OnActivationChanged)
RegisterNetEvent("simplepassive:doCleanup", OnDoCleanup)
RegisterCommand("passiveprinttick", OnPrintTickCommand, true)
