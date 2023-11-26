local shockwaveEmitter = {}

shockwaveEmitter.name = "SS2024/ShockwaveEmitter"
shockwaveEmitter.depth = 100
shockwaveEmitter.placements = {
    {
        name = "Shockwave Emitter",
        data = {
            focalRatio = "1.5",
            initialSize = "1",
            timers = "3,3,5",
            shockwaveThickness = "3",
            expand = "30",
            breakoutSpeeds = "30",
            flag = "shockwaveStarter",
            cycle = false
        }

    }
}

function shockwaveEmitter.texture(room, entity)
    return "objects/ss2024/ellipticalShockwave/loennspriteplaceholder"
end

return nil
--return shockwaveEmitter