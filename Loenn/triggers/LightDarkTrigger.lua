local lightDarkTrigger = {}

local modeOptions = {
    "Normal",
    "Dark",
}

lightDarkTrigger.name = "SS2024/LightDarkTrigger"
lightDarkTrigger.fieldInformation = {
    mode = {
        options = modeOptions,
        editable = false,
    },
}
lightDarkTrigger.placements = {
    name = "lightDarkTrigger",
    data = {
        mode = "Normal",
        persistent = true,
        removeSelf = false,
        onlyOnce = false,
    }
}

return lightDarkTrigger
