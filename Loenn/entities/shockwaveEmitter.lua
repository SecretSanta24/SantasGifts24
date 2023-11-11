local shockwaveEmitter = {}

shockwaveEmitter.name = "SS2024/ShockwaveEmitter"
shockwaveEmitter.depth = 100
shockwaveEmitter.placements = {
    {
        name = "Shockwave Emitter",
        data = {
            focalRatio = 1.5,
            frequency = 5,
            initialSize = 1,
            flag = "",
            firstSpawn = 5,
            shockwaveThickness = 3,
            expandRate = 30
        }

    }
}

function shockwaveEmitter.texture(room, entity)
    return "objects/ss2024/ellipticalShockwave/loennspriteplaceholder"
end

return nil