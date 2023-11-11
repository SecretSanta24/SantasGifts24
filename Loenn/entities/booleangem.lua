local SS2024BooleanGem = {}

SS2024BooleanGem.name = "SS2024/BooleanGem"
SS2024BooleanGem.depth = -100
SS2024BooleanGem.placements = {
    {
        name = "boolean",
        alternativeName = "strago (duckgoose helper)",
        data = {
            oneUse = false,
            flag = "booleanGemFlag",
            stopMomentum = true,
            path = "objects/refill/",
            particleColors = "d3edff,94a5ef,a5c3ff,6c74dd"
        }
    }
}

function SS2024BooleanGem.texture(room, entity)
    return entity.path.."idle00"
end

return nil
--return SS2024BooleanGem