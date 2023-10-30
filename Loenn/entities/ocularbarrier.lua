local SS2024OcularBarrier = {}

SS2024OcularBarrier.name = "SS2024/OcularBarrier"
SS2024OcularBarrier.fillColor = {1.0, 0.3, 0.8, 0.6}
SS2024OcularBarrier.borderColor = {1.0, 0.3, 0.9, 0.8}
SS2024OcularBarrier.fieldInformation = {
    activeColor = {
        fieldType = "color"
    },
    inactiveColor = {
        fieldType = "color"
    },
    invertedActiveColor = {
        fieldType = "color"
    },
    invertedInactiveColor = {
        fieldType = "color"
    },
}
SS2024OcularBarrier.placements = {
    name = "bar",
    data = {
        flag = "lookout_interacting",
        invert = false,
        width = 8,
        height = 8,
        activeColor = "99FF66",
        inactiveColor = "005500",
        invertedActiveColor = "6699FF",
        invertedInactiveColor = "000055",
        texturePath = "objects/ss2024/ocularBarrier/"
    }
}

SS2024OcularBarrier.depth = 0

return SS2024OcularBarrier