using System;
using UnityEngine;
using Seb.GPUSorting;
using Unity.Mathematics;
using System.Collections.Generic;
using Seb.Helpers;
using static Seb.Helpers.ComputeHelper;
using System.Runtime.InteropServices;

namespace Seb.Fluid.Simulation
{
	public class FluidSim : MonoBehaviour
	{
		public event Action<FluidSim> SimulationInitCompleted;

		[Header("Time Step")] public float normalTimeScale = 1;
		public float slowTimeScale = 0.1f;
		public float maxTimestepFPS = 60; // if time-step dips lower than this fps, simulation will run slower (set to 0 to disable)
		public int iterationsPerFrame = 3;

		[Header("Simulation Settings")] public float gravity = -10;
		public float smoothingRadius = 0.2f;
		public float targetDensity = 630;
		public float pressureMultiplier = 288;
		public float nearPressureMultiplier = 2.15f;
		public float viscosityStrength = 0;
		[Range(0, 1)] public float collisionDamping = 0.95f;

		[Header("Foam Settings")] public bool foamActive;
		public int maxFoamParticleCount = 1000;
		public float trappedAirSpawnRate = 70;
		public float spawnRateFadeInTime = 0.5f;
		public float spawnRateFadeStartTime = 0;
		public Vector2 trappedAirVelocityMinMax = new(5, 25);
		public Vector2 foamKineticEnergyMinMax = new(15, 80);
		public float bubbleBuoyancy = 1.5f;
		public int sprayClassifyMaxNeighbours = 5;
		public int bubbleClassifyMinNeighbours = 15;
		public float bubbleScale = 0.5f;
		public float bubbleChangeScaleSpeed = 7;

		[Header("Water Color Settings")]
		//public Color currentWaterColour = Color.blue;
		public Color waterColourA = Color.blue;
		//public Color waterColourB = Color.red;
		//[Range(0, 1)] public float mixAmount;
		public float colourMixSpeed = 1f;

		[Header("Simple Global Mixing")]
		public Color mixedWaterColour = new Color(0.5f, 0f, 0.8f, 1f);
		public float globalMixSpeed = 0.5f;

		public Color currentMarchingCubesColour;

		bool watersAreMixing = false;
		float globalMixAmount = 0f;

		[Header("Auto Detect Mixing")]
		public bool autoDetectWaterTouching = true;
		public int mixCheckEveryFrames = 10;
		public float purpleDetectionThreshold = 0.15f;

		float4[] colourReadback;


		[Header("Object Interaction")]
		public int maxCollisionBoxes = 8;
		public int maxCollisionSpheres = 8;
		public int maxCollisionCapsules = 8;
		[Range(0, 1)] public float objectCollisionDamping = 0.5f;
		public string ignoreWaterTag = "IgnoreWater";
		//[HideInInspector] public RenderTexture ColourMap3D;

		List<Collider> collisionColliders = new List<Collider>();
		private List<Collider> ignoredWaterColliders = new List<Collider>();
		ComputeBuffer collisionBoxBuffer;
		ComputeBuffer collisionSphereBuffer;
		ComputeBuffer collisionCapsuleBuffer;


		[StructLayout(LayoutKind.Sequential)]
		struct CollisionBoxData
		{
			public Matrix4x4 worldToLocal;
			public Matrix4x4 localToWorld;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct CollisionSphereData
		{
			public Vector4 centerRadius;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct CollisionCapsuleData
		{
			public Vector4 pointARadius;
			public Vector4 pointB;
		}

		[Header("Volumetric Render Settings")] public bool renderToTex3D;
		public int densityTextureRes;

		[Header("References")] public ComputeShader compute;
		public Spawner3D spawner;

		[HideInInspector] public RenderTexture DensityMap;
		public Vector3 Scale => transform.localScale;

		// Buffers
		public ComputeBuffer foamBuffer { get; private set; }
		public ComputeBuffer foamSortTargetBuffer { get; private set; }
		public ComputeBuffer foamCountBuffer { get; private set; }
		public ComputeBuffer positionBuffer { get; private set; }
		public ComputeBuffer velocityBuffer { get; private set; }
		public ComputeBuffer densityBuffer { get; private set; }
		public ComputeBuffer predictedPositionsBuffer;
		public ComputeBuffer debugBuffer { get; private set; }
		public ComputeBuffer colourBuffer { get; private set; }
		ComputeBuffer sortTarget_colourBuffer;

		ComputeBuffer sortTarget_positionBuffer;
		ComputeBuffer sortTarget_velocityBuffer;
		ComputeBuffer sortTarget_predictedPositionsBuffer;

		// Kernel IDs
		int externalForcesKernel;
		int spatialHashKernel;
		int reorderKernel;
		int reorderCopybackKernel;
		int densityKernel;
		int pressureKernel;
		int viscosityKernel;
		int updatePositionsKernel;
		int renderKernel;
		int foamUpdateKernel;
		int foamReorderCopyBackKernel;

		SpatialHash spatialHash;

		// State
		bool isPaused;
		bool pauseNextFrame;
		float smoothRadiusOld;
		float simTimer;
		bool inSlowMode;
		Spawner3D.SpawnData spawnData;
		Dictionary<ComputeBuffer, string> bufferNameLookup;

		void Start()
		{
			Debug.Log("Controls: Space = Play/Pause, Q = SlowMode, R = Reset");
			isPaused = false;

			Initialize();
		}

		void Initialize()
		{
			currentMarchingCubesColour = waterColourA;

            externalForcesKernel = compute.FindKernel("ExternalForces");
            spatialHashKernel = compute.FindKernel("UpdateSpatialHash");
            reorderKernel = compute.FindKernel("Reorder");
            reorderCopybackKernel = compute.FindKernel("ReorderCopyBack");
            densityKernel = compute.FindKernel("CalculateDensities");
            pressureKernel = compute.FindKernel("CalculatePressureForce");
            viscosityKernel = compute.FindKernel("CalculateViscosity");
            updatePositionsKernel = compute.FindKernel("UpdatePositions");
            renderKernel = compute.FindKernel("UpdateDensityTexture");
            foamUpdateKernel = compute.FindKernel("UpdateWhiteParticles");
            foamReorderCopyBackKernel = compute.FindKernel("WhiteParticlePrepareNextFrame");
            
			spawnData = spawner.GetSpawnData();
			int numParticles = spawnData.points.Length;

			spatialHash = new SpatialHash(numParticles);
			
			// Create buffers
			positionBuffer = CreateStructuredBuffer<float3>(numParticles);
			predictedPositionsBuffer = CreateStructuredBuffer<float3>(numParticles);
			velocityBuffer = CreateStructuredBuffer<float3>(numParticles);
			densityBuffer = CreateStructuredBuffer<float2>(numParticles);
			foamBuffer = CreateStructuredBuffer<FoamParticle>(maxFoamParticleCount);
			foamSortTargetBuffer = CreateStructuredBuffer<FoamParticle>(maxFoamParticleCount);
			foamCountBuffer = CreateStructuredBuffer<uint>(4096);
			debugBuffer = CreateStructuredBuffer<float3>(numParticles);

			sortTarget_positionBuffer = CreateStructuredBuffer<float3>(numParticles);
			sortTarget_predictedPositionsBuffer = CreateStructuredBuffer<float3>(numParticles);
			sortTarget_velocityBuffer = CreateStructuredBuffer<float3>(numParticles);

			colourBuffer = CreateStructuredBuffer<float4>(numParticles);
			sortTarget_colourBuffer = CreateStructuredBuffer<float4>(numParticles);

			bufferNameLookup = new Dictionary<ComputeBuffer, string>
			{
				{ positionBuffer, "Positions" },
				{ predictedPositionsBuffer, "PredictedPositions" },
				{ velocityBuffer, "Velocities" },
				{ densityBuffer, "Densities" },
				{ spatialHash.SpatialKeys, "SpatialKeys" },
				{ spatialHash.SpatialOffsets, "SpatialOffsets" },
				{ spatialHash.SpatialIndices, "SortedIndices" },
				{ sortTarget_positionBuffer, "SortTarget_Positions" },
				{ sortTarget_predictedPositionsBuffer, "SortTarget_PredictedPositions" },
				{ sortTarget_velocityBuffer, "SortTarget_Velocities" },
				{ foamCountBuffer, "WhiteParticleCounters" },
				{ foamBuffer, "WhiteParticles" },
				{ foamSortTargetBuffer, "WhiteParticlesCompacted" },
				{ debugBuffer, "Debug" },
				{ colourBuffer, "Colours" },
				{ sortTarget_colourBuffer, "SortTarget_Colours" }
			};

			// Set buffer data
			SetInitialBufferData(spawnData);

			// External forces kernel
			SetBuffers(compute, externalForcesKernel, bufferNameLookup, new ComputeBuffer[]
			{
				positionBuffer,
				predictedPositionsBuffer,
				velocityBuffer
			});

			// Spatial hash kernel
			SetBuffers(compute, spatialHashKernel, bufferNameLookup, new ComputeBuffer[]
			{
				spatialHash.SpatialKeys,
				spatialHash.SpatialOffsets,
				predictedPositionsBuffer,
				spatialHash.SpatialIndices
			});

			// Reorder kernel
			SetBuffers(compute, reorderKernel, bufferNameLookup, new ComputeBuffer[]
			{
				positionBuffer,
				sortTarget_positionBuffer,
				predictedPositionsBuffer,
				sortTarget_predictedPositionsBuffer,
				velocityBuffer,
				sortTarget_velocityBuffer,
				colourBuffer,
				sortTarget_colourBuffer,
				spatialHash.SpatialIndices
			});

			// Reorder copyback kernel
			SetBuffers(compute, reorderCopybackKernel, bufferNameLookup, new ComputeBuffer[]
			{
				positionBuffer,
				sortTarget_positionBuffer,
				predictedPositionsBuffer,
				sortTarget_predictedPositionsBuffer,
				velocityBuffer,
				sortTarget_velocityBuffer,
				colourBuffer,
				sortTarget_colourBuffer,
				spatialHash.SpatialIndices
			});

			// Density kernel
			SetBuffers(compute, densityKernel, bufferNameLookup, new ComputeBuffer[]
			{
				predictedPositionsBuffer,
				densityBuffer,
				spatialHash.SpatialKeys,
				spatialHash.SpatialOffsets,
				colourBuffer
			});

			// Pressure kernel
			SetBuffers(compute, pressureKernel, bufferNameLookup, new ComputeBuffer[]
			{
				predictedPositionsBuffer,
				densityBuffer,
				velocityBuffer,
				spatialHash.SpatialKeys,
				spatialHash.SpatialOffsets,
				foamBuffer,
				foamCountBuffer,
				debugBuffer
			});

			// Viscosity kernel
			SetBuffers(compute, viscosityKernel, bufferNameLookup, new ComputeBuffer[]
			{
				predictedPositionsBuffer,
				densityBuffer,
				velocityBuffer,
				spatialHash.SpatialKeys,
				spatialHash.SpatialOffsets
			});

			// Update positions kernel
			SetBuffers(compute, updatePositionsKernel, bufferNameLookup, new ComputeBuffer[]
			{
				positionBuffer,
				velocityBuffer
			});

			// Render to 3d tex kernel
			SetBuffers(compute, renderKernel, bufferNameLookup, new ComputeBuffer[]
			{
				predictedPositionsBuffer,
				densityBuffer,
				spatialHash.SpatialKeys,
				spatialHash.SpatialOffsets,
				colourBuffer
			});

			// Foam update kernel
			SetBuffers(compute, foamUpdateKernel, bufferNameLookup, new ComputeBuffer[]
			{
				foamBuffer,
				foamCountBuffer,
				predictedPositionsBuffer,
				densityBuffer,
				velocityBuffer,
				spatialHash.SpatialKeys,
				spatialHash.SpatialOffsets,
				foamSortTargetBuffer,
				//debugBuffer
			});


			// Foam reorder copyback kernel
			SetBuffers(compute, foamReorderCopyBackKernel, bufferNameLookup, new ComputeBuffer[]
			{
				foamBuffer,
				foamSortTargetBuffer,
				foamCountBuffer,
			});

			compute.SetInt("numParticles", positionBuffer.count);
			compute.SetInt("MaxWhiteParticleCount", maxFoamParticleCount);

			UpdateSmoothingConstants();

			// Run single frame of sim with deltaTime = 0 to initialize density texture
			// (so that display can work even if paused at start)
			if (renderToTex3D)
			{
				RunSimulationFrame(0);
			}

			SimulationInitCompleted?.Invoke(this);
		}

		void Update()
		{
			// Run simulation
			if (!isPaused)
			{
				float maxDeltaTime = maxTimestepFPS > 0 ? 1 / maxTimestepFPS : float.PositiveInfinity; // If framerate dips too low, run the simulation slower than real-time
				float dt = Mathf.Min(Time.deltaTime * ActiveTimeScale, maxDeltaTime);
				RunSimulationFrame(dt);
				if (autoDetectWaterTouching && !watersAreMixing && Time.frameCount % mixCheckEveryFrames == 0)
				{
					CheckIfWatersTouched();
				}
			}

			if (pauseNextFrame)
			{
				isPaused = true;
				pauseNextFrame = false;
			}

			if (watersAreMixing)
			{
				globalMixAmount = Mathf.MoveTowards(globalMixAmount, 1f, Time.deltaTime * globalMixSpeed);
				currentMarchingCubesColour = Color.Lerp(waterColourA, mixedWaterColour, globalMixAmount);
			}
			//HandleInput();
		}

		void RunSimulationFrame(float frameDeltaTime)
		{
			float subStepDeltaTime = frameDeltaTime / iterationsPerFrame;
			UpdateSettings(subStepDeltaTime, frameDeltaTime);

			// Simulation sub-steps
			for (int i = 0; i < iterationsPerFrame; i++)
			{
				simTimer += subStepDeltaTime;
				RunSimulationStep();
			}

			// Foam and spray particles
			if (foamActive)
			{
				Dispatch(compute, maxFoamParticleCount, kernelIndex: foamUpdateKernel);
				Dispatch(compute, maxFoamParticleCount, kernelIndex: foamReorderCopyBackKernel);
			}

			// 3D density map
			if (renderToTex3D)
			{
				UpdateDensityMap();
			}
		}

		void UpdateDensityMap()
		{
			float maxAxis = Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
			int w = Mathf.RoundToInt(transform.localScale.x / maxAxis * densityTextureRes);
			int h = Mathf.RoundToInt(transform.localScale.y / maxAxis * densityTextureRes);
			int d = Mathf.RoundToInt(transform.localScale.z / maxAxis * densityTextureRes);

			CreateRenderTexture3D(
				ref DensityMap,
				w,
				h,
				d,
				UnityEngine.Experimental.Rendering.GraphicsFormat.R16_SFloat,
				TextureWrapMode.Clamp
			);

			compute.SetTexture(renderKernel, "DensityMap", DensityMap);
			compute.SetInts("densityMapSize", DensityMap.width, DensityMap.height, DensityMap.volumeDepth);

			Dispatch(compute, DensityMap.width, DensityMap.height, DensityMap.volumeDepth, renderKernel);
		}

		void RunSimulationStep()
		{
			Dispatch(compute, positionBuffer.count, kernelIndex: externalForcesKernel);

			Dispatch(compute, positionBuffer.count, kernelIndex: spatialHashKernel);
			spatialHash.Run();
			
			Dispatch(compute, positionBuffer.count, kernelIndex: reorderKernel);
			Dispatch(compute, positionBuffer.count, kernelIndex: reorderCopybackKernel);

			Dispatch(compute, positionBuffer.count, kernelIndex: densityKernel);
			Dispatch(compute, positionBuffer.count, kernelIndex: pressureKernel);
			if (viscosityStrength != 0) Dispatch(compute, positionBuffer.count, kernelIndex: viscosityKernel);
			Dispatch(compute, positionBuffer.count, kernelIndex: updatePositionsKernel);
		}

		void UpdateSmoothingConstants()
		{
			float r = smoothingRadius;
			float spikyPow2 = 15 / (2 * Mathf.PI * Mathf.Pow(r, 5));
			float spikyPow3 = 15 / (Mathf.PI * Mathf.Pow(r, 6));
			float spikyPow2Grad = 15 / (Mathf.PI * Mathf.Pow(r, 5));
			float spikyPow3Grad = 45 / (Mathf.PI * Mathf.Pow(r, 6));

			compute.SetFloat("K_SpikyPow2", spikyPow2);
			compute.SetFloat("K_SpikyPow3", spikyPow3);
			compute.SetFloat("K_SpikyPow2Grad", spikyPow2Grad);
			compute.SetFloat("K_SpikyPow3Grad", spikyPow3Grad);
		}

		void UpdateSettings(float stepDeltaTime, float frameDeltaTime)
		{
			if (smoothingRadius != smoothRadiusOld)
			{
				smoothRadiusOld = smoothingRadius;
				UpdateSmoothingConstants();
			}

			Vector3 simBoundsSize = transform.localScale;
			Vector3 simBoundsCentre = transform.position;

			compute.SetFloat("deltaTime", stepDeltaTime);
			compute.SetFloat("whiteParticleDeltaTime", frameDeltaTime);
			compute.SetFloat("simTime", simTimer);
			compute.SetFloat("gravity", gravity);
			compute.SetFloat("collisionDamping", collisionDamping);
			compute.SetFloat("smoothingRadius", smoothingRadius);
			compute.SetFloat("targetDensity", targetDensity);
			compute.SetFloat("pressureMultiplier", pressureMultiplier);
			compute.SetFloat("nearPressureMultiplier", nearPressureMultiplier);
			compute.SetFloat("viscosityStrength", viscosityStrength);
			compute.SetVector("boundsSize", simBoundsSize);
			compute.SetVector("centre", simBoundsCentre);

			compute.SetMatrix("localToWorld", transform.localToWorldMatrix);
			compute.SetMatrix("worldToLocal", transform.worldToLocalMatrix);

			// Foam settings
			float fadeInT = (spawnRateFadeInTime <= 0) ? 1 : Mathf.Clamp01((simTimer - spawnRateFadeStartTime) / spawnRateFadeInTime);
			compute.SetVector("trappedAirParams", new Vector3(trappedAirSpawnRate * fadeInT * fadeInT, trappedAirVelocityMinMax.x, trappedAirVelocityMinMax.y));
			compute.SetVector("kineticEnergyParams", foamKineticEnergyMinMax);
			compute.SetFloat("bubbleBuoyancy", bubbleBuoyancy);
			compute.SetInt("sprayClassifyMaxNeighbours", sprayClassifyMaxNeighbours);
			compute.SetInt("bubbleClassifyMinNeighbours", bubbleClassifyMinNeighbours);
			compute.SetFloat("bubbleScaleChangeSpeed", bubbleChangeScaleSpeed);
			compute.SetFloat("bubbleScale", bubbleScale);

			compute.SetFloat("colourMixSpeed", colourMixSpeed);

			UpdateCollisionObjects();
		}

		void SetInitialBufferData(Spawner3D.SpawnData spawnData)
		{

			float4[] colours = new float4[spawnData.colours.Length];

			for (int i = 0; i < colours.Length; i++)
			{
				Color c = spawnData.colours[i];
				colours[i] = new float4(c.r, c.g, c.b, c.a);
			}

			colourBuffer.SetData(colours);

			positionBuffer.SetData(spawnData.points);
			predictedPositionsBuffer.SetData(spawnData.points);
			velocityBuffer.SetData(spawnData.velocities);

			foamBuffer.SetData(new FoamParticle[foamBuffer.count]);

			debugBuffer.SetData(new float3[debugBuffer.count]);
			foamCountBuffer.SetData(new uint[foamCountBuffer.count]);
			simTimer = 0;
		}

		void HandleInput()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				isPaused = !isPaused;
			}

			if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				isPaused = false;
				pauseNextFrame = true;
			}

			if (Input.GetKeyDown(KeyCode.R))
			{
				pauseNextFrame = true;
				SetInitialBufferData(spawnData);
				// Run single frame of sim with deltaTime = 0 to initialize density texture
				// (so that display can work even if paused at start)
				if (renderToTex3D)
				{
					RunSimulationFrame(0);
				}
			}

			if (Input.GetKeyDown(KeyCode.Q))
			{
				inSlowMode = !inSlowMode;
			}
		}

		private float ActiveTimeScale => inSlowMode ? slowTimeScale : normalTimeScale;

		void OnDestroy()
		{
			foreach (var kvp in bufferNameLookup)
			{
				Release(kvp.Key);
			}

			Release(collisionBoxBuffer);
			Release(collisionSphereBuffer);
			Release(collisionCapsuleBuffer);

			Release(colourBuffer);
			Release(sortTarget_colourBuffer);

			spatialHash.Release();
		}


		public struct FoamParticle
		{
			public float3 position;
			public float3 velocity;
			public float lifetime;
			public float scale;
		}

		void OnDrawGizmos()
		{
			// Draw Bounds
			var m = Gizmos.matrix;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = new Color(0, 1, 0, 0.5f);
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			Gizmos.matrix = m;
		}

		public void AddCollisionCollider(Collider other)
		{
			if (ignoredWaterColliders.Contains(other))
				return;
				
			if (other.CompareTag(ignoreWaterTag))
				return;

			if (!collisionColliders.Contains(other))
				collisionColliders.Add(other);
		}

		public void RemoveCollisionCollider(Collider other)
		{
			collisionColliders.Remove(other);
		}

		public void IgnoreCollider(Collider col)
		{
			if (!ignoredWaterColliders.Contains(col))
				ignoredWaterColliders.Add(col);
		}
		
		public void StartGlobalMixing()
		{
			watersAreMixing = true;
		}

		void CheckIfWatersTouched()
		{
			if (colourBuffer == null)
				return;

			if (colourReadback == null || colourReadback.Length != colourBuffer.count)
				colourReadback = new float4[colourBuffer.count];

			colourBuffer.GetData(colourReadback);

			for (int i = 0; i < colourReadback.Length; i++)
			{
				float r = colourReadback[i].x;
				float b = colourReadback[i].z;

				// Red + blue present = purple/mixed particle
				if (r > purpleDetectionThreshold && b > purpleDetectionThreshold)
				{
					StartGlobalMixing();
					return;
				}
			}
		}

		void OnTriggerEnter(Collider other)
		{
			AddCollisionCollider(other);
		}

		void OnTriggerStay(Collider other)
		{
			AddCollisionCollider(other);
		}

		void OnTriggerExit(Collider other)
		{
			RemoveCollisionCollider(other);
		}

		void UpdateCollisionObjects()
		{
			if (collisionBoxBuffer == null || collisionBoxBuffer.count != maxCollisionBoxes)
			{
				Release(collisionBoxBuffer);
				collisionBoxBuffer = new ComputeBuffer(maxCollisionBoxes, 128);
			}

			if (collisionSphereBuffer == null || collisionSphereBuffer.count != maxCollisionSpheres)
			{
				Release(collisionSphereBuffer);
				collisionSphereBuffer = new ComputeBuffer(maxCollisionSpheres, 16);
			}

			if (collisionCapsuleBuffer == null || collisionCapsuleBuffer.count != maxCollisionCapsules)
			{
				Release(collisionCapsuleBuffer);
				collisionCapsuleBuffer = new ComputeBuffer(maxCollisionCapsules, 32);
			}

			CollisionBoxData[] boxes = new CollisionBoxData[maxCollisionBoxes];
			CollisionSphereData[] spheres = new CollisionSphereData[maxCollisionSpheres];
			CollisionCapsuleData[] capsules = new CollisionCapsuleData[maxCollisionCapsules];

			int boxCount = 0;
			int sphereCount = 0;
			int capsuleCount = 0;

			for (int i = collisionColliders.Count - 1; i >= 0; i--)
			{
				Collider col = collisionColliders[i];

				if (col == null)
				{
					collisionColliders.RemoveAt(i);
					continue;
				}

				if (col is BoxCollider box && boxCount < maxCollisionBoxes)
				{
					Transform t = box.transform;

					Matrix4x4 boxMatrix = Matrix4x4.TRS(
						t.TransformPoint(box.center),
						t.rotation,
						Vector3.Scale(t.lossyScale, box.size)
					);

					boxes[boxCount] = new CollisionBoxData
					{
						worldToLocal = boxMatrix.inverse,
						localToWorld = boxMatrix
					};

					boxCount++;
				}
				else if (col is SphereCollider sphere && sphereCount < maxCollisionSpheres)
				{
					Transform t = sphere.transform;
					Vector3 center = t.TransformPoint(sphere.center);
					float scale = Mathf.Max(Mathf.Abs(t.lossyScale.x), Mathf.Abs(t.lossyScale.y), Mathf.Abs(t.lossyScale.z));
					float radius = sphere.radius * scale;

					spheres[sphereCount] = new CollisionSphereData
					{
						centerRadius = new Vector4(center.x, center.y, center.z, radius)
					};

					sphereCount++;
				}
				else if (col is CapsuleCollider capsule && capsuleCount < maxCollisionCapsules)
				{
					Transform t = capsule.transform;

					Vector3 localAxis = capsule.direction switch
					{
						0 => Vector3.right,
						1 => Vector3.up,
						_ => Vector3.forward
					};

					Vector3 worldCenter = t.TransformPoint(capsule.center);
					Vector3 worldAxis = t.TransformDirection(localAxis).normalized;

					Vector3 scaleAbs = new Vector3(
						Mathf.Abs(t.lossyScale.x),
						Mathf.Abs(t.lossyScale.y),
						Mathf.Abs(t.lossyScale.z)
					);

					float radiusScale;
					float heightScale;

					if (capsule.direction == 0)
					{
						heightScale = scaleAbs.x;
						radiusScale = Mathf.Max(scaleAbs.y, scaleAbs.z);
					}
					else if (capsule.direction == 1)
					{
						heightScale = scaleAbs.y;
						radiusScale = Mathf.Max(scaleAbs.x, scaleAbs.z);
					}
					else
					{
						heightScale = scaleAbs.z;
						radiusScale = Mathf.Max(scaleAbs.x, scaleAbs.y);
					}

					float radius = capsule.radius * radiusScale;
					float height = Mathf.Max(capsule.height * heightScale, radius * 2f);
					float halfSegmentLength = Mathf.Max(0f, height * 0.5f - radius);

					Vector3 pointA = worldCenter + worldAxis * halfSegmentLength;
					Vector3 pointB = worldCenter - worldAxis * halfSegmentLength;

					capsules[capsuleCount] = new CollisionCapsuleData
					{
						pointARadius = new Vector4(pointA.x, pointA.y, pointA.z, radius),
						pointB = new Vector4(pointB.x, pointB.y, pointB.z, 0)
					};

					capsuleCount++;
				}
			}

			collisionBoxBuffer.SetData(boxes);
			collisionSphereBuffer.SetData(spheres);
			collisionCapsuleBuffer.SetData(capsules);

			compute.SetBuffer(updatePositionsKernel, "CollisionBoxes", collisionBoxBuffer);
			compute.SetBuffer(updatePositionsKernel, "CollisionSpheres", collisionSphereBuffer);
			compute.SetBuffer(updatePositionsKernel, "CollisionCapsules", collisionCapsuleBuffer);

			compute.SetInt("numCollisionBoxes", boxCount);
			compute.SetInt("numCollisionSpheres", sphereCount);
			compute.SetInt("numCollisionCapsules", capsuleCount);
			compute.SetFloat("objectCollisionDamping", objectCollisionDamping);
		}
    }
}