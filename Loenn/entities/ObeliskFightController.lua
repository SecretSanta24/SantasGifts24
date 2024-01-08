local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local entity = {}

entity.name = "SS2024/ObeliskFightController"
entity.placements = {
    {
        name = "ObeliskFightController",
        placementType = "rectangle",
        data = {
            trueEvil = false,
        }
    }
}

function entity.sprite(room, entity, viewport)
    local rectangle
    local drawableSprite
    local fillColor = {1, 0, 0, 0.5}
    rectangle = utils.rectangle(entity.x, entity.y, 8, 8)
    drawableSprite = drawableRectangle.fromRectangle("fill", rectangle, fillColor)
    drawableSprite.depth = entity.depth
    return drawableSprite
end

return nil
-- return entity