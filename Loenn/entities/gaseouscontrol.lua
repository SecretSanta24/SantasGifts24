local SS2024GaseousControl = {}

SS2024GaseousControl.name = "SS2024/GaseousControl"
SS2024GaseousControl.depth = 0
SS2024GaseousControl.texture = "objects/ss2024/gaseous"
SS2024GaseousControl.placements = {
    name = "gaseous",
    data = {
        colorgradeA = "feelingdown",
        colorgradeB = "golden",
        drainRate = 150,
        fadeInTag = "o2_in_tag",
        fadeOutTag = "o2_out_tag",
        fastDeath = true,
        flag = "o2_flag",
        recoverRate = 1000,
        anxiety = 0.3,
        musicParamName = "param_here",
        musicParamMin = 0,
        musicParamMax = 1,
        cameraZoomTarget = 1,
    }
}

return nil
--return SS2024GaseousControl