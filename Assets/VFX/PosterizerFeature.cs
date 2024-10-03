using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Dcam2 {

sealed class PosterizerPass : ScriptableRenderPass
{
    class PassData { public PosterizerController Controller { get; set; } }

    public override void RecordRenderGraph(RenderGraph graph,
                                           ContextContainer context)
    {
        // PosterizerController component reference
        var camera = context.Get<UniversalCameraData>().camera;
        var ctrl = camera.GetComponent<PosterizerController>();
        if (ctrl == null || !ctrl.enabled) return;

        // Not supported: Back buffer source
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        // Destination texture allocation
        var source = resource.activeColorTexture;
        var desc = graph.GetTextureDesc(source);
        desc.name = "Posterizer";
        desc.clearBuffer = false;
        desc.depthBufferBits = 0;
        var dest = graph.CreateTexture(desc);

        // Blit
        var param = new RenderGraphUtils.
          BlitMaterialParameters(source, dest, ctrl.Material, 0);
        graph.AddBlitPass(param, passName: "Posterizer");

        // Destination texture as the camera texture
        resource.cameraColor = dest;
    }
}

public sealed class PosterizerFeature : ScriptableRendererFeature
{
    PosterizerPass _pass;

    public override void Create()
      => _pass = new PosterizerPass
           { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };

    public override void AddRenderPasses(ScriptableRenderer renderer,
                                         ref RenderingData data)
      => renderer.EnqueuePass(_pass);
}

} // namespace Dcam2