local xnaColors = require("consts.xna_colors")
local yellow = xnaColors.Yellow
local flagLightning = {}

flagLightning.name = "SS2024/FlagLightning"
flagLightning.fillColor = {yellow[1] * 0.3, yellow[2] * 0.3, yellow[3] * 0.3, 0.6}
flagLightning.borderColor = {yellow[1] * 0.8, yellow[2] * 0.8, yellow[3] * 0.8, 0.8}
flagLightning.placements = {
    name = "Flag Lightning",
    data = {
        width = 8,
        height = 8
    }
}

flagLightning.depth = -1000000

--return flagLightning
return nil