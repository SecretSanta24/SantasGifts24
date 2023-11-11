local drawableSprite = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")
local SS2024Thalassocardiologist = {}


SS2024Thalassocardiologist.name = "SS2024/Thalassocardiologist"
SS2024Thalassocardiologist.depth = 0
SS2024Thalassocardiologist.texture = "objects/puffer/idle07"
SS2024Thalassocardiologist.fieldInformation = {
    particleColor1 = {
        fieldType = "color"
    },
    particleColor2 = {
        fieldType = "color"
    }
}
SS2024Thalassocardiologist.placements = {
    {
        name = "left",
        data = {
            right = false,
            particleColor1 = "880088",
            particleColor2 = "ffffff",
            sprite = "SS2024Thalassocardiologist",
            explodeAudio = "event:/new_content/game/10_farewell/puffer_splode",
            doNotAlert = true,
            doNotRenderEyes = true,
            doNotRenderOutline = true
        }
    },
    {
        name = "right",
        data = {
            right = true,
            particleColor1 = "880088",
            particleColor2 = "ffffff",
            sprite = "SS2024Thalassocardiologist",
            explodeAudio = "event:/new_content/game/10_farewell/puffer_splode",
            doNotAlert = true,
            doNotRenderEyes = true,
            doNotRenderOutline = true
        }
    }
}

function SS2024Thalassocardiologist.scale(room, entity)
    local right = entity.right

    return right and 1 or -1, 1
end

function SS2024Thalassocardiologist.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.right = not entity.right
    end

    return horizontal
end

function SS2024Thalassocardiologist.selection(room, entity)
    local x, y = entity.x, entity.y
    return utils.rectangle(x-8, y-8, 16, 16)
end
return nil
--return SS2024Thalassocardiologist