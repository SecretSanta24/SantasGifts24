local drawableSprite = require("structs.drawable_sprite")

local minecart = {}

minecart.name = "SS2024/Minecart"
minecart.depth = 5
minecart.justification = {0.5, 1}

minecart.placements = {
    name = "default",
    data = {
        direction = "Right",
        speed = 100.0,
        reentryCooldown = 0.5,
        oneUse = false,
    }
}

minecart.fieldInformation = {
    direction = {
        options = { "Left", "Right" },
        editable = false,
    },
}

function minecart.sprite(room, entity)
    return {
        drawableSprite.fromTexture("objects/ss2024/minecart/body", entity):setJustification(0.5, 1),
        drawableSprite.fromTexture("objects/ss2024/minecart/wheel", entity):setJustification(0.5, 0.5):addPosition(-10.5, -3.5),
        drawableSprite.fromTexture("objects/ss2024/minecart/wheel", entity):setJustification(0.5, 0.5):addPosition(10.5, -3.5),
    }
end

return nil
--return minecart