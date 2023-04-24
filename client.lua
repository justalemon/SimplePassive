-- The activations sent by the server
Activations = {}

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

function DrawDebugMarker(entity, r, g, b)
    if not DoesEntityExist(entity) or not IsEntityOnScreen(entity) then
        return
    end

    local pos = GetEntityCoords(entity, false)
    local _, x, y = GetScreenCoordFromWorldCoord(pos.x, pos.y, pos.z)
    BeginTextCommandDisplayText("CELL_EMAIL_BCON")
    AddTextComponentSubstringPlayerName(tostring(entity))
    SetTextColour(r, g, b, 255)
    SetTextJustification(0)
    EndTextCommandDisplayText(x, y)
end

function DisableCollisionsThisFrame(one, two, print)
    SetEntityNoCollisionEntity(one, two, true)
    SetEntityNoCollisionEntity(two, one, true)

    if print then
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
