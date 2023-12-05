local zipline = {}

zipline.name = "SS2024/ElectricZipline"
zipline.depth = 0
zipline.texture = "objects/ss2024/zipline/handle"
zipline.nodeTexture = "objects/ss2024/zipline/handle_end"
zipline.nodeLimits = {0, -1}
zipline.nodeVisibility = "always"
zipline.nodeLineRenderType = "line"
zipline.placements = {
    name = "Electric Zipline",
    data = {
        useStamina = true
    }
}

return nil
