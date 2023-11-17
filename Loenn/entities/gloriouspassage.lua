local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")

local SS2024GloriousPassage = {}

SS2024GloriousPassage.name = "SS2024/GloriousPassage"
SS2024GloriousPassage.depth = 10
SS2024GloriousPassage.minimumSize = {8, 8}
SS2024GloriousPassage.placements = {
    {
        name = "passage",
        alternativeName = "door",
        data = {
            width = 16,
            height = 24,
            roomName = "",
            audio = "",
            closedPath = "objects/ss2024/gloriousPassage/closed",
            openPath = "objects/ss2024/gloriousPassage/open",
            simpleTrigger = false,
        }
    }
}

local ninePatchOptions = {
    mode = "border",
    borderMode = "repeat",
}

function SS2024GloriousPassage.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local crystalTexture = entity.closedPath
    local rectTexture = "objects/ss2024/rect"

    local ninePatch = drawableNinePatch.fromTexture(rectTexture, ninePatchOptions, x, y, width, height)
    local crystalSprite = drawableSprite.fromTexture(crystalTexture, entity)
    local sprites = ninePatch:getDrawableSprite()

    crystalSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    if entity.simpleTrigger then
        crystalSprite:setColor({1, 1, 1, 0.3})
    end
    table.insert(sprites, crystalSprite)

    return sprites
end
return nil
--return SS2024GloriousPassage