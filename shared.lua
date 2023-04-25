function Debug(message)
    if GetConvarInt("simplepassive_debug", 0) == 0 then
        return
    end

    print(message)
end

function GetDefaultActivation()
    return GetConvarInt("simplepassive_default", 0) ~= 0
end
