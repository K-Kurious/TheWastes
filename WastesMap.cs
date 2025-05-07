using CustomMapLib;
using Il2CppRUMBLE.MoveSystem;
using MelonLoader;
using UnityEngine;
using TheWastes;
using System.Collections;
using static RumbleModdingAPI.Calls.GameObjects.Gym.Logic.HeinhouserProducts.Leaderboard;
using static RumbleModdingAPI.Calls;
using BuildInfo = TheWastes.BuildInfo;
using RenderSettings = UnityEngine.RenderSettings;

[assembly: MelonInfo(typeof(WastesMap), BuildInfo.Name, BuildInfo.Version, BuildInfo.Author, BuildInfo.DownloadLink)]
[assembly: MelonGame(null, null)]

namespace TheWastes
{
    public static class BuildInfo
    {
        public const string Name = "The Wastes";
        public const string Description = "A huge desolate Island";
        public const string Author = "SisterPankake";
        public const string Company = null;
        public const string Version = "1.0.5";
        public const string DownloadLink = null;
    }

    public class WastesMap : Map
    {
        public override void OnLateInitializeMelon() => Initialize(BuildInfo.Name, BuildInfo.Version, BuildInfo.Author);

        private GameObject arena = new GameObject();
        private GameObject net = new GameObject();
        private GameObject spawn = new GameObject();
        private GameObject arena_Floor = new GameObject();
        private GameObject kill_Vis = new GameObject();
        private GameObject islands = new GameObject();
        private GameObject sun = new GameObject();
        private Material skybox = null;
        private bool wasLightmapChanged = false;
        private Texture2D lightmap = null;
        private Il2CppAssetBundle assetBundle = null;
        private GameObject CombatFloorHolder = null;

        public override void OnMapCreation()
        {
            // Load and assign Map and variables
            assetBundle = LoadBundle("TheWastes.Resources.wastes");
            arena = GameObject.Instantiate(assetBundle.LoadAsset<GameObject>("TheWastes"));
            net = arena.transform.GetChild(0).gameObject;
            spawn = arena.transform.GetChild(1).gameObject;
            kill_Vis = arena.transform.GetChild(2).gameObject;
            islands = arena.transform.GetChild(3).gameObject;
            sun = arena.transform.GetChild(4).gameObject;
            CombatFloorHolder = arena.transform.Find("Colliders/CombatFloor").gameObject;
            if (CombatFloorHolder == null)
            {
                MelonLogger.Error("Failed to find Combat Floor");
            }

            arena.transform.SetParent(mapParent.transform);

            // Setup SkyBox variable
            skybox = assetBundle.LoadAsset<Material>("Desert_Sky");

            // Set Spawn Locations
            HostPedestal.SetFirstSequence(spawn.transform.GetChild(0).position);
            HostPedestal.SetSecondSequence(spawn.transform.GetChild(1).position);
            ClientPedestal.SetFirstSequence(spawn.transform.GetChild(2).position);
            ClientPedestal.SetSecondSequence(spawn.transform.GetChild(3).position);

            // Set Layers and GroundColliders 
            SetupLayers(CombatFloorHolder, ObjectType.CombatFloor);
        }

        public override void OnMapMatchLoad(bool amHost)
        {
            // Assign SkyBox
            MelonLogger.Msg("Try with Just skybox = skybox");
            RenderSettings.skybox = skybox;
            //RenderSettings.skybox.SetTexture(skybox.GetTexture());
            GameObject directionalLight = GameObject.Find("Directional Light");
            if(directionalLight != null)
            {
                directionalLight.transform.position = sun.transform.position;
                directionalLight.transform.rotation = sun.transform.rotation;
                MelonLogger.Msg("Found Directional Light in scene");
            }
            else
            {
                MelonLogger.Error("Failed to find Directional Light in scene");
            }

        }

        public Il2CppAssetBundle LoadBundle(string path)
        {
#pragma warning disable CS8600, CS8602 // Converting null literal or possible null value to non-nullable type.
            using (System.IO.Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream(path))
            {
                if (bundleStream == null)
                {
                    MelonLogger.Error("Failed to find resource stream!");
                    return null;
                }

                byte[] bundleBytes = new byte[bundleStream.Length];
                bundleStream.Read(bundleBytes, 0, bundleBytes.Length);
                return Il2CppAssetBundleManager.LoadFromMemory(bundleBytes);
            }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        private void SetupLayers(GameObject parent, ObjectType type)
        {
            if (parent == null)
            {
                MelonLogger.Error($"Passed a null parent: {parent.name}");
                return;
            }

            if (parent.transform.childCount > 0)
            {
                MelonLogger.Msg($"Parent: {parent.name} has {parent.transform.childCount} children");
                foreach (GameObject child in parent.transform)
                {
                    switch(type)
                    {
                        case ObjectType.Wall: 
                            child.layer = (int)ObjectType.Wall;
                            break;

                        case ObjectType.CombatFloor: 
                            child.layer = (int)ObjectType.CombatFloor;
                            GroundCollider groundCollider = child.AddComponent<GroundCollider>();
                            groundCollider.collider = (Collider)child.GetComponent<MeshCollider>(); 
                            break;

                        case ObjectType.NonCombatFloor:
                            child.layer = (int)ObjectType.NonCombatFloor; 
                            break;
                    }
                }
            }
            MelonLogger.Msg("Successfully setup layers");
            return;
        }

        // Code for loading custom lightmaps
        // STOLEN from Orangenal, slightly modified
        // Not used for now
        private Texture2D LoadAsset()
        {
            if (assetBundle == null)
            {
                using (Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream("TheWastes.Resources.wastes"))
                {
                    if (bundleStream == null)
                    {
                        MelonLogger.Error("Failed to find resource stream!");
                        return null;
                    }

                    byte[] bundleBytes = new byte[bundleStream.Length];
                    bundleStream.Read(bundleBytes, 0, bundleBytes.Length);
                    assetBundle = Il2CppAssetBundleManager.LoadFromMemory(bundleBytes);
                }
            }

            if (assetBundle == null)
            {
                MelonLogger.Error("AssetBundle failed to load.");
                return null;
            }

            Texture2D asset = assetBundle.LoadAsset<Texture2D>("");
            
            if (asset == null)
                MelonLogger.Error("Failed to load lightmap texture from AssetBundle!");
            else
                asset.Apply(true, false); // Make sure it's marked readable

            return asset;
        }

        // STOLEN from Orangenal
        //IEnumerator SwapLightmap(bool legacy = false)
        //{
        //    // Setting immediately on scene change doesn't change the lightmap index??
        //    yield return new WaitForSeconds(0.2f);

        //    if (!legacy && !wasLightmapChanged)
        //    {
        //        lightmap = LoadAsset();
        //        if (lightmap == null)
        //        {
        //            MelonLogger.Error("Lightmap is null!");
        //            yield break;
        //        }

        //        // Make a copy of the existing lightmap
        //        LightmapData[] oldLightmaps = LightmapSettings.lightmaps;
        //        LightmapData[] newLightmaps = new LightmapData[oldLightmaps.Length + 1];
        //        for (int i = 0; i < oldLightmaps.Length; i++)
        //            newLightmaps[i] = oldLightmaps[i];

        //        // Swap in our greyscale lightmap
        //        LightmapData customLightmapData = new LightmapData
        //        {
        //            lightmapColor = lightmap
        //        };
        //        newLightmaps[oldLightmaps.Length] = customLightmapData;

        //        LightmapSettings.lightmaps = newLightmaps;

        //        foreach (MeshRenderer r in renderers)
        //        {
        //            r.lightmapIndex = newLightmaps.Length - 1;
        //            r.lightmapScaleOffset = new Vector4(1, 1, 0, 0); // full map
        //        }
        //        wasLightmapChanged = true;
        //    }
        //    else if (!legacy && wasLightmapChanged) // If we already added our lightmap we don't need to do it again
        //    {
        //        foreach (MeshRenderer r in renderers)
        //        {
        //            r.lightmapIndex = 2;
        //        }
        //    }
        //    else
        //    {
        //        foreach (MeshRenderer r in renderers)
        //        {
        //            r.lightmapIndex = 1;
        //        }
        //    }
        //}
    }
}
