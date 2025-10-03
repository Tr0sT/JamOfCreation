//#define DEBUG_IMPORTER

using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.IO;
using System.Linq;

[ScriptedImporter(3, new string[] { "noesiseffect", "noesisbrush" })]
class NoesisShaderImporter : ScriptedImporter
{
      #if UNITY_EDITOR_WIN
        private static string EditorLang = "hlsl";
        private static string Compiler = "Windows/ShaderCompiler.exe";
      #endif

      #if UNITY_EDITOR_OSX
        private static string EditorLang = "mtl";
        private static string Compiler = "MacOS/ShaderCompiler";
      #endif

      #if UNITY_EDITOR_LINUX
        private static string EditorLang = "glsl";
        private static string Compiler = "Linux/ShaderCompiler";
      #endif

    private static string GetLangs(BuildTarget target)
    {
        // EditorLang is only needed for playing in the editor
        // We should find a way to strip this platform for every standalone
        if (target == BuildTarget.StandaloneWindows) return $"{EditorLang},hlsl,glsl,spirv";
        if (target == BuildTarget.StandaloneWindows64) return $"{EditorLang},hlsl,glsl,spirv";
        if (target == BuildTarget.StandaloneOSX ) return $"{EditorLang},mtl";
        if (target == BuildTarget.StandaloneLinux64) return $"{EditorLang},glsl";
        if (target == BuildTarget.WebGL) return $"{EditorLang},essl";
        if (target == BuildTarget.WSAPlayer) return $"{EditorLang},hlsl";
        if (target == BuildTarget.Android) return $"{EditorLang},essl,spirv";
        if (target == BuildTarget.iOS) return $"{EditorLang},mtl";
        if (target == BuildTarget.Switch) return $"{EditorLang},nvn";
        if (target == BuildTarget.PS4) return $"{EditorLang},pssl_orbis";
        if (target == BuildTarget.PS5) return $"{EditorLang},pssl_prospero";
        if (target == BuildTarget.GameCoreXboxSeries) return $"{EditorLang},hlsl";
        if (target == BuildTarget.GameCoreXboxOne) return $"{EditorLang},hlsl";

        return "";
    }

    private static byte[] CompileShader(AssetImportContext ctx)
    {
        string compiler = Path.GetFullPath($"Packages/com.noesis.noesisgui/Shaders/Bin/{Compiler}");
        string include = Path.GetFullPath("Packages/com.noesis.noesisgui/Shaders/Include");
        string lang = GetLangs(ctx.selectedBuildTarget);

        if (lang.Length == 0)
        {
            ctx.LogImportError($"{ctx.assetPath}: '{ctx.selectedBuildTarget}' is not supported");
        }
        else
        {
            try
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = compiler;
                process.StartInfo.Arguments = $"-lang={lang} -I\"{include}\" -o Temp/shader.bin \"{ctx.assetPath}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                process.WaitForExit();

                string err = process.StandardError.ReadToEnd();
                if (err.Length != 0)
                {
                    ctx.LogImportError(err.Replace("\\", "/").Replace(Application.dataPath, "Assets"));
                }

                if (process.ExitCode == 0)
                {
                    return File.ReadAllBytes(@"Temp\shader.bin");
                }
            }
            catch (System.Exception e)
            {
                ctx.LogImportError(e.Message);
            }
        }

        return null;
    }

    public override void OnImportAsset(AssetImportContext ctx)
    {
      #if DEBUG_IMPORTER
        Debug.Log($"=> Import {ctx.assetPath} - {ctx.selectedBuildTarget}");
      #endif

        NoesisShader shader = (NoesisShader)ScriptableObject.CreateInstance<NoesisShader>();
        shader.label = Path.GetFileNameWithoutExtension(ctx.assetPath);
        shader.code = CompileShader(ctx);
        shader.type = ctx.assetPath.EndsWith(".noesisbrush") ? 1 : 0;

        if (ctx.assetPath.EndsWith(".noesiseffect"))
        {
            ctx.DependsOnSourceAsset("Packages/com.noesis.noesisgui/Shaders/EffectHelpers.h");
            ctx.DependsOnSourceAsset("Packages/com.noesis.noesisgui/Shaders/BaseShader.h");
        }
        else
        {
            ctx.DependsOnSourceAsset("Packages/com.noesis.noesisgui/Shaders/BrushHelpers.h");
            ctx.DependsOnSourceAsset("Packages/com.noesis.noesisgui/Shaders/BaseShader.h");
        }

        ctx.AddObjectToAsset("Shader", shader);
    }
}
