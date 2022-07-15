using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.AddressableAssets;
using ThunderKit.Addressable.Tools;
using System.Linq;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ThunderKit.Addressable.AddressableMaterials
{
    [InitializeOnLoad]
    public class AddressableMaterial : ScriptableObject
    {
        public string shaderAddress;
        public Material material;


        private Shader loadedShader;
        private IResourceLocation resourceLocation;

        public void OnEnable()
        {
            Load();
        }

        public void Load()
        {
            if (!AddressableGraphicsSettings.Initialized)
            {
                AddressableGraphicsSettings.AddressablesInitialized += Load;
                return;
            }
            AddressableGraphicsSettings.AddressablesInitialized -= Load;
            resourceLocation = Addressables.LoadResourceLocationsAsync(shaderAddress, typeof(Shader)).WaitForCompletion().FirstOrDefault();
            Debug.Log(resourceLocation.Dependencies.First());
            

            var shader = Addressables.LoadAssetAsync<Shader>(shaderAddress).WaitForCompletion();
            loadedShader = shader;
            ShaderUtil.RegisterShader(loadedShader);
            material.shader = shader;
        }

        public void Load(object sender, EventArgs e)
        {
            Load();
        }
    }
}