using Microsoft.VisualStudio.Project;
using System;
using System.Runtime.InteropServices;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Dandan.VisualLisp.Project
{
    [Guid(GuidList.guidProjectFactoryString)]
    class LispProjectFactory : ProjectFactory
    {
        VisualLispPackage package;

        public LispProjectFactory(VisualLispPackage package)
            : base(package)
        {
            this.package = package;
        }

        protected override ProjectNode CreateProject()
        {
            LispProjectNode project = new LispProjectNode(this.package);

            project.SetSite((IOleServiceProvider)((IServiceProvider)this.package).GetService(typeof(IOleServiceProvider)));
            return project;
        }
    }
}
