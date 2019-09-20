using System;
using UnityEngine;

namespace HighlightPlus 
{
	[ExecuteInEditMode]
	[HelpURL ("http://kronnect.com/taptapgo")]
	public class HighlightEffect : MonoBehaviour 
	{
		public enum SeeThroughMode 
		{
			WhenHighlighted = 0,
			AlwaysWhenOccluded = 1,
			Never = 2
		}

		[Serializable]
		public struct GlowPassData 
		{
			public float offset;
			public float alpha;
			public Color color;
		}

		private struct ModelMaterials 
		{
			public Transform transform;
			public bool bakedTransform;
			public Vector3 currentPosition, currentRotation, currentScale;
			public bool currentRenderIsVisible;
			public Mesh mesh, originalMesh;
			public Renderer renderer;
			public SkinnedMeshRenderer skinnedMeshRenderer;
			public Material fxMatGlow, fxMatOutline;
			public Material[] fxMatSeeThrough, fxMatOverlay;
			public Mesh renderingMesh;
			public Matrix4x4 renderingMatrix;
			public bool cloth;
		}

		public bool previewInEditor;
		public Transform target;
		public bool highlighted;

		[Range (0, 1)]
		public float overlay = 0.5f;
		public Color overlayColor = Color.yellow;
		public float overlayAnimationSpeed = 1f;

		[Range (0, 1)]
		public float outline = 1f;
		public Color outlineColor = Color.black;
		public float outlineWidth = 0.45f;

		[Range (0, 5)]
		public float glow = 1f;
		public float glowWidth = 0.4f;
		public bool glowDithering = true;
		public float glowMagicNumber1 = 0.75f;
		public float glowMagicNumber2 = 0.5f;
		public float glowAnimationSpeed = 1f;
		public GlowPassData[] glowPasses;

		public SeeThroughMode seeThrough;
		[Range (0, 5f)]
		public float seeThroughIntensity = 0.8f;
		[Range (0, 1)]
		public float seeThroughTintAlpha = 0.5f;
		public Color seeThroughTintColor = Color.red;

		[SerializeField][HideInInspector]
		private ModelMaterials[] modelMaterials;

		private static Material fxMatMask;
		private static Material fxMatSeeThrough;
		private static Material fxMatGlow;
		private static Material fxMatOutline;
		private static Material fxMatOverlay;

		private void OnEnable() 
		{
			if (target == null)
			{
				target = transform;
			}
			
			if (glowPasses == null || glowPasses.Length == 0) 
			{
				glowPasses = new GlowPassData[4];
				glowPasses [0] = new GlowPassData () { offset = 4, alpha = 0.1f, color = new Color (0.64f, 1f, 0f, 1f) };
				glowPasses [1] = new GlowPassData () { offset = 3, alpha = 0.2f, color = new Color (0.64f, 1f, 0f, 1f) };
				glowPasses [2] = new GlowPassData () { offset = 2, alpha = 0.3f, color = new Color (0.64f, 1f, 0f, 1f) };
				glowPasses [3] = new GlowPassData () { offset = 1, alpha = 0.4f, color = new Color (0.64f, 1f, 0f, 1f) };
			}

			InitMaterial(ref fxMatMask, "HighlightPlus/Geometry/Mask");
			InitMaterial(ref fxMatSeeThrough, "HighlightPlus/Geometry/SeeThrough");
			InitMaterial (ref fxMatGlow, "HighlightPlus/Geometry/Glow");
			InitMaterial(ref fxMatOutline, "HighlightPlus/Geometry/Outline");
			InitMaterial(ref fxMatOverlay, "HighlightPlus/Geometry/Overlay");

			SetupMaterial();
		}

		private void OnDisable() 
		{
			try
			{
				UpdateMaterialProperties();
			}
			catch 
			{
				// ignored
			}
		}

		public void Refresh() 
		{
			if (!enabled) 
			{
				enabled = true;
			} 
			else
			{
				SetupMaterial();
			}
		}
		
		private void OnRenderObject () 
		{
			
#if UNITY_EDITOR
			if (!previewInEditor && !Application.isPlaying)
			{
				return;
			}
#endif
			bool seeThroughReal = seeThroughIntensity > 0 && (seeThrough == SeeThroughMode.AlwaysWhenOccluded || (seeThrough == SeeThroughMode.WhenHighlighted && highlighted));
			if (!highlighted && !seeThroughReal) 
			{
				return;
			}

			// Ensure renderers are valid and visible (in case LODgroup has changed active renderer)
			for (int i = 0; i < modelMaterials.Length; i++) 
			{
				if (modelMaterials[i].renderer != null && modelMaterials[i].renderer.isVisible != modelMaterials[i].currentRenderIsVisible)
				{
					SetupMaterial();
					break;
				}
			}

			// Apply effect
			float glowReal = highlighted ? glow : 0;

			// First create masks
			for (int i = 0; i < modelMaterials.Length; i++)
			{
				Transform transform = modelMaterials[i].transform;
				if (transform == null)
				{
					continue;
				}

				Mesh mesh = modelMaterials[i].mesh;		
				Vector3 skinnedMeshLossyScale;
				
				if (modelMaterials[i].skinnedMeshRenderer != null)
				{
					modelMaterials[i].skinnedMeshRenderer.BakeMesh(mesh);
					BakeTransform(i, false);
					skinnedMeshLossyScale = Vector3.one;
				} 
				else 
				{
					skinnedMeshLossyScale = transform.lossyScale;
				}

				modelMaterials[i].renderingMesh = mesh;

				if (mesh == null)
				{
					continue;
				}
				
				Matrix4x4 matrix;				
				if (modelMaterials[i].bakedTransform) 
				{
					if (modelMaterials[i].currentPosition != transform.position || modelMaterials[i].currentRotation != transform.eulerAngles || modelMaterials[i].currentScale != transform.lossyScale)
					{
						BakeTransform(i, true);
					}
					
					matrix = Matrix4x4.identity;
				} 
				else
				{
					matrix = Matrix4x4.TRS(transform.position, transform.rotation, skinnedMeshLossyScale);
				}

				modelMaterials[i].renderingMatrix = matrix;
				fxMatMask.SetPass(0);
				
				for (int l = 0; l < mesh.subMeshCount; l++) 
				{
					Graphics.DrawMeshNow (mesh, matrix, l);
				}
			}

			// Add effects
			for (int i = 0; i < modelMaterials.Length; i++)
			{
				Transform transform = modelMaterials[i].transform;
				if (transform == null)
				{
					continue;
				}

				Mesh mesh = modelMaterials[i].renderingMesh;
				if (mesh == null)
				{
					continue;
				}
				
				Matrix4x4 matrix = modelMaterials[i].renderingMatrix;
				for (int l = 0; l < mesh.subMeshCount; l++) 
				{
					if (seeThroughReal && !modelMaterials[i].cloth) 
					{
						modelMaterials [i].fxMatSeeThrough [l].SetPass(0);
						Graphics.DrawMeshNow (mesh, matrix, l);
					}
					
					if (highlighted) 
					{
						if (glow > 0) 
						{
							for (int j = 0; j < glowPasses.Length; j++) 
							{
								modelMaterials [i].fxMatGlow.SetColor ("_GlowColor", glowPasses [j].color);
								modelMaterials [i].fxMatGlow.SetVector ("_Glow", new Vector4 (glowReal * glowPasses [j].alpha, glowPasses [j].offset * glowWidth / 100f, glowMagicNumber1, glowMagicNumber2));
								modelMaterials [i].fxMatGlow.SetPass (0);
								Graphics.DrawMeshNow (mesh, matrix, l);
							}
						}
						
						if (outline > 0) 
						{
							modelMaterials [i].fxMatOutline.SetPass(0);
							Graphics.DrawMeshNow (mesh, matrix, l);
						}
						
						if (overlay > 0) 
						{
							modelMaterials [i].fxMatOverlay [l].SetPass(0);
							Graphics.DrawMeshNow (mesh, matrix, l);
						}
					}
				}
			}
		}

		private void InitMaterial (ref Material material, string shaderName) 
		{
			if (material == null) 
			{
				Shader shaderFX = Shader.Find (shaderName);
				if (shaderFX == null) 
				{
					Debug.LogError ("Shader " + shaderName + " not found.");
					enabled = false;
					return;
				}
				material = new Material (shaderFX);
			}
		}

		public void SetTarget (Transform transform) 
		{
			if (transform == target || transform == null)
			{
				return;
			}

			if (highlighted)
			{
				SetHighlighted(false);
			}

			target = transform;
			SetupMaterial();
		}

		public void SetHighlighted (bool state) 
		{
			highlighted = state;
			Refresh ();
		}

		private void SetupMaterial() 
		{
			//HACK: хайлайт где-то протекает с пустым таргетом, пока закостылим
			if (!target)
			{
				return;
			}

			Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
			if (modelMaterials == null || modelMaterials.Length < renderers.Length) 
			{
				modelMaterials = new ModelMaterials[renderers.Length];
			}
			
			for (int i = 0; i < renderers.Length; i++)
			{
				Renderer renderer = renderers[i];
				modelMaterials[i] = new ModelMaterials();
				modelMaterials[i].renderer = renderer;

				if (!renderer.isVisible)
				{
					continue;
				}

				if (renderer.transform != target && renderer.GetComponent<HighlightEffect>() != null)
				{
					continue; // independent subobject
				}

				if (renderer is SkinnedMeshRenderer) 
				{
					// ignore cloth skinned renderers
					modelMaterials[i].cloth = renderer.GetComponent<Cloth>() != null;
					SkinnedMeshRenderer smr = (SkinnedMeshRenderer)renderer;
					modelMaterials [i].mesh = new Mesh ();
					modelMaterials [i].skinnedMeshRenderer = smr;
				}
				else if (renderer.gameObject.isStatic) 
				{
					MeshCollider mc = renderer.GetComponent<MeshCollider> ();
					if (mc != null) 
					{
						modelMaterials [i].mesh = mc.sharedMesh;
					}
				}
				
				if (modelMaterials [i].mesh == null) 
				{
					MeshFilter mf = renderer.GetComponent<MeshFilter> ();
					if (mf != null) 
					{
						modelMaterials [i].mesh = mf.sharedMesh;
					}
				}

				modelMaterials[i].transform = renderer.transform;
				modelMaterials [i].fxMatGlow = Instantiate<Material> (fxMatGlow);
				modelMaterials[i].fxMatOutline = Instantiate<Material>(fxMatOutline);
				modelMaterials[i].fxMatSeeThrough = Fork(fxMatSeeThrough, renderer.sharedMaterials);
				modelMaterials[i].fxMatOverlay = Fork(fxMatOverlay, renderer.sharedMaterials);
				modelMaterials[i].originalMesh = modelMaterials[i].mesh;
				modelMaterials[i].currentRenderIsVisible = true;
				
				if (modelMaterials [i].skinnedMeshRenderer == null) 
				{
					// check if scale is negative
					BakeTransform (i, true);
				}
			}

			UpdateMaterialProperties();
		}

		private Material[] Fork (Material mat, Material[] originals) 
		{
			if (originals == null)
			{
				return null;
			}

			Material[] result = new Material[originals.Length];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = Instantiate(mat);
			}
			
			return result;
		}

		private void BakeTransform(int i, bool duplicateMesh) 
		{
			if (modelMaterials[i].mesh == null)
			{
				return;
			}

			Transform t = modelMaterials [i].transform;
			Vector3 scale = t.localScale;
			
			if (scale.x >= 0 && scale.y >= 0 && scale.z >= 0)
			{
				modelMaterials[i].bakedTransform = false;
				return;
			}
			
			// Duplicates mesh and bake rotation
			Mesh fixedMesh = duplicateMesh ? Instantiate<Mesh> (modelMaterials [i].originalMesh) : modelMaterials [i].mesh;
			Vector3[] vertices = fixedMesh.vertices;
			for (int k = 0; k < vertices.Length; k++) 
			{
				vertices [k] = t.TransformPoint (vertices [k]);
			}
			
			fixedMesh.vertices = vertices;
			Vector3[] normals = fixedMesh.normals;
			
			if (normals != null) 
			{
				for (int k = 0; k < normals.Length; k++) 
				{
					normals [k] = t.TransformVector (normals [k]).normalized;
				}
				
				fixedMesh.normals = normals;
			}
			
			fixedMesh.RecalculateBounds ();
			modelMaterials [i].mesh = fixedMesh;
			modelMaterials [i].bakedTransform = true;
			modelMaterials [i].currentPosition = t.position;
			modelMaterials [i].currentRotation = t.eulerAngles;
			modelMaterials [i].currentScale = t.lossyScale;
		}


		private void UpdateMaterialProperties () 
		{
			Color outlineColor = this.outlineColor;
			outlineColor.a = outline;
			
			Color overlayColor = this.overlayColor;
			overlayColor.a = overlay;
			
			Color seeThroughTintColor = this.seeThroughTintColor;
			seeThroughTintColor.a = this.seeThroughTintAlpha;

			for (int i = 0; i < modelMaterials.Length; i++) 
			{
				if (modelMaterials [i].mesh != null) 
				{
					// Setup materials

					// Glow
					Material fxMat = modelMaterials [i].fxMatGlow;
					fxMat.SetVector("_Glow2", new Vector3(outlineWidth / 100f, glowAnimationSpeed, glowDithering ? 0 : 1));

					// Outline
					fxMat = modelMaterials [i].fxMatOutline;
					fxMat.SetColor ("_OutlineColor", outlineColor);
					fxMat.SetFloat ("_OutlineWidth", outlineWidth / 100f);

					// See-through
					for (int smi = 0; smi < modelMaterials[i].mesh.subMeshCount; smi++)
					{
						Material mat = null;
						if (smi < modelMaterials[i].renderer.sharedMaterials.Length)
						{
							mat = modelMaterials[i].renderer.sharedMaterials[smi];
						}
						
						//mat = modelMaterials[i].renderer.sharedMaterials[smi];
						if (mat == null)
						{
							continue;
						}

						fxMat = modelMaterials[i].fxMatSeeThrough[smi];
						if (mat.HasProperty ("_MainTex")) 
						{
							fxMat.mainTexture = mat.mainTexture;
							fxMat.mainTextureOffset = mat.mainTextureOffset;
							fxMat.mainTextureScale = mat.mainTextureScale;
						}
						
						fxMat.SetFloat ("_SeeThrough", seeThroughIntensity);
						fxMat.SetColor ("_SeeThroughTintColor", seeThroughTintColor);

						// Overlay
						fxMat = modelMaterials[i].fxMatOverlay[smi];
						if (mat.HasProperty ("_MainTex")) 
						{
							fxMat.mainTexture = mat.mainTexture;
							fxMat.mainTextureOffset = mat.mainTextureOffset;
							fxMat.mainTextureScale = mat.mainTextureScale;
						}
						
						if (mat.HasProperty ("_Color")) 
						{
							fxMat.SetColor ("_OverlayBackColor", mat.GetColor ("_Color"));
						}
						
						fxMat.color = overlayColor;
						fxMat.SetFloat ("_OverlaySpeed", overlayAnimationSpeed);
					}
				}
			}
		}
	}
}
