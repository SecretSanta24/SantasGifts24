local drawableSprite = require("structs.drawable_sprite")

local smwKey = {}

smwKey.name = "SS2024/SMWKey"
smwKey.depth = 100
smwKey.placements = {
    name = "SMW Key",
    data = {
        grabbable = true
    }
}

-- Offset is from sprites.xml, not justifications
local offsetY = -10
local texture = "objects/ss2024/smwKey/leafkey00"

function smwKey.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(texture, entity)

    sprite.y += offsetY

    return sprite
end

--return smwKey
return nil