local entity = {}

entity.texture = "objects/ss2024/boosterzip/Duskiemoon/booster00"
entity.nodeTexture = "objects/ss2024/boosterzip/Duskiemoon/outline"
entity.name = "SS2024/BoosterZipper"
entity.nodeLimits = {1, 1}
entity.nodeLineRenderType = "line"
entity.nodeVisibility = "always"
entity.placements = {
    name = "BoosterZipper",
    data = {
        zipperMoveTime = 0.5,
        boosterRespawnTime = 1.0
    }
}

return nil
--return entity