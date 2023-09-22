local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local atlases = require("atlases")
local utils = require("utils")
local drawing = require("utils.drawing")

local doors = {}

local smwDoorVert = {}

smwDoorVert.name = "SS2024/SMWDoor"
smwDoorVert.depth = 0
smwDoorVert.minimumSize = {8, 8}
smwDoorVert.placements = {}
smwDoorVert.canResize = {false, true}


local smwDoorHoriz = {}

smwDoorHoriz.name = "SS2024/SMWDoorHorizontal"
smwDoorHoriz.depth = 0
smwDoorHoriz.minimumSize = {8, 8}
smwDoorHoriz.placements = {}
smwDoorHoriz.canResize = {true, false}


table.insert(smwDoorHoriz.placements, {
	name = "SMW Door (Horizontal)",
    data = {
		width = 24,
        height = 8
    }
})

table.insert(smwDoorVert.placements, {
	name = "SMW Door (Vertical)",
    data = {
		width = 8,
        height = 24
    }
})




local texture = "objects/ss2024/smwDoor/smwDoor"

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat"
}

function smwDoorVert.sprite(room, entity)
	local sprites = {}
	local height = entity.height
	local x = entity.x
	local y = entity.y
	for i=0,height/8 - 1,1 do
		local sprite = drawableSprite.fromTexture(texture, {x = x, y = y + i * 8, atlas = atlas})
		sprite:setJustification(0.0, 0.0)
		table.insert(sprites, sprite)
	end

	return sprites
end

function smwDoorHoriz.sprite(room, entity)
	local sprites = {}
	local width = entity.width
	local x = entity.x
	local y = entity.y
	for i=0,width/8 - 1,1 do
		local sprite = drawableSprite.fromTexture(texture, {x = x + i * 8, y = y, atlas = atlas})
		sprite:setJustification(0.0, 0.0)
		table.insert(sprites, sprite)
	end

	return sprites
end

function smwDoorVert.rotate(room, entity, direction)
    return true
end


return {smwDoorVert, smwDoorHoriz}