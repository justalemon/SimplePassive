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
