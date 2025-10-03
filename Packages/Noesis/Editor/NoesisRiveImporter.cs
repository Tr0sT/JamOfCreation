//#define DEBUG_IMPORTER

using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using Noesis;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;


[ScriptedImporter(1, "riv")]
class NoesisRiveImporter : ScriptedImporter
{
    static string[] GetDependencies(string path)
    {
        NoesisUnity.InitCore();
        string[] dependencies;

        using (FileStream file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            _dependencies = new List<string>();
            Noesis_GetRiveDependencies(BaseComponent.GetPtr(file), path, _dependencyCallback);
            dependencies = _dependencies.ToArray();
            _dependencies = null;
        }

        return dependencies;
    }

    static string[] GatherDependenciesFromSourceFile(string path)
    {
        return GetDependencies(path);
    }

    public override void OnImportAsset(AssetImportContext ctx)
    {
        NoesisUnity.InitCore();

        #if DEBUG_IMPORTER
            Debug.Log($"=> Import {ctx.assetPath}");
        #endif

        NoesisRive rive = ScriptableObject.CreateInstance<NoesisRive>();
        rive.uri = ctx.assetPath;
        rive.content = File.ReadAllBytes(ctx.assetPath);

        ctx.AddObjectToAsset("Rive", rive);
        ctx.SetMainObject(rive);

        string[] dependencies = GetDependencies(ctx.assetPath);
        List<UnityEngine.Texture> textures = new List<UnityEngine.Texture>();
        List<NoesisFont> fonts = new List<NoesisFont>();

        foreach (var uri in dependencies)
        {
            ctx.DependsOnArtifact(uri);

            if (!String.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(uri)))
            {
                if (AssetImporter.GetAtPath(uri) is TextureImporter textureImporter)
                {
                    if (!AssetDatabase.GetLabels(textureImporter).Contains("Noesis"))
                    {
                        Debug.LogWarning($"{uri} is missing Noesis label");

                        UnityEngine.Texture texture = AssetDatabase.LoadAssetAtPath<UnityEngine.Texture>(uri);

                        if (texture != null)
                        {
                            textures.Add(texture);
                        }
                    }
                }
                else
                {
                    NoesisFont font = AssetDatabase.LoadAssetAtPath<NoesisFont>(uri);

                    if (font != null)
                    {
                        fonts.Add(font);
                    }
                }
            }
        }

        rive.textures = textures.Select(x => new NoesisRive.Texture { uri = AssetDatabase.GetAssetPath(x), texture = x }).ToArray();
        rive.fonts = fonts.Select(x => new NoesisRive.Font { uri = AssetDatabase.GetAssetPath(x), font = x }).ToArray();
    }

    [DllImport(Library.Name)]
    static extern void Noesis_GetRiveDependencies(IntPtr stream, [MarshalAs(UnmanagedType.LPStr)] string baseUri,
        DependencyCallback callback);

    private delegate void DependencyCallback([MarshalAs(UnmanagedType.LPWStr)]string uri);
    private static DependencyCallback _dependencyCallback = OnRiveDependency;

    [MonoPInvokeCallback(typeof(DependencyCallback))]
    private static void OnRiveDependency(string uri)
    {
        _dependencies.Add(uri);
    }

    [ThreadStatic]
    private static List<string> _dependencies = null;
}
