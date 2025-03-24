using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DrawInstancedRenderPassFeature : ScriptableRendererFeature
{
    class DrawInstancedRenderPass : ScriptableRenderPass
    {
        public bool isDrawOpaque = true;
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            if (isDrawOpaque)
            {
                JellyfishSpawner.instance?.DrawInstanced(cmd);
            }
            else
            {
				JellyfishSpawner.instance?.DrawTransparentInstanced(cmd);
			}

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
			CommandBufferPool.Release(cmd);
		}

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    DrawInstancedRenderPass m_RenderOpaquePass;
	DrawInstancedRenderPass m_RenderTransparentPass;
	/// <inheritdoc/>
	public override void Create()
    {
        m_RenderOpaquePass = new DrawInstancedRenderPass();
        m_RenderOpaquePass.isDrawOpaque = true;
		// Configures where the render pass should be injected.
		m_RenderOpaquePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

		m_RenderTransparentPass = new DrawInstancedRenderPass();
		m_RenderTransparentPass.isDrawOpaque = false;
		// Configures where the render pass should be injected.
		m_RenderTransparentPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
	}

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_RenderOpaquePass);
		renderer.EnqueuePass(m_RenderTransparentPass);
	}
}


