////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Martin Bustos @FronkonGames <fronkongames@gmail.com>. All rights reserved.
//
// THIS FILE CAN NOT BE HOSTED IN PUBLIC REPOSITORIES.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using UnityEditor;
using static FronkonGames.Glitches.Interferences.Inspector;

namespace FronkonGames.Glitches.Interferences.Editor
{
  /// <summary> Interferences inspector. </summary>
  [CustomPropertyDrawer(typeof(Interferences.Settings))]
  public class InterferencesFeatureSettingsDrawer : Drawer
  {
    private Interferences.Settings settings;

    protected override void ResetValues() => settings?.ResetDefaultValues();

    protected override void InspectorGUI()
    {
      settings ??= GetSettings<Interferences.Settings>();

      /////////////////////////////////////////////////
      // Common.
      /////////////////////////////////////////////////
      settings.intensity = Slider("Intensity", "Controls the intensity of the effect [0, 1]. Default 0.", settings.intensity, 0.0f, 1.0f, 1.0f);

      /////////////////////////////////////////////////
      // Interferences.
      /////////////////////////////////////////////////
      Separator();

      settings.blend = (ColorBlends)EnumPopup("Blend", "Color blend operation. Default Solid.", settings.blend, ColorBlends.Solid);

      settings.offset = Slider("Offset", "Interference size [0, 10]. Default 1.", settings.offset, 0.0f, 10.0f, 1.0f);
      settings.distortion = Slider("Distortion", "Distortion [0, 2]. Default 0.25.", settings.distortion, 0.0f, 2.0f, 0.25f);
      IndentLevel++;
      settings.distortionSpeed = Slider("Speed", "Distortion speed [0, 100]. Default 10.", settings.distortionSpeed, 0.0f, 100.0f, 10.0f);
      settings.distortionDensity = Slider("Density", "Distortion density [0, 10]. Default 2.", settings.distortionDensity, 0.0f, 10.0f, 2.0f);
      settings.distortionAmplitude = Slider("Amplitude", "Distortion speed [0, 100]. Default 10.", settings.distortionAmplitude, 0.0f, 2.0f, 0.15f);
      settings.distortionFrequency = Slider("Frequency", "Distortion frequency [0, 10]. Default 0.3.", settings.distortionFrequency, 0.0f, 10.0f, 0.3f);
      IndentLevel--;

      settings.scanlines = Slider("Scanlines", "Scanlines [0, 1]. Default 0.75.", settings.scanlines, 0.0f, 1.0f, 0.75f);
      IndentLevel++;
      settings.scanlinesDensity = Slider("Density", "Scanlines density [0, 1]. Default 0.25.", settings.scanlinesDensity, 0.0f, 1.0f, 0.25f);
      settings.scanlinesOpacity = Slider("Opacity", "Scanlines opacity [0, 1]. Default 0.5.", settings.scanlinesOpacity, 0.0f, 1.0f, 0.5f);
      IndentLevel--;

      /////////////////////////////////////////////////
      // Color.
      /////////////////////////////////////////////////
      Separator();

      if (Foldout("Color") == true)
      {
        IndentLevel++;

        settings.brightness = Slider("Brightness", "Brightness [-1.0, 1.0]. Default 0.", settings.brightness, -1.0f, 1.0f, 0.0f);
        settings.contrast = Slider("Contrast", "Contrast [0.0, 10.0]. Default 1.", settings.contrast, 0.0f, 10.0f, 1.0f);
        settings.gamma = Slider("Gamma", "Gamma [0.1, 10.0]. Default 1.", settings.gamma, 0.01f, 10.0f, 1.0f);
        settings.hue = Slider("Hue", "The color wheel [0.0, 1.0]. Default 0.", settings.hue, 0.0f, 1.0f, 0.0f);
        settings.saturation = Slider("Saturation", "Intensity of a colors [0.0, 2.0]. Default 1.", settings.saturation, 0.0f, 2.0f, 1.0f);

        IndentLevel--;
      }

      /////////////////////////////////////////////////
      // Advanced.
      /////////////////////////////////////////////////
      Separator();

      if (Foldout("Advanced") == true)
      {
        IndentLevel++;

#if !UNITY_6000_0_OR_NEWER
        settings.filterMode = (FilterMode)EnumPopup("Filter mode", "Filter mode. Default Bilinear.", settings.filterMode, FilterMode.Bilinear);
#endif
        settings.affectSceneView = Toggle("Affect the Scene View?", "Does it affect the Scene View?", settings.affectSceneView);
        settings.whenToInsert = (UnityEngine.Rendering.Universal.RenderPassEvent)EnumPopup("RenderPass event",
          "Render pass injection. Default BeforeRenderingPostProcessing.",
          settings.whenToInsert,
          UnityEngine.Rendering.Universal.RenderPassEvent.BeforeRenderingPostProcessing);
#if !UNITY_6000_0_OR_NEWER
        settings.enableProfiling = Toggle("Enable profiling", "Enable render pass profiling", settings.enableProfiling);
#endif

        IndentLevel--;
      }
    }
  }
}
