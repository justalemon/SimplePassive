function DrawDebugMarker(entity, r, g, b)
    if not DoesEntityExist(entity) or not IsEntityOnScreen(entity) then
        return
    end

    local _, x, y = GetScreenCoordFromWorldCoord()
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
