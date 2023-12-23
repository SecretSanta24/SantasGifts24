local portal = {}

portal.name = "SS2024/LightDarkPortal"
portal.depth = -1000000
portal.nodeLineRenderType = "line"
portal.nodeLimits = {1, 1}
portal.nodeLineRenderType = "line"
portal.nodeVisibility = "always"
portal.justification = {0.5, 0.5}
portal.canResize = {false, false}
portal.placements = {
    name = "lightDarkPortal",
    data = {
        persistent = false,
    }
}
portal.texture = "objects/ss2024/lightDarkPortal/normal00"
portal.nodeTexture = "objects/ss2024/lightDarkPortal/dark00"

return nil
--return portal
