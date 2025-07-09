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
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FronkonGames.Glitches.Interferences
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Settings. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class Interferences
  {
    /// <summary> Settings. </summary>
    [Serializable]
    public sealed class Settings
    {
      public Settings() => ResetDefaultValues();

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Common settings.

      /// <summary> Controls the intensity of the effect [0, 1]. Default 1. </summary>
      /// <remarks> An effect with Intensity equal to 0 will not be executed. </remarks>
      public float intensity = 1.0f;

      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Interferences settings.

      /// <summary> Color blend operation. Default Solid. </summary>
      public ColorBlends blend = ColorBlends.Solid;

      /// <summary> Interference size [0, 10]. Default 1. </summary>
      public float offset = 1.0f;

      /// <summary> Distortion [0, 2]. Default 0.25. </summary>
      public float distortion = 0.25f;

      /// <summary> Distortion speed [0, 100]. Default 10. </summary>
      public float distortionSpeed = 10.0f;

      /// <summary> Distortion density [0, 10]. Default 2. </summary>
      public float distortionDensity = 2.0f;

      /// <summary> Distortion amplitude [0, 5]. Default 0.15. </summary>
      public float distortionAmplitude = 0.15f;

      /// <summary> Distortion frequency [0, 10]. Default 0.3. </summary>
      public float distortionFrequency = 0.3f;

      /// <summary> Scanlines [0, 1]. Default 0.75. </summary>
      public float scanlines = 0.75f;

      /// <summary> Scanlines density [0, 1]. Default 0.25. </summary>
      public float scanlinesDensity = 0.25f;

      /// <summary> Scanlines opacity [0, 1]. Default 0.5. </summary>
      public float scanlinesOpacity = 0.5f;

      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Color settings.

      /// <summary> Brightness [-1, 1]. Default 0. </summary>
      public float brightness = 0.0f;

      /// <summary> Contrast [0, 10]. Default 1. </summary>
      public float contrast = 1.0f;

      /// <summary> Gamma [0.1, 10]. Default 1. </summary>
      public float gamma = 1.0f;

      /// <summary> The color wheel [0, 1]. Default 0. </summary>
      public float hue = 0.0f;

      /// <summary> Intensity of a colors [0, 2]. Default 1. </summary>
      public float saturation = 1.0f;

      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
      #region Advanced settings.
      /// <summary> Does it affect the Scene View? </summary>
      public bool affectSceneView = false;

#if !UNITY_6000_0_OR_NEWER
      /// <summary> Enable render pass profiling. </summary>
      public bool enableProfiling = false;

      /// <summary> Filter mode. Default Bilinear. </summary>
      public FilterMode filterMode = FilterMode.Bilinear;
#endif

      /// <summary> Render pass injection. Default BeforeRenderingPostProcessing. </summary>
      public RenderPassEvent whenToInsert = RenderPassEvent.BeforeRenderingPostProcessing;
      #endregion
      /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

      /// <summary> Reset to default values. </summary>
      public void ResetDefaultValues()
      {
        intensity = 1.0f;

        blend = ColorBlends.Solid;
        offset = 1.0f;
        distortion = 0.25f;
        distortionSpeed = 10.0f;
        distortionDensity = 2.0f;
        distortionAmplitude = 0.15f;
        distortionFrequency = 0.3f;
        scanlines = 0.75f;
        scanlinesDensity = 0.25f;
        scanlinesOpacity = 0.5f;

        brightness = 0.0f;
        contrast = 1.0f;
        gamma = 1.0f;
        hue = 0.0f;
        saturation = 1.0f;

        affectSceneView = false;
#if !UNITY_6000_0_OR_NEWER
        enableProfiling = false;
        filterMode = FilterMode.Bilinear;
#endif
        whenToInsert = RenderPassEvent.BeforeRenderingPostProcessing;
      }
    }
  }
}
