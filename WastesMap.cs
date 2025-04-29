using CustomMapLib;
using Il2CppRUMBLE.MoveSystem;
using MelonLoader;
using UnityEngine;
using static RumbleModdingAPI.Calls.GameObjects.Gym.Logic.HeinhouserProducts.Leaderboard;
using static RumbleModdingAPI.Calls;
using BuildInfo = TheWastes.BuildInfo;

[assembly: MelonInfo(typeof(TheWastes.WastesMap), BuildInfo.Name, BuildInfo.Version, BuildInfo.Author, BuildInfo.DownloadLink)]
[assembly: MelonGame(null, null)]

namespace TheWastes
{
    public static class BuildInfo
    {
        public const string Name = "The Wastes";
        public const string Description = "A huge desolate Island";
        public const string Author = "SisterPankake";
        public const string Company = null;
        public const string Version = "1.0.0";
        public const string DownloadLink = null;
    }

    public class WastesMap : Map
    {
        public override void OnLateInitializeMelon() => Initialize(BuildInfo.Name, BuildInfo.Version, BuildInfo.Author);

        GameObject arena = new GameObject();
        GameObject net = new GameObject();
        GameObject spawn = new GameObject();
        GameObject arena_Floor = new GameObject();
        GameObject islands = new GameObject();
        GameObject kill_Vis = new GameObject();
        Material skybox = new Material(Shader.FindBuiltin("Skybox/Procedural"));

        public override void OnMapCreation()
        {
            // Load and assign Map
            Il2CppAssetBundle bundle = LoadBundle("TheWastes.Resources.wastes");
            arena = GameObject.Instantiate(bundle.LoadAsset<GameObject>("TheWastes"));
            net = arena.transform.GetChild(0).gameObject;
            spawn = arena.transform.GetChild(1).gameObject;
            arena_Floor = arena.transform.GetChild(2).gameObject;
            islands = arena.transform.GetChild(3).gameObject;
            kill_Vis = arena.transform.GetChild(4).gameObject;

            arena.transform.SetParent(mapParent.transform);
            GroundCollider groundCollider = arena_Floor.AddComponent<GroundCollider>();
            groundCollider.isMainGroundCollider = true;
            groundCollider.collider = arena_Floor.GetComponent<MeshCollider>();

            // Assign SkyBox 
            skybox = bundle.LoadAsset<Material>("Desert_Sky");
        }

        public override void OnMapMatchLoad(bool amHost)
        {
            // Set Spawn Locations
            HostPedestal.SetFirstSequence(spawn.transform.GetChild(0).position);
            HostPedestal.SetSecondSequence(spawn.transform.GetChild(1).position);
            ClientPedestal.SetFirstSequence(spawn.transform.GetChild(2).position);
            ClientPedestal.SetSecondSequence(spawn.transform.GetChild(3).position);

            // Assign SkyBox
            RenderSettings.skybox = skybox;
        }
        public Il2CppAssetBundle LoadBundle(string path)
        {
#pragma warning disable CS8600, CS8602 // Converting null literal or possible null value to non-nullable type.
            using (System.IO.Stream bundleStream = MelonAssembly.Assembly.GetManifestResourceStream(path))
            {
                byte[] bundleBytes = new byte[bundleStream.Length];
                bundleStream.Read(bundleBytes, 0, bundleBytes.Length);
                return Il2CppAssetBundleManager.LoadFromMemory(bundleBytes);
            }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
    }
}
