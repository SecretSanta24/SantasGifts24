local refill = {}

refill.name = "SS2024/UncursedRefill"
refill.depth = -100
refill.placements = {
    {
        name = "Uncursed Refill",
        data = {
            oneUse = false
        }
    }
}

function refill.texture(room, entity)
    return "objects/ss2024/lyraUncursedRefill/idle00"
end

return nil