local fakeTilesHelper = require("helpers.fake_tiles")

local groupedFallingBlock = {}

groupedFallingBlock.name = "SS2024/GroupedFallingDashBlock"

groupedFallingBlock.placements = {
    name = "Grouped Falling Dash Block",
    data = {
        width = 8,
        height = 8,
        tiletype = "3",
        climbFall = true
    }
}

groupedFallingBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", false)
groupedFallingBlock.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return groupedFallingBlock