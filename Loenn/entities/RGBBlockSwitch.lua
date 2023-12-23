local entity = {}
local utils = require("utils")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableLineStruct = require("structs.drawable_line")

entity.name = "SS2024/RGBBlockSwitch"
entity.depth = -11011
entity.minimumSize = {24, 24}
entity.placements = {
    name = "RGBBlockSwitch",
    data = {
        ActiveColor = 0,
        width = 16,
        height = 16,
        ResetColorsOnDeath = true
    }
}

entity.fieldInformation = {
    ActiveColor = {
        options = {
            ["Red"] = 0,
            ["Green"] = 1,
            ["Blue"] = 2,
        }
    }
}

local colors = {
    {1, 0, 0, 0.5},
    {0, 1, 0, 0.5},
    {0, 0, 1, 0.5},
}

function entity.sprite(room, entity, viewport)
    local rectangle
    local drawableSprite
    local fillColor = colors[entity.ActiveColor+1]
    local borderColor = {1, 1, 1, 0.8}
    rectangle = utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    drawableSprite = drawableRectangle.fromRectangle("bordered", rectangle, fillColor, borderColor)
    return drawableSprite
end

return nil
-- return entity