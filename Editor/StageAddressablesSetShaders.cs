using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using ThunderKit.Core.Attributes;
using System.Threading.Tasks;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests.Datum;
using ThunderKit.Core.Paths;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Build;
using ThunderKit.Addressable.Manifest;
using ThunderKit.Addressable.Builders;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.AddressableAssets.ResourceLocators;

namespace ThunderKit.Addressable.PipelineJobs
{

    //TODO: Documentation for this
    [PipelineSupport(typeof(Pipeline)), RequiresManifestDatumType(typeof(AddressableSettings))]
    public class StageAddressablesSetShaders : PipelineJob
    {
        [Header("Note: this will use the AddressableSettings attached to the manifest as base.")]

        [Tooltip("The name of the catalog file that will be produced. Path Reference compatible. If left empty, will default to \"catalog\".")]
        [PathReferenceResolver]
        public string catalogName = "<ManifestName>Catalog";

        [HideInInspector]
        [Tooltip("The key of the catalog, used for loading the bundle from the settings.json file. Path Reference compatible. Cannot be named \"AddressablesMainContentCatalog\".")]
        [PathReferenceResolver]
        public string catalogID = "AddressablesMainContentCatalog";

        [Tooltip("Uses Addressables Path resolving. Use it to point to a static string property within your mod's assembly. It should point to the directory of your catalog (not the path). This should not be left as the default.")]
        [PathReferenceResolver]
        public string localCatalogLoadDirectory = "{UnityEngine.AddressableAssets.Addressables.RuntimePath}";

        public override Task Execute(Pipeline pipeline)
        {
            AssetDatabase.SaveAssets();
            for (pipeline.ManifestIndex = 0; pipeline.ManifestIndex < pipeline.Manifests.Length; pipeline.ManifestIndex++)
            {
                var ads = pipeline.Manifest.Data.OfType<AddressableSettings>().FirstOrDefault();
                var addressableAssetSettings = ads.addressableAssetSettings;
                if (!addressableAssetSettings)
                {
                    pipeline.Log(LogLevel.Warning, $"No AddressableSettings found on manifest {pipeline.manifest.name}, skipping.");
                    return Task.CompletedTask;
                }
                string catName = !string.IsNullOrEmpty(catalogName) ? PathReference.ResolvePath(catalogName, pipeline, this).Replace(" ", "") : "catalog";
                string id = PathReference.ResolvePath(catalogID, pipeline, this).Replace(" ", "");
                string catalogLoadDir = PathReference.ResolvePath(localCatalogLoadDirectory, pipeline, this);

                string catNameExt = $"{catName}.json";
                var builderInput = new AddressablesDataBuilderInput(addressableAssetSettings)
                {
                    RuntimeCatalogFilename = catNameExt
                };

                ModdedBuildScriptBase buildScript;
                //TODO: Verify whether the assets have changed for faster building.
                //if(AddressableHelper.SettingsExist())
                //{
                //    var rtd = JsonUtility.FromJson<ResourceManagerRuntimeData>(File.ReadAllText(AddressableHelper.SettingsPath));
                //    var mainCatalog = rtd.CatalogLocations.First();
                //    if(mainCatalog.Keys.First() == id && mainCatalog.InternalId == $"{catalogLoadDir}/{catNameExt}")
                //    {
                //    }
                //}

                buildScript = new ModdedBuildScriptPackedMode(catName, id, catalogLoadDir, pipeline);
                buildScript.BuildData<AddressableAssetBuildResult>(builderInput);

                foreach (var stage in ads.StagingPaths)
                {
                    FileUtil.ReplaceDirectory(Addressables.BuildPath, PathReference.ResolvePath(stage, pipeline, this));
                }
            }
            pipeline.ManifestIndex = -1;

            return Task.CompletedTask;
        }
    }
}
