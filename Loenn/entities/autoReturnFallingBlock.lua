local fakeTilesHelper = require("helpers.fake_tiles")

local fallingBlock = {}

local directions = {"Up", "Down", "Left", "Right"}


fallingBlock.name = "SS2024/AutoReturnFallingBlock"
fallingBlock.placements = {
    name = "Auto Return Falling Block",
    data = {
        tiletype = "3",
        climbFall = true,
        behind = false,
        width = 8,
        height = 8,
        resetDelay = 1.0,
        flagOnReset = "",
        flagOnFall = "",
        flagTrigger = "",
        resetFlagState = true,
        fallFlagState = true,
        maxSpeed = 160.0,
        acceleration = 500.0,
        direction = "Down",
        landingSound = "",
        returnSound = "",
        shakeSound = "",
        invertFlagTrigger = false,
        returnMaxSpeed = 160.0,
        returnAcceleration = 500.0


    }
}


fallingBlock.fieldInformation = {
    direction = {
        options = directions,
        editable = false
    }
}

fallingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)
fallingBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

function fallingBlock.depth(room, entity)
    return entity.behind and 5000 or 0
end

return nil