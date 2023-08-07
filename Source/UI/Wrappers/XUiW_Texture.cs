using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CustomModManager.UI.Wrappers
{
    public sealed class XUiW_Texture : XUiW<XUiV_Texture>
    {
        private static readonly FieldInfo wwwAssignedField = AccessTools.DeclaredField(typeof(XUiV_Texture), "wwwAssigned");
        private IXUiTexture texture;

        public XUiW_Texture(XUiController controller, string childName) : base(controller, childName)
        {
        }

        public void SetTexture(IXUiTexture texture)
        {
            this.ViewComponent.IsVisible = false;
            
            if(this.texture != null)
                this.texture.Unload(this.ViewComponent);
    
            texture.Load(this.ViewComponent);
            this.ViewComponent.UpdateData();
            this.ViewComponent.IsVisible = true;

            this.texture = texture;
        }

        public int GetHeight()
        {
            return this.ViewComponent.Size.y;
        }

        public int GetWidth()
        {
            return this.ViewComponent.Size.x;
        }

        public interface IXUiTexture
        {
            void Load(XUiV_Texture texture);

            void Unload(XUiV_Texture texture);
        }

        public sealed class XUiTexturePath : IXUiTexture
        {
            private readonly string modFolderRelativePath;

            public XUiTexturePath(string modFolderRelativePath)
            {
                this.modFolderRelativePath = modFolderRelativePath;
            }

            public void Load(XUiV_Texture texture)
            {
                texture.ParseAttribute("texture", this.modFolderRelativePath, null);
            }

            public void Unload(XUiV_Texture texture)
            {
                texture.UnloadTexture();
            }
        }

        public sealed class XUiTextureAssemblyResource : IXUiTexture
        {
            private readonly string manifestResourcePath;

            public XUiTextureAssemblyResource(string manifestResourcePath)
            {
                this.manifestResourcePath = manifestResourcePath;
            }

            public void Load(XUiV_Texture texture)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(this.manifestResourcePath))
                    {
                        stream.CopyTo(memoryStream);
                    }

                    byte[] data = memoryStream.ToArray();
                    
                    Texture2D texture2d = new Texture2D(0, 0);
                    texture2d.LoadImage(data);

                    texture.Texture = texture2d;
                    wwwAssignedField.SetValue(texture, true);
                }
            }

            public void Unload(XUiV_Texture texture)
            {
                texture.Texture = null;
                texture.UITexture.mainTexture = null;
            }
        }
    }
}
