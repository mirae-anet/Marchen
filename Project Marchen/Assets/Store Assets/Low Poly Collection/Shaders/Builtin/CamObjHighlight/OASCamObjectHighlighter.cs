using UnityEngine;
using System.Linq;
using OffAxisStudios.ImageEffects;
using UnityEngine.Rendering;

namespace OffAxisStudios
{
	[RequireComponent(typeof(Camera))]
	public class OASCamObjectHighlighter : MonoBehaviour
	{
		public enum HighlightType
		{
			Glow = 0,
			Solid = 1
		}

		public enum SortingType
		{
			Overlay = 3,
			DepthFilter = 4
		}

		public enum FillType
		{
			Fill,
			Outline
		}

		public enum RTResolution
		{
			Quarter = 4,
			Half = 2,
			Full = 1
		}

		public Renderer[] RenderersToHighlight;
		public HighlightType SelectedEffect = HighlightType.Glow;
		public SortingType RenderSortType = SortingType.DepthFilter;
		public FillType RenderFillType = FillType.Outline;
		public RTResolution Resolution = RTResolution.Full;
		public string OccludersTag = "Occluder";
		public Color HighlightColour = new Color(1f, 0f, 0f, 0.65f);

		private OASBlurOptimized _blur;
		private Renderer[] _occluders = null;
		private Material _highlightMaterial;
		private CommandBuffer _renderBuffer;
		private int _rtWidth = 512;
		private int _rtHeight = 512;

		private void Awake()
		{
			CreateBuffers();
			CreateMaterials();
			SetOccluderObjects();

			_blur = gameObject.AddComponent<OASBlurOptimized>();
			_blur.blurShader = Shader.Find("Hidden/OAS Fast Blur");
			_blur.enabled = false;

			_rtWidth = (int)(Screen.width / (float)Resolution);
			_rtHeight = (int)(Screen.height / (float)Resolution);
		}

		private void CreateBuffers()
		{
			_renderBuffer = new CommandBuffer();
		}

		private void ClearCommandBuffers()
		{
			_renderBuffer.Clear();
		}

		private void CreateMaterials()
		{
			_highlightMaterial = new Material(Shader.Find("Off Axis Studios/Highlight"));
		}

		private void SetOccluderObjects()
		{
			if (string.IsNullOrEmpty(OccludersTag))
				return;

			var occluderGOs = GameObject.FindGameObjectsWithTag(OccludersTag);

			_occluders = occluderGOs.Select(go => go.GetComponent<Renderer>()).Where(renderer1 => renderer1 != null).ToArray();
		}

		private void RenderHighlights(RenderTexture rt)
		{
			var rtid = new RenderTargetIdentifier(rt);
			_renderBuffer.SetRenderTarget(rtid);


			foreach (var rend in RenderersToHighlight)
			{
				if (rend != null)
				{
					_renderBuffer.DrawRenderer(rend, _highlightMaterial, 0, (int)RenderSortType);
				}
			}

			RenderTexture.active = rt;
			Graphics.ExecuteCommandBuffer(_renderBuffer);
			RenderTexture.active = null;
		}

		private void RenderOccluders(RenderTexture rt)
		{
			if (_occluders == null)
				return;

			var rtid = new RenderTargetIdentifier(rt);
			_renderBuffer.SetRenderTarget(rtid);

			_renderBuffer.Clear();

			foreach (var renderer1 in _occluders)
			{
				_renderBuffer.DrawRenderer(renderer1, _highlightMaterial, 0, (int)RenderSortType);
			}

			RenderTexture.active = rt;
			Graphics.ExecuteCommandBuffer(_renderBuffer);
			RenderTexture.active = null;
		}

		private void OnRenderImage(Texture source, RenderTexture destination)
		{
			RenderTexture highlightRt;

#if UNITY_ANDROID
        RenderTexture.active = highlightRT = RenderTexture.GetTemporary(m_RTWidth, m_RTHeight, 0, RenderTextureFormat.ARGB32 );
#else
			RenderTexture.active = highlightRt = RenderTexture.GetTemporary(_rtWidth, _rtHeight, 0, RenderTextureFormat.R8);
#endif
			GL.Clear(true, true, Color.clear);
			RenderTexture.active = null;

			ClearCommandBuffers();

			RenderHighlights(highlightRt);

#if UNITY_ANDROID
        RenderTexture.active = highlightRT = RenderTexture.GetTemporary(m_RTWidth, m_RTHeight, 0, RenderTextureFormat.ARGB32 );
#else
			var blurred = RenderTexture.GetTemporary(_rtWidth, _rtHeight, 0, RenderTextureFormat.R8);
#endif

			_blur.OnRenderImage(highlightRt, blurred);


			RenderOccluders(highlightRt);

			if (RenderFillType == FillType.Outline)
			{
#if UNITY_ANDROID
            RenderTexture.active = highlightRT = RenderTexture.GetTemporary(m_RTWidth, m_RTHeight, 0, RenderTextureFormat.ARGB32 );
#else
				var occluded = RenderTexture.GetTemporary(_rtWidth, _rtHeight, 0, RenderTextureFormat.R8);
#endif
				_highlightMaterial.SetTexture("_OccludeMap", highlightRt);
				Graphics.Blit(blurred, occluded, _highlightMaterial, 2);

				_highlightMaterial.SetTexture("_OccludeMap", occluded);

				RenderTexture.ReleaseTemporary(occluded);

			}
			else
			{
				_highlightMaterial.SetTexture("_OccludeMap", blurred);
			}

			_highlightMaterial.SetColor("_Color", HighlightColour);
			Graphics.Blit(source, destination, _highlightMaterial, (int)SelectedEffect);


			RenderTexture.ReleaseTemporary(blurred);
			RenderTexture.ReleaseTemporary(highlightRt);
		}
	}
}