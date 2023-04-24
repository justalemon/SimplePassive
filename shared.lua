function Debug(message)
    if not (not GetConvarInt("simplepassive_debug", 0)) then
        return
    end

    print(message)
end

function GetDefaultActivation()
    return not (not GetConvarInt("simplepassive_default", 0))
end
