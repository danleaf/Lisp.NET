using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Project;
using System;
using System.Drawing;
using System.Windows.Forms;

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using System.IO;


namespace Company.自定义项目
{
    [Guid("1b7499a9-8ca7-4b5f-890c-154df4894ed2")]
    class 自定义项目Factory : ProjectFactory
    {
        自定义项目Package package;

        public 自定义项目Factory(自定义项目Package package)
            : base(package)
        {
            this.package = package;
        }

        protected override ProjectNode CreateProject()
        {
            自定义项目Node project = new 自定义项目Node(this.package);

            project.SetSite((IOleServiceProvider)((IServiceProvider)this.package).GetService(typeof(IOleServiceProvider)));
            return project;
        }
    }

    public class 自定义项目Node : ProjectNode
    {
        private 自定义项目Package package;

        public 自定义项目Node(自定义项目Package package)
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
            get { return typeof(自定义项目Factory).GUID; }
        }
        public override string ProjectType
        {
            get { return "LSharpProject"; }
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

        static 自定义项目Node()
        {
            imageList = Utilities.GetImageList(typeof(自定义项目Node).Assembly.GetManifestResourceStream("Company.自定义项目.Resources.ImageList.bmp"));
        }

        internal static int imageIndex;
        public override int ImageIndex
        {
            get { return imageIndex; }
        }
    }

}
