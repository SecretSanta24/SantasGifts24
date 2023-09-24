local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local entity = {}

entity.name = "SS2024/ParamOnFlagController"
entity.placements = {
    {
        name = "ParamOnFlagController",
        placementType = "rectangle",
        data = {
            Flag = "",
            Parameter = "",
            OnValue = 1.0,
            OffValue = 0.0,
        }
    }
}

function entity.sprite(room, entity, viewport)
    local rectangle
    local drawableSprite
    local fillColor = {1, 1, 1, 0.5}
    rectangle = utils.rectangle(entity.x, entity.y, 8, 8)
    drawableSprite = drawableRectangle.fromRectangle("fill", rectangle, fillColor)
    drawableSprite.depth = entity.depth
    return drawableSprite
end

return nil
-- return entity