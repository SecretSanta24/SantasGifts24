local xnaColors = require("consts.xna_colors")
local yellow = xnaColors.Yellow
local waterLightning = {}

waterLightning.name = "SS2024/WaterLightning"
waterLightning.fillColor = {yellow[1] * 0.3, yellow[2] * 0.3, yellow[3] * 0.3, 0.6}
waterLightning.borderColor = {yellow[1] * 0.8, yellow[2] * 0.8, yellow[3] * 0.8, 0.8}
waterLightning.placements = {
    name = "Water Lightning",
    data = {
        width = 8,
        height = 8
    }
}

waterLightning.depth = 0

return nil