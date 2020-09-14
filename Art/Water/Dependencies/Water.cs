using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Water
{
	[ExecuteInEditMode] // Make water live-update even when not in play mode
	public class Water : MonoBehaviour
	{
		public enum WaterMode
		{
			Simple = 0,
			Reflective = 1,
			Refractive = 2,
		};
		public WaterMode m_WaterMode = WaterMode.Refractive;
		public bool m_DisablePixelLights = true;
		public int m_TextureSize = 256;
		public float m_ClipPlaneOffset = 0.07f;

		public bool m_TurnOffWaterOcclusion = true;

		public LayerMask m_ReflectLayers = -1;
		public LayerMask m_RefractLayers = -1;

		private Dictionary<Camera, Camera> m_ReflectionCameras = new Dictionary<Camera, Camera>(); // Camera -> Camera table
		private Dictionary<Camera, Camera> m_RefractionCameras = new Dictionary<Camera, Camera>(); // Camera -> Camera table

		private RenderTexture m_ReflectionTexture = null;
		private RenderTexture m_RefractionTexture = null;
		private WaterMode m_HardwareWaterSupport = WaterMode.Refractive;
		private int m_OldReflectionTextureSize = 0;
		//private int m_OldRefractionTextureSize = 0;

		private static bool s_InsideWater = false;

		private Renderer rendererCached;

		private int shIdWaveSpeed = Shader.PropertyToID("WaveSpeed");
		private int shId_WaveScale = Shader.PropertyToID("_WaveScale");
		private int shId_WaveOffset = Shader.PropertyToID("_WaveOffset");
		private int shId_WaveScale4 = Shader.PropertyToID("_WaveScale4");
		private int shId_WaveMatrix = Shader.PropertyToID("_WaveMatrix");
		private int shId_WaveMatrix2 = Shader.PropertyToID("_WaveMatrix2");



		new private Renderer renderer
		{
			get
			{
				if ( rendererCached == null )
					rendererCached = GetComponent<Renderer>();
				return rendererCached;
			}
		}

		private Material matCached;
		private Material mat
		{
			get
			{
				if ( matCached == null )
					matCached = renderer.sharedMaterial;
				return matCached;
			}
		}

		// This is called when it's known that the object will be rendered by some
		// camera. We render reflections / refractions and do other updates here.
		// Because the script executes in edit mode, reflections for the scene view
		// camera will just work!
		public void OnWillRenderObject()
		{
			if ( !enabled || !renderer || !renderer.sharedMaterial || !renderer.enabled )
				return;

			Camera cam = Camera.current;
			if ( !cam || !cam.CompareTag( "MainCamera" ) )
				return;

			// Safeguard from recursive water reflections.      
			if ( s_InsideWater )
				return;
			s_InsideWater = true;

			// Actual water rendering mode depends on both the current setting AND
			// the hardware support. There's no point in rendering refraction textures
			// if they won't be visible in the end.
			m_HardwareWaterSupport = FindHardwareWaterSupport();
			WaterMode mode = GetWaterMode();

			Camera reflectionCamera;//, refractionCamera;
			CreateWaterObjects( cam, out reflectionCamera );//, out refractionCamera );

			// find out the reflection plane: position and normal in world space
			Vector3 pos = transform.position;
			Vector3 normal = transform.up;

			//avoid frustum error ugh
			//avoid frustum error ugh
			//bool frustumError = false;
			//int zeroVectors = 0;
			//var camRotation = cam.transform.rotation;
			//if ( camRotation.x == 0 )
			//	zeroVectors++;
			//if ( camRotation.y == 0 )
			//	zeroVectors++;
			//if ( camRotation.z == 0 )
			//	zeroVectors++;
			//if ( zeroVectors > 1 )
			//{
			//	frustumError = true;
			//}

			// Optionally disable pixel lights for reflection/refraction
			int oldPixelLightCount = QualitySettings.pixelLightCount;
			if ( m_DisablePixelLights )
				QualitySettings.pixelLightCount = 0;

			UpdateCameraModes( cam, reflectionCamera );
			//UpdateCameraModes( cam, refractionCamera );

			// Render reflection if needed
			if ( mode >= WaterMode.Reflective )// && !frustumError )
			{
				// Reflect camera around reflection plane
				float d = -Vector3.Dot(normal, pos) - m_ClipPlaneOffset;
				Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

				Matrix4x4 reflection = Matrix4x4.zero;
				CalculateReflectionMatrix( ref reflection, reflectionPlane );
				Vector3 oldpos = cam.transform.position;
				//Debug.Log( $"oldpos: {oldpos}" );
				Vector3 newpos = reflection.MultiplyPoint(oldpos);
				//Debug.Log( $"newpos: {newpos}" );
				reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;
				// Setup oblique projection matrix so that near plane is our reflection

				//			reflectionCamera.worldToCameraMatrix = new Matrix4x4(
				//			//-0.53887f,    0.17386f,		0.82425f,		1.63841f,
				//			//0.71059f,		-0.43168f,   0.55562f,		-10.83935f,
				//			//0.45242f,		0.88511f,		0.10907f,		21.07603f,
				//			//0.00000f		0.00000f		0.00000f		1.00000f,
				//			new Vector4( -0.53887f,
				//						 0.71059f,
				//						 0.45242f,
				//						 0.00000f ),

				//	new Vector4( 0.17386f,
				//				-0.43168f,
				//				0.88511f,
				//				0.00000f ),

				//				 new Vector4( 0.82425f,
				//							  0.55562f,
				//								 0.10907f,
				//								 0.00000f ),
				//new Vector4( 1.63841f,
				//			-10.83935f,
				//			 	21.07603f,
				//			 	1.00000f ) );
				// plane. This way we clip everything below/above it for free.
				Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
				//clipPlane = new Vector4( 0.0f, -1.0f, -0.3f, -15.5f );
				//Debug.Log( $"clipPlane: {clipPlane}" );
				reflectionCamera.projectionMatrix = cam.CalculateObliqueMatrix( clipPlane );




				//Matrix4x4 m = cam.CalculateObliqueMatrix( clipPlane );
				//reflectionCamera.projectionMatrix = m;
				//Debug.Log( $"cam.CalculateObliqueMatrix( clipPlane ): {cam.CalculateObliqueMatrix( clipPlane )}" );
				//reflectionCamera.projectionMatrix = new Matrix4x4(
				//new Vector4( 0.63938f,  0.00000f,	0.00000f,		0.00000f	),
				//new Vector4( 0.00000f, 1.73205f,	0.00000f,		0.00000f ),
				//new Vector4( -0.03392f, - 2.21662f,    0.33250f, -35.86786f		),
				//new Vector4( 0.00000f, 0.00000f,	- 1.00000f,    0.00000f		) );
				//new Vector4( 0.6393847f,
				//			 0.00000f,
				//			 -0.3562191f,
				//			 0.00000f ),

				//new Vector4( 0.00000f,
				//			 1.732051f,
				//			 1.846399f,
				//			 0.00000f ),

				//new Vector4( 0.00000f,
				//			 0.00000f,
				//			 -2.62263f,
				//			 -1.00000f ),
				//new Vector4( 0.00000f,
				//			0.00000f,
				//			58.63717f,
				//			0.00000f ) );
				//0.56257 0.00000 0.00000 0.00000
				//0.00000 1.00000 0.00000 0.00000
				//- 0.60771    3.14996 - 5.18022    100.03530
				//0.00000 0.00000 - 1.00000    0.00000
				//Debug.Log( $"reflectionCamera.projectionMatrix 1 z: {reflectionCamera.projectionMatrix.GetColumn(1).z}" );
				//Debug.Log( $"reflectionCamera.projectionMatrix y : {reflectionCamera.projectionMatrix.GetColumn( 2 ).z}" );
				//Debug.Log( $"reflectionCamera.projectionMatrix z: {reflectionCamera.projectionMatrix.GetColumn( 3 ).z}" );
				//Debug.Log( $"reflectionCamera.projectionMatrix w: {reflectionCamera.projectionMatrix.GetColumn( 3 ).w}" );
				//				0.56257		0.00000		0.00000		0.00000
				//				0.00000		1.00000		0.00000		0.00000
				//				-0.60771    3.14996		-5.18022    100.03530
				//				0.00000		0.00000		-1.00000    0.00000

				//new Vector4( 0.00000f, 1.73205f, 0.00000f, 0.00000f ),
				//new Vector4( -0.03392f, -2.21662f, 0.33250f, -35.86786f ),
				//new Vector4( 0.00000f, 0.00000f, -1.00000f, 0.00000f ) );

				reflectionCamera.cullingMask = ~( 1 << 4 ) & m_ReflectLayers.value; // never render water layer
				reflectionCamera.targetTexture = m_ReflectionTexture;
				//GL.SetRevertBackfacing(true);
				GL.invertCulling = true;
				reflectionCamera.transform.position = newpos;
				//reflectionCamera.transform.position = new Vector3( 92.3f, -24.1f, 47.0f);
				Vector3 euler = cam.transform.eulerAngles;
				reflectionCamera.transform.eulerAngles = new Vector3( -euler.x, euler.y, euler.z );
				//Debug.Log( $"reflectionCamera.projectionMatrix: {reflectionCamera.projectionMatrix}" );
				//Debug.Log( $"reflectionCamera.worldToCameraMatrix: {reflectionCamera.worldToCameraMatrix}" );
				//x,y: 298, 237
				reflectionCamera.Render();
				reflectionCamera.transform.position = oldpos;
				//GL.SetRevertBackfacing(false);
				GL.invertCulling = false;
				renderer.sharedMaterial.SetTexture( "_ReflectionTex", m_ReflectionTexture );
			}

			// Render refraction
			//if ( mode >= WaterMode.Refractive )// && !frustumError )
			//{
			//	refractionCamera.worldToCameraMatrix = cam.worldToCameraMatrix;

			//	// Setup oblique projection matrix so that near plane is our reflection
			//	// plane. This way we clip everything below/above it for free.
			//	Vector4 clipPlane = CameraSpacePlane(refractionCamera, pos, normal, -1.0f);
			//	refractionCamera.projectionMatrix = cam.CalculateObliqueMatrix( clipPlane );

			//	refractionCamera.cullingMask = ~( 1 << 4 ) & m_RefractLayers.value; // never render water layer
			//	refractionCamera.targetTexture = m_RefractionTexture;
			//	refractionCamera.transform.position = cam.transform.position;
			//	refractionCamera.transform.rotation = cam.transform.rotation;
			//	refractionCamera.Render();
			//	renderer.sharedMaterial.SetTexture( "_RefractionTex", m_RefractionTexture );
			//}

			// Restore pixel light count
			if ( m_DisablePixelLights )
				QualitySettings.pixelLightCount = oldPixelLightCount;

			// Setup shader keywords based on water mode
			switch ( mode )
			{
				case WaterMode.Simple:
					Shader.EnableKeyword( "WATER_SIMPLE" );
					Shader.DisableKeyword( "WATER_REFLECTIVE" );
					Shader.DisableKeyword( "WATER_REFRACTIVE" );
					break;
				case WaterMode.Reflective:
					Shader.DisableKeyword( "WATER_SIMPLE" );
					Shader.EnableKeyword( "WATER_REFLECTIVE" );
					Shader.DisableKeyword( "WATER_REFRACTIVE" );
					break;
				case WaterMode.Refractive:
					Shader.DisableKeyword( "WATER_SIMPLE" );
					Shader.DisableKeyword( "WATER_REFLECTIVE" );
					Shader.EnableKeyword( "WATER_REFRACTIVE" );
					break;
			}

			s_InsideWater = false;
		}


		// Cleanup all the objects we possibly have created
		void OnDisable()
		{
			if ( m_ReflectionTexture )
			{
				DestroyImmediate( m_ReflectionTexture );
				m_ReflectionTexture = null;
			}
			if ( m_RefractionTexture )
			{
				DestroyImmediate( m_RefractionTexture );
				m_RefractionTexture = null;
			}
			foreach ( KeyValuePair<Camera, Camera> kvp in m_ReflectionCameras )
				DestroyImmediate( ( kvp.Value ).gameObject );
			m_ReflectionCameras.Clear();
			foreach ( KeyValuePair<Camera, Camera> kvp in m_RefractionCameras )
				DestroyImmediate( ( kvp.Value ).gameObject );
			m_RefractionCameras.Clear();
		}


		// This just sets up some matrices in the material; for really
		// old cards to make water texture scroll.
		void Update()
		{
			if ( !renderer || !mat )
				return;

			Vector4 waveSpeed = mat.GetVector(shIdWaveSpeed);
			float waveScale = mat.GetFloat(shId_WaveScale);
			Vector4 waveScale4 = new Vector4(waveScale, waveScale, waveScale * 0.4f, waveScale * 0.45f);

			// Time since level load, and do intermediate calculations with doubles
			double t = Time.timeSinceLevelLoad / 20.0;
			Vector4 offsetClamped = new Vector4(
				(float)System.Math.IEEERemainder(waveSpeed.x * waveScale4.x * t, 1.0),
				(float)System.Math.IEEERemainder(waveSpeed.y * waveScale4.y * t, 1.0),
				(float)System.Math.IEEERemainder(waveSpeed.z * waveScale4.z * t, 1.0),
				(float)System.Math.IEEERemainder(waveSpeed.w * waveScale4.w * t, 1.0)
			);

			mat.SetVector( shId_WaveOffset, offsetClamped );
			mat.SetVector( shId_WaveScale4, waveScale4 );

			Vector3 waterSize = renderer.bounds.size;
			Vector3 scale = new Vector3(waterSize.x * waveScale4.x, waterSize.z * waveScale4.y, 1);
			Matrix4x4 scrollMatrix = Matrix4x4.TRS(new Vector3(offsetClamped.x, offsetClamped.y, 0), Quaternion.identity, scale);
			mat.SetMatrix( shId_WaveMatrix, scrollMatrix );

			scale = new Vector3( waterSize.x * waveScale4.z, waterSize.z * waveScale4.w, 1 );
			scrollMatrix = Matrix4x4.TRS( new Vector3( offsetClamped.z, offsetClamped.w, 0 ), Quaternion.identity, scale );
			mat.SetMatrix( shId_WaveMatrix2, scrollMatrix );
		}

		private void UpdateCameraModes( Camera src, Camera dest )
		{
			if ( dest == null )
				return;
			// set water camera to clear the same way as current camera
			dest.clearFlags = src.clearFlags;
			dest.backgroundColor = src.backgroundColor;
			if ( src.clearFlags == CameraClearFlags.Skybox )
			{
				Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
				Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
				if ( !sky || !sky.material )
				{
					mysky.enabled = false;
				}
				else
				{
					mysky.enabled = true;
					mysky.material = sky.material;
				}
			}
			// update other values to match current camera.
			// even if we are supplying custom camera&projection matrices,
			// some of values are used elsewhere (e.g. skybox uses far plane)
			dest.nearClipPlane = src.nearClipPlane;
			dest.nearClipPlane = 1f;
			dest.orthographic = src.orthographic;
			dest.fieldOfView = src.fieldOfView;
			dest.aspect = src.aspect;
			dest.orthographicSize = src.orthographicSize;
		}
		// On-demand create any objects we need for water
		private void CreateWaterObjects( Camera currentCamera, out Camera reflectionCamera )
		{
			WaterMode mode = GetWaterMode();

			reflectionCamera = null;

			if ( mode >= WaterMode.Reflective )
			{
				// Reflection render texture
				if ( !m_ReflectionTexture || m_OldReflectionTextureSize != m_TextureSize )
				{
					if ( m_ReflectionTexture )
						DestroyImmediate( m_ReflectionTexture );
					m_ReflectionTexture = new RenderTexture( m_TextureSize, m_TextureSize, 16 );
					m_ReflectionTexture.name = "__WaterReflection" + GetInstanceID();
					m_ReflectionTexture.isPowerOfTwo = true;
					m_ReflectionTexture.hideFlags = HideFlags.DontSave;
					m_OldReflectionTextureSize = m_TextureSize;

				}

				// Camera for reflection
				m_ReflectionCameras.TryGetValue( currentCamera, out reflectionCamera );
				if ( !reflectionCamera ) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
				{
					GameObject go = new GameObject("Water Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
					reflectionCamera = go.GetComponent<Camera>();
					reflectionCamera.enabled = false;
					reflectionCamera.transform.position = transform.position;
					reflectionCamera.transform.rotation = transform.rotation;
					reflectionCamera.gameObject.AddComponent<FlareLayer>();
					go.hideFlags = HideFlags.DontSave;
					m_ReflectionCameras[currentCamera] = reflectionCamera;

					if ( m_TurnOffWaterOcclusion )
						reflectionCamera.useOcclusionCulling = false;
				}
			}
		}

		// On-demand create any objects we need for water
		//private void CreateWaterObjects( Camera currentCamera, out Camera reflectionCamera, out Camera refractionCamera )
		//{
		//	WaterMode mode = GetWaterMode();

		//	reflectionCamera = null;
		//	refractionCamera = null;

		//	if ( mode >= WaterMode.Reflective )
		//	{
		//		// Reflection render texture
		//		if ( !m_ReflectionTexture || m_OldReflectionTextureSize != m_TextureSize )
		//		{
		//			if ( m_ReflectionTexture )
		//				DestroyImmediate( m_ReflectionTexture );
		//			m_ReflectionTexture = new RenderTexture( m_TextureSize, m_TextureSize, 16 );
		//			m_ReflectionTexture.name = "__WaterReflection" + GetInstanceID();
		//			m_ReflectionTexture.isPowerOfTwo = true;
		//			m_ReflectionTexture.hideFlags = HideFlags.DontSave;
		//			m_OldReflectionTextureSize = m_TextureSize;

		//		}

		//		// Camera for reflection
		//		m_ReflectionCameras.TryGetValue( currentCamera, out reflectionCamera );
		//		if ( !reflectionCamera ) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
		//		{
		//			GameObject go = new GameObject("Water Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
		//			reflectionCamera = go.GetComponent<Camera>();
		//			reflectionCamera.enabled = false;
		//			reflectionCamera.transform.position = transform.position;
		//			reflectionCamera.transform.rotation = transform.rotation;
		//			reflectionCamera.gameObject.AddComponent<FlareLayer>();
		//			go.hideFlags = HideFlags.DontSave;
		//			m_ReflectionCameras[currentCamera] = reflectionCamera;

		//			if ( m_TurnOffWaterOcclusion )
		//				reflectionCamera.useOcclusionCulling = false;
		//		}
		//	}

		//	if ( mode >= WaterMode.Refractive )
		//	{
		//		// Refraction render texture
		//		if ( !m_RefractionTexture || m_OldRefractionTextureSize != m_TextureSize )
		//		{
		//			if ( m_RefractionTexture )
		//				DestroyImmediate( m_RefractionTexture );
		//			m_RefractionTexture = new RenderTexture( m_TextureSize, m_TextureSize, 16 );
		//			m_RefractionTexture.name = "__WaterRefraction" + GetInstanceID();
		//			m_RefractionTexture.isPowerOfTwo = true;
		//			m_RefractionTexture.hideFlags = HideFlags.DontSave;
		//			m_OldRefractionTextureSize = m_TextureSize;
		//		}

		//		// Camera for refraction
		//		m_RefractionCameras.TryGetValue( currentCamera, out refractionCamera );
		//		if ( !refractionCamera ) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
		//		{
		//			GameObject go = new GameObject("Water Refr Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
		//			refractionCamera = go.GetComponent<Camera>();
		//			refractionCamera.enabled = false;
		//			refractionCamera.transform.position = transform.position;
		//			refractionCamera.transform.rotation = transform.rotation;
		//			refractionCamera.gameObject.AddComponent<FlareLayer>();
		//			go.hideFlags = HideFlags.DontSave;
		//			m_RefractionCameras[currentCamera] = refractionCamera;

		//			if ( m_TurnOffWaterOcclusion )
		//				refractionCamera.useOcclusionCulling = false;
		//		}
		//	}
		//}

		private WaterMode GetWaterMode()
		{
			if ( m_HardwareWaterSupport < m_WaterMode )
				return m_HardwareWaterSupport;
			else
				return m_WaterMode;
		}

		private WaterMode FindHardwareWaterSupport()
		{
			if ( !renderer )
				return WaterMode.Simple;

			Material mat = renderer.sharedMaterial;
			if ( !mat )
				return WaterMode.Simple;

			string mode = mat.GetTag("WATERMODE", false);
			if ( mode == "Refractive" )
				return WaterMode.Refractive;
			if ( mode == "Reflective" )
				return WaterMode.Reflective;

			return WaterMode.Simple;
		}

		// Extended sign: returns -1, 0 or 1 based on sign of a
		private static float sgn( float a )
		{
			if ( a > 0.0f ) return 1.0f;
			if ( a < 0.0f ) return -1.0f;
			return 0.0f;
		}

		// Given position/normal of the plane, calculates plane in camera space.
		private Vector4 CameraSpacePlane( Camera cam, Vector3 pos, Vector3 normal, float sideSign )
		{
			Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
			Matrix4x4 m = cam.worldToCameraMatrix;
			Vector3 cpos = m.MultiplyPoint(offsetPos);
			Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
			return new Vector4( cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot( cpos, cnormal ) );
		}

		// Calculates reflection matrix around the given plane
		private static void CalculateReflectionMatrix( ref Matrix4x4 reflectionMat, Vector4 plane )
		{
			reflectionMat.m00 = ( 1F - 2F * plane[0] * plane[0] );
			reflectionMat.m01 = ( -2F * plane[0] * plane[1] );
			reflectionMat.m02 = ( -2F * plane[0] * plane[2] );
			reflectionMat.m03 = ( -2F * plane[3] * plane[0] );

			reflectionMat.m10 = ( -2F * plane[1] * plane[0] );
			reflectionMat.m11 = ( 1F - 2F * plane[1] * plane[1] );
			reflectionMat.m12 = ( -2F * plane[1] * plane[2] );
			reflectionMat.m13 = ( -2F * plane[3] * plane[1] );

			reflectionMat.m20 = ( -2F * plane[2] * plane[0] );
			reflectionMat.m21 = ( -2F * plane[2] * plane[1] );
			reflectionMat.m22 = ( 1F - 2F * plane[2] * plane[2] );
			reflectionMat.m23 = ( -2F * plane[3] * plane[2] );

			reflectionMat.m30 = 0F;
			reflectionMat.m31 = 0F;
			reflectionMat.m32 = 0F;
			reflectionMat.m33 = 1F;
		}

	}
}
