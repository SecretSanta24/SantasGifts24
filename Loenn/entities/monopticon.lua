local SS2024Monopticon = {}

SS2024Monopticon.name = "SS2024/Monopticon"
SS2024Monopticon.depth = -8500
SS2024Monopticon.justification = {0.5, 1.0}
SS2024Monopticon.nodeLineRenderType = "line"
SS2024Monopticon.texture = "objects/lookout/lookout06"
SS2024Monopticon.nodeLimits = {0, -1}
SS2024Monopticon.placements = {
    name = "mono",

    alternativeName = {"lookout", "binoculars", "honking goose (duckgoose helper)"},
    data = {
        summit = false,
        onlyY = false,
        interactFlag = "lookout_interacting",
        blockDash = true,
        blockJump = true,
        dashCancelDelay = 9,
        openFrames = 20,
        preOpenFrames = 12,
        closeFrames = 20
    }
}

return SS2024Monopticon