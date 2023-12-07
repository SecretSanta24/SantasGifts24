local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local entity = {}


entity.name = "SantasGifts24/FunnyEffect"
entity.resizable = {true, true}
entity.placements = {
    name = "FunnyEffect",
    data = {
        Width = 8,
        Height = 8,
        startFlag = "funnyEffect_start",
        endFlag = "funnyEffect_end",
        sound = "event:/santas_gifts_funny_effect",
    }
}

function entity.sprite(room, entity, viewport)
    local rectangle
    local drawableSprite
    local fillColor = {0, 0, 1}
    rectangle = utils.rectangle(entity.x, entity.y, 8, 8)
    drawableSprite = drawableRectangle.fromRectangle("fill", rectangle, fillColor)
    drawableSprite.depth = entity.depth
    return drawableSprite
end

return entity
--return nil