local controller = {}

controller.name = "SS2024/RetainFlagOnRespawnController"
controller.placements = {
    {
        name = "RetainFlagStatesOnRespawnController"
    }
}

function controller.texture(room, entity)
    return "objects/ss2024/retainFlagStateOnRespawn/loennsprite"
end

--return controller
return nil