local shockwaveEmiiter = {}

shockwaveEmiiter.name = "SS2024/ShockwaveEmitter"
shockwaveEmiiter.depth = 100
shockwaveEmiiter.placements = {
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

function shockwaveEmiiter.texture(room, entity)
    return "objects/ss2024/ellipticalShockwave/loennspriteplaceholder"
end

return shockwaveEmiiter