using UnityEngine;

public class MenuSceneStateEnum
{
    public enum MenuSceneState
    {        
        Init = 0,
        IntroPanelFadeIn = 10,
        IntroPanelStegaImageFadeIn = 11,        
        IntroPanelStegaImageFadeOut = 12,
        IntroPanelFadeOut = 13,
        IntroPanelAledifonTextFadeIn = 14,        
        IntroPanelAledifonTextFadeOut = 15,
        
        MenuPanelTitleTextFadeIn = 16,
        
        MenuPanelState = 20,
        ControlPanelState = 30,
        QuitGameState = 40,

        IntroScenePanelShowText = 50,        
        StartGame = 100
    }
}
