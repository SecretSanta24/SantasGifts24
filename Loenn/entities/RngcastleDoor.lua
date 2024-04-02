local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local entity = {}

entity.name = "SS2024/RngcastleDoor"
entity.nodeLimits = {1, 1}
entity.canResize = {true, true}
entity.nodeVisibility = "always"
entity.placements = {
    {
        name = "RngcastleDoor",
        placementType = "rectangle",
        data = {
            startingLevel = "",
            bossLevel = "",
            endLevel = "",
            rooms = "",
            heartImage = "checkpoint",
            width = 16,
            height = 16
        }
    }
}

function entity.sprite(room, entity, viewport)
    local rectangle
    local drawableSprite
    local fillColor = {1, 1, 1}
    rectangle = utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    drawableSprite = drawableRectangle.fromRectangle("bordered", rectangle, {0, 0, 0, 0}, fillColor)
    drawableSprite.depth = entity.depth
    return drawableSprite
end


function entity.nodeSprite(room, entity, node)
    local rectangle
    local drawableSprite
    local fillColor = {0.83, 0.68, 0.21}
    rectangle = utils.rectangle(node.x, node.y, 2, 2)
    drawableSprite = drawableRectangle.fromRectangle("fill", rectangle, fillColor)
    drawableSprite.depth = entity.depth
    return drawableSprite
end

return nil
--return entity