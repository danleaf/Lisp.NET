using Microsoft.VisualStudio.Project;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Dandan.VisualLisp.Project
{
    public class LispProjectNode : ProjectNode
    {
        private VisualLispPackage package;

        public LispProjectNode(VisualLispPackage package)
        {
            this.package = package;

            imageIndex = this.ImageHandler.ImageList.Images.Count;

            foreach (Image img in imageList.Images)
            {
                this.ImageHandler.AddImage(img);
            }
        }
        public override Guid ProjectGuid
        {
            get { return typeof(LispProjectFactory).GUID; }
        }
        public override string ProjectType
        {
            get { return "LispProject"; }
        }

        public override void AddFileFromTemplate(string source, string target)
        {
            string nameSpace = this.FileTemplateProcessor.GetFileNamespace(target, this);
            string className = Path.GetFileNameWithoutExtension(target);

            this.FileTemplateProcessor.AddReplace("$namespace$", nameSpace);
            this.FileTemplateProcessor.AddReplace("$classname$", className);

            this.FileTemplateProcessor.UntokenFile(source, target);
            this.FileTemplateProcessor.Reset();
        }

        private static ImageList imageList;

        static LispProjectNode()
        {
            imageList = Utilities.GetImageList(
                typeof(LispProjectNode).Assembly
                .GetManifestResourceStream("Dandan.VisualLisp.Resources.ImageList.bmp"));
        }

        internal static int imageIndex;
        public override int ImageIndex
        {
            get { return imageIndex; }
        }
    }
}
