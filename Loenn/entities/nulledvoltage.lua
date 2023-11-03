local SS2024NulledVoltage = {}

SS2024NulledVoltage.name = "SS2024/NulledVoltage"
SS2024NulledVoltage.depth = -1000100
SS2024NulledVoltage.fillColor = {0.55, 0.97, 0.96, 0.4}
SS2024NulledVoltage.borderColor = {0.99, 0.96, 0.47, 1.0}
SS2024NulledVoltage.nodeLineRenderType = "line"
SS2024NulledVoltage.nodeLimits = {0, 1}
SS2024NulledVoltage.placements = {
    name = "volts",
    alternativeName = "mature swan (duckgoose helper)",
    data = {
        width = 8,
        height = 8,
        perLevel = false,
        moveTime = 5.0
    }
}
return nil
--return SS2024NulledVoltage