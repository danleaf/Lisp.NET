using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Shell;
using VSLangProj;

namespace LispExtension.Project
{
    [Guid("16065A00-E5D1-45A7-95F5-C46B2BDBB833")]
    public class LispProjectNode : ProjectNode
    {
        internal enum MyCustomProjectImageName
        {
            Project = 0,
        }

        internal const string ProjectTypeName = "Lisp";

        private Package package;
        internal static int imageOffset;
        private static ImageList imageList;
        private VSProject vsProject;

        public Package Package
        {
            get { return package; }
        }

        static LispProjectNode()
        {
            imageList = Microsoft.VisualStudio.Project.Utilities.GetImageList(typeof(LispProjectNode).Assembly.GetManifestResourceStream("LispProject.Resources.ImageList.bmp"));
        }
        
        public LispProjectNode(Package package)
        {
            this.package = package;

            InitializeImageList();

            CanProjectDeleteItems = true;
        }
        public static ImageList ImageList
        {
            get { return imageList; }
            set { imageList = value; }
        }

        protected internal VSProject VSProject
        {
            get
            {
                if (vsProject == null)
                {
                    vsProject = new OAVSProject(this);
                }

                return vsProject;
            }
        }

        public References References { get { return VSProject.References; } }

        public override Guid ProjectGuid
        {
            get { return typeof(LispProjectFactory).GUID; }
        }

        public override bool IsCodeFile(string fileName)
        {
            return fileName.ToLower().EndsWith(".lsp") || fileName.ToLower().EndsWith(".lss");
        }

        public override string ProjectType
        {
            get { return ProjectTypeName; }
        }

        public override int ImageIndex
        {
            get { return imageOffset + (int)MyCustomProjectImageName.Project; }
        }

        public override object GetAutomationObject()
        {
            return new OALispProject(this);
        }

        public override FileNode CreateFileNode(ProjectElement item)
        {
            LispFileNode node = new LispFileNode(this, item);

            node.OleServiceProvider.AddService(typeof(EnvDTE.Project), new OleServiceProvider.ServiceCreatorCallback(CreateServices), false);
            node.OleServiceProvider.AddService(typeof(ProjectItem), node.ServiceCreator, false);
            node.OleServiceProvider.AddService(typeof(VSProject), new OleServiceProvider.ServiceCreatorCallback(CreateServices), false);

            return node;
        }

        //protected override Guid[] GetConfigurationIndependentPropertyPages()
        //{
        //    Guid[] result = new Guid[1];
        //    result[0] = typeof(GeneralPropertyPage).GUID;
        //    return result;
        //}

        //protected override Guid[] GetPriorityProjectDesignerPages()
        //{
        //    Guid[] result = new Guid[1];
        //    result[0] = typeof(GeneralPropertyPage).GUID;
        //    return result;
        //}

        public override void AddFileFromTemplate(string source, string target)
        {
            if (!File.Exists(source))
            {
                throw new FileNotFoundException(string.Format("Template file not found: {0}", source));
            }

            string projectPath = Path.GetDirectoryName(GetMkDocument());
            string relativePath = Path.GetDirectoryName(target).Substring(projectPath.Length);

            string fileNamespace = relativePath.Replace("\\", ".");
            fileNamespace = fileNamespace.TrimStart(new[] { '.' });
            if (!string.IsNullOrEmpty(fileNamespace)) fileNamespace += ".";
            fileNamespace += Path.GetFileNameWithoutExtension(target);

            FileTemplateProcessor.AddReplace("%namespace%", fileNamespace);

            try
            {
                FileTemplateProcessor.UntokenFile(source, target);
                FileTemplateProcessor.Reset();
            }
            catch (Exception e)
            {
                throw new FileLoadException("Failed to add template file to project", target, e);
            }
        }
        
        private void InitializeImageList()
        {
            imageOffset = ImageHandler.ImageList.Images.Count;

            foreach (Image img in ImageList.Images)
            {
                ImageHandler.AddImage(img);
            }
        }

        private object CreateServices(Type serviceType)
        {
            object service = null;
            if (typeof(VSProject) == serviceType)
            {
                service = VSProject;
            }
            else if (typeof(EnvDTE.Project) == serviceType)
            {
                service = GetAutomationObject();
            }
            return service;
        }

        //protected override ConfigProvider CreateConfigProvider()
        //{
        //    return new ClojureConfigProvider(this);
        //}
    }
}
