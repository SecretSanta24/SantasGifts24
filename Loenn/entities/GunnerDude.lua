local gunnerDude = {}

gunnerDude.name = "SS2024/GunnerDude"
gunnerDude.depth = -2000000
gunnerDude.texture = "SS2024/corkr900/GunnerDude/normal00"
gunnerDude.placements = {
    name = "gunnerDude",
    data = {
        faceLeft = false,
    }
}

function gunnerDude.scale(room, entity)
    if entity.faceLeft then
        return { -1, 1 }
    end
    return { 1, 1 }
end

function gunnerDude.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.faceLeft = not entity.faceLeft
    end

    return horizontal
end

return nil
--return gunnerDude
