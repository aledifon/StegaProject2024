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
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
#endif

namespace FronkonGames.Glitches.Interferences
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Render Pass. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class Interferences
  {
    [DisallowMultipleRendererFeature]
    private sealed class RenderPass : ScriptableRenderPass
    {
      // Internal use only.
      internal Material material { get; set; }

      private readonly Settings settings;

#if UNITY_6000_0_OR_NEWER
#else
      private RenderTargetIdentifier colorBuffer;
      private RenderTextureDescriptor renderTextureDescriptor;

      private readonly int renderTextureHandle0 = Shader.PropertyToID($"{Constants.Asset.AssemblyName}.RTH0");

      private const string CommandBufferName = Constants.Asset.AssemblyName;

      private ProfilingScope profilingScope;
      private readonly ProfilingSampler profilingSamples = new(Constants.Asset.AssemblyName);
#endif

      private static class ShaderIDs
      {
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");

        internal static readonly int Blend = Shader.PropertyToID("_Blend");
        internal static readonly int Offset = Shader.PropertyToID("_Offset");
        internal static readonly int Distortion = Shader.PropertyToID("_Distortion");
        internal static readonly int DistortionSpeed = Shader.PropertyToID("_DistortionSpeed");
        internal static readonly int DistortionDensity = Shader.PropertyToID("_DistortionDensity");
        internal static readonly int DistortionAmplitude = Shader.PropertyToID("_DistortionAmplitude");
        internal static readonly int DistortionFrequency = Shader.PropertyToID("_DistortionFrequency");
        internal static readonly int Scanlines = Shader.PropertyToID("_Scanlines");
        internal static readonly int ScanlinesDensity = Shader.PropertyToID("_ScanlinesDensity");
        internal static readonly int ScanlinesOpacity = Shader.PropertyToID("_ScanlinesOpacity");

        internal static readonly int Brightness = Shader.PropertyToID("_Brightness");
        internal static readonly int Contrast = Shader.PropertyToID("_Contrast");
        internal static readonly int Gamma = Shader.PropertyToID("_Gamma");
        internal static readonly int Hue = Shader.PropertyToID("_Hue");
        internal static readonly int Saturation = Shader.PropertyToID("_Saturation");
      }

      /// <summary> Render pass constructor. </summary>
      public RenderPass(Settings settings) : base()
      {
        this.settings = settings;
#if UNITY_6000_0_OR_NEWER
        profilingSampler = new ProfilingSampler(Constants.Asset.AssemblyName);
#endif
      }

      private void UpdateMaterial()
      {
        material.shaderKeywords = null;
        material.SetFloat(ShaderIDs.Intensity, settings.intensity);

        material.SetInt(ShaderIDs.Blend, (int)settings.blend);
        material.SetFloat(ShaderIDs.Offset, settings.offset);
        material.SetFloat(ShaderIDs.Distortion, settings.distortion);
        material.SetFloat(ShaderIDs.DistortionSpeed, settings.distortionSpeed);
        material.SetFloat(ShaderIDs.DistortionDensity, settings.distortionDensity);
        material.SetFloat(ShaderIDs.DistortionAmplitude, settings.distortionAmplitude);
        material.SetFloat(ShaderIDs.DistortionFrequency, settings.distortionFrequency);
        material.SetFloat(ShaderIDs.Scanlines, settings.scanlines);
        material.SetFloat(ShaderIDs.ScanlinesDensity, settings.scanlinesDensity);
        material.SetFloat(ShaderIDs.ScanlinesOpacity, settings.scanlinesOpacity);

        material.SetFloat(ShaderIDs.Brightness, settings.brightness);
        material.SetFloat(ShaderIDs.Contrast, settings.contrast);
        material.SetFloat(ShaderIDs.Gamma, 1.0f / settings.gamma);
        material.SetFloat(ShaderIDs.Hue, settings.hue);
        material.SetFloat(ShaderIDs.Saturation, settings.saturation);
      }

#if UNITY_6000_0_OR_NEWER
      /// <inheritdoc/>
      public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
      {
        if (material == null || settings.intensity == 0.0f)
          return;

        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        if (resourceData.isActiveTargetBackBuffer == true)
          return;

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
        if (cameraData.camera.cameraType == CameraType.SceneView && settings.affectSceneView == false || cameraData.postProcessEnabled == false)
          return;

        TextureHandle source = resourceData.activeColorTexture;
        TextureHandle destination = renderGraph.CreateTexture(source.GetDescriptor(renderGraph));

        UpdateMaterial();

        RenderGraphUtils.BlitMaterialParameters pass = new(source, destination, material, 0);
        renderGraph.AddBlitPass(pass, $"{Constants.Asset.AssemblyName}.Pass");

        resourceData.cameraColor = destination;
      }
#elif UNITY_2022_3_OR_NEWER
      /// <inheritdoc/>
      public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
      {
        renderTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        renderTextureDescriptor.depthBufferBits = 0;

        colorBuffer = renderingData.cameraData.renderer.cameraColorTargetHandle;
        cmd.GetTemporaryRT(renderTextureHandle0, renderTextureDescriptor, settings.filterMode);
      }

      /// <inheritdoc/>
      public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
      {
        if (material == null ||
            renderingData.postProcessingEnabled == false ||
            settings.intensity <= 0.0f ||
            settings.affectSceneView == false && renderingData.cameraData.isSceneViewCamera == true)
          return;

        CommandBuffer cmd = CommandBufferPool.Get(CommandBufferName);

        if (settings.enableProfiling == true)
          profilingScope = new ProfilingScope(cmd, profilingSamples);

        UpdateMaterial();

#if ENABLE_VR && ENABLE_XR_MODULE
        if (renderingData.cameraData.xr != null &&
            renderingData.cameraData.xr.enabled == true &&
            renderingData.cameraData.xr.singlePassEnabled == true)
        {
          renderingData.cameraData.camera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, renderingData.cameraData.GetProjectionMatrix(0));
          renderingData.cameraData.camera.SetStereoViewMatrix(Camera.StereoscopicEye.Left, renderingData.cameraData.GetViewMatrix(0));
          renderingData.cameraData.camera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, renderingData.cameraData.GetProjectionMatrix(1));
          renderingData.cameraData.camera.SetStereoViewMatrix(Camera.StereoscopicEye.Right, renderingData.cameraData.GetViewMatrix(1));

          cmd.SetSinglePassStereo(SystemInfo.supportsMultiview == true ? SinglePassStereoMode.Multiview : SinglePassStereoMode.Instancing);
        }
#endif
        cmd.Blit(colorBuffer, renderTextureHandle0, material);
        cmd.Blit(renderTextureHandle0, colorBuffer);

        cmd.ReleaseTemporaryRT(renderTextureHandle0);

        if (settings.enableProfiling == true)
          profilingScope.Dispose();

#if ENABLE_VR && ENABLE_XR_MODULE
        cmd.SetSinglePassStereo(SinglePassStereoMode.None);
#endif
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
      }

      public override void OnCameraCleanup(CommandBuffer cmd) => cmd.ReleaseTemporaryRT(renderTextureHandle0);
#else
      #error Unsupported Unity version. Please update to a newer version of Unity.
#endif
    }
  }
}
