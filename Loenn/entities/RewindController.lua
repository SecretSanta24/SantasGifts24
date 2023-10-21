local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local entity = {}

entity.name = "SS2024/RewindController"
entity.placements = {
    {
        name = "RewindController",
        placementType = "rectangle",
        data = {
            requiredFlag = "",
            ignoreIfNotMoving = false
        }
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

return nil
-- return entity