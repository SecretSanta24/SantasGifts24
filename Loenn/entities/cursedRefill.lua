jlocal refill = {}

refill.name = "SS2024/CursedRefill"
refill.depth = -100
refill.placements = {
    {
        name = "Cursed Refill",
        data = {
            oneUse = false
        }
    }
}

function refill.texture(room, entity)
    return "objects/ss2024/lyraCursedRefill/idle00"
end

return nil