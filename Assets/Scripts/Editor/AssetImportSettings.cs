using UnityEditor;

namespace ComputerExplorer.EditorTools
{
    /// <summary>
    /// Import rules so generated art works without manual setup:
    /// icons/images → UI sprites; AR target PNGs → readable, uncompressed RGBA32 (required for runtime Vuforia
    /// Image Targets); app icon → uncompressed.
    /// </summary>
    public class AssetImportSettings : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            var importer = (TextureImporter)assetImporter;
            string path = assetPath.Replace('\\', '/');

            if (path.Contains("/Resources/Icons/") || path.Contains("/Resources/Images/") || path.Contains("/Images/UI/"))
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.maxTextureSize = path.Contains("/Icons/") ? 256 : 1024;
            }
            else if (path.Contains("/Resources/ARTargets/") || path.Contains("/Images/ARTargets/"))
            {
                importer.textureType = TextureImporterType.Default;
                importer.isReadable = true;
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 1024;
                foreach (var platform in new[] { "Android", "iPhone", "Standalone" })
                {
                    var s = importer.GetPlatformTextureSettings(platform);
                    s.overridden = true;
                    s.format = TextureImporterFormat.RGBA32;
                    s.maxTextureSize = 1024;
                    importer.SetPlatformTextureSettings(s);
                }
            }
        }
    }
}
