
local drawableSprite = require("structs.drawable_sprite")

local SS2024TechialSeason = {}

SS2024TechialSeason.name = "SS2024/TechialSeason"
SS2024TechialSeason.depth = -8501
SS2024TechialSeason.justification = {0.5, 1.0}

SS2024TechialSeason.fieldInformation = {
    orientation = {
        options = {
            {"Up", 0},
            {"Right", 1},
            {"Left", 2},
        },
        editable = false
    }
}
SS2024TechialSeason.placements = {
    name = "spring",
    data = {
        playerCanUse = true,
        orientation = 1,
        oneUse = true,
        keepPlayerState = false,
        freeze = false,
        texture = "objects/spring/"
    }
}

function SS2024TechialSeason.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(entity.texture .. "00", entity)

    if entity.orientation == 1 then
        sprite.rotation = math.pi / 2
    end
    if entity.orientation == 2 then
        sprite.rotation = -math.pi / 2
    end

    return sprite
end

return nil
--return SS2024TechialSeason
