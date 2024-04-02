local fakeTilesHelper = require("helpers.fake_tiles")
local entity = {}

entity.name = "SS2024/EvilTilesetController"
entity.texture = "objects/controllers/EvilTilesetController/controller"
entity.placements = {
    {
        name = "EvilTilesetController",
        placementType = "rectangle",
        data = {
            tileset = "z"
        }
    }
}


entity.fieldInformation = fakeTilesHelper.getFieldInformation("tileset")

return nil
--return entity