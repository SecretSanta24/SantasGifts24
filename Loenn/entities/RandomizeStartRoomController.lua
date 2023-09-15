local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local entity = {}

entity.name = "SS2024/RandomizeStartRoomController"
entity.placements = {
    {
        name = "RandomizeStartRoomController",
        placementType = "rectangle",
        data = {
            RoomNames = ""
        }
    }
}

function entity.sprite(room, entity, viewport)
    local rectangle
    local drawableSprite
    local fillColor = {0.83, 0.68, 0.21}
    rectangle = utils.rectangle(entity.x, entity.y, 8, 8)
    drawableSprite = drawableRectangle.fromRectangle("fill", rectangle, fillColor)
    drawableSprite.depth = entity.depth
    return drawableSprite
end

return nil
-- return entity