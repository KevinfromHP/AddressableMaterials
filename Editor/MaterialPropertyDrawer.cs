using System.Collections;
using UnityEngine;
using UnityEditor;
using ThunderKit.Addressable.Tools;
using UnityEngine.AddressableAssets;
using System;
using Object = UnityEngine.Object;

 namespace ThunderKit.Addressable.AddressableMaterials
{
    [InitializeOnLoad]
    public class MaterialEditorAdditions
    {
        static MaterialEditorAdditions()
        {
            Editor.finishedDefaultHeaderGUI += Draw;
        }

        private static void Draw(Editor editor)
        {
            if (!(editor is MaterialEditor materialEditor))
            {
                return;
            }

            var id = GUIUtility.GetControlID(new GUIContent("Pick Addressable Shader"), FocusType.Passive);

            if (GUILayout.Button("Pick Addressable Shader"))
            {
                var objectSelector = EditorWindow.CreateWindow<AddressableObjectSelector>();
                objectSelector.searchInput = "t:Shader";
                var mousePosition = Event.current.mousePosition;
                objectSelector.position = new Rect(mousePosition.x, mousePosition.y, 800, 400);
                objectSelector.onItemChosen += (address) =>
                {
                    try
                    {
                        var target = materialEditor.target;
                        var targetPath = AssetDatabase.GetAssetPath(target);
                        if (!string.IsNullOrEmpty(targetPath))
                        {
                            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(targetPath))
                            {
                                if (!AssetDatabase.IsMainAsset(asset))
                                {
                                    AssetDatabase.RemoveObjectFromAsset(asset);
                                    Object.DestroyImmediate(asset);
                                }
                            }
                            var adm = ScriptableObject.CreateInstance<AddressableMaterial>();
                            adm.shaderAddress = address.ToString();
                            adm.material = target as Material;
                            adm.name = "AddressableMaterial";
                            AssetDatabase.AddObjectToAsset(adm, target);
                            AssetDatabase.SetMainObject(target, targetPath);
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(adm));
                            AssetDatabase.ImportAsset(targetPath);
                            adm.Load();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }

                };
            }
        }
    }
}