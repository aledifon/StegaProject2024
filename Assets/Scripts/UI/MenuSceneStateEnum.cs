using UnityEngine;

public class MenuSceneStateEnum
{
    public enum MenuSceneState
    {        
        Init = 0,
        IntroPanelFadeIn = 10,
        IntroPanelStegaImageFadeIn = 11,
        //Delay 2s
        IntroPanelStegaImageFadeOut = 12,
        IntroPanelAledifonTextFadeIn = 13,
        //Delay 2s
        IntroPanelAledifonTextFadeOut = 14,
        IntroPanelFadeOut = 15,
        MenuPanelTitleTextFadeIn = 16,
        
        MenuPanelState = 20,
        ControlPanelState = 30,
        QuitGameState = 40,

        IntroScenePanelShowText = 50,
        // Delay 2s
        StartGame = 100
    }
}
