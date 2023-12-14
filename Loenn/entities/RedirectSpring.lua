local springDepth = -8501
local springTexture = "objects/spring/00"

local springUp = {}

springUp.name = "SS2024/RedirectSpringUp"
springUp.depth = springDepth
springUp.justification = {0.5, 1.0}
springUp.texture = springTexture
springUp.placements = {
    name = "redirectSpringUp",
    data = {
    }
}

function springUp.rotate(room, entity, direction)
    if direction > 0 then
        entity._name = "SS2024/RedirectSpringLeft"

    else
        entity._name = "SS2024/RedirectSpringRight"
    end

    return true
end

local springRight = {}

springRight.name = "SS2024/RedirectSpringLeft"
springRight.depth = springDepth
springRight.justification = {0.5, 1.0}
springRight.texture = springTexture
springRight.rotation = math.pi / 2
springRight.placements = {
    name = "redirectSpringRight",
    data = {
    }
}

function springRight.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity._name = "SS2024/RedirectSpringRight"
    end

    return horizontal
end

function springRight.rotate(room, entity, direction)
    if direction < 0 then
        entity._name = "SS2024/RedirectSpringUp"
    end

    return direction < 0
end

local springLeft = {}

springLeft.name = "SS2024/RedirectSpringRight"
springLeft.depth = springDepth
springLeft.justification = {0.5, 1.0}
springLeft.texture = springTexture
springLeft.rotation = -math.pi / 2
springLeft.placements = {
    name = "redirectSpringLeft",
    data = {
    }
}

function springLeft.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity._name = "SS2024/RedirectSpringLeft"
    end

    return horizontal
end

function springLeft.rotate(room, entity, direction)
    if direction > 0 then
        entity._name = "SS2024/RedirectSpringUp"
    end

    return direction > 0
end

return nil
--return {
--    springUp,
--    springRight,
--    springLeft
--}
