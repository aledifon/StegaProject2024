using UnityEngine;

public class LevelSceneStateEnum
{
    public enum LevelSceneState
    {        
        Gameplay = 0,
        DeathPanelFadeInOut = 1,        
        GameOverPanel = 3,
        EndScenePanelShowText = 10,                 // FadeIn + ShowText Typewriter FX

        CreditsGamePanelFadeIn = 20,        
        CreditsGamePanelStegaFadeIn = 21,        
        CreditsGamePanelStegaFadeOut = 22,
        CreditsGamePanelAledifonTextFadeIn = 23,
        CreditsGamePanelAledifonTextFadeOut = 24,
        CreditsGamePanelAssetsTextFadeIn = 25,
        CreditsGamePanelAssetsTextFadeOut = 26,
        CreditsGamePanelEndGameTextFadeIn = 27,
        
        CreditsCompleted = 30,                        
    }
}
