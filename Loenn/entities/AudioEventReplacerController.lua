local entity = {}

entity.name = "SS2024/AudioEventReplacerController"
entity.texture = "objects/controllers/AudioEventReplacerController/controller"
entity.placements = {
    {
        name = "AudioEventReplacerController",
        placementType = "rectangle",
        data = {
            OldAudioEvent = "",
            NewAudioEvent = "event:/none",
            LogAudioPlaying = false,
        }
    }
}

return nil
--return entity