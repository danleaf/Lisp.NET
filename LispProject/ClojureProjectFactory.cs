﻿using Microsoft.VisualStudio.Project;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace LispProject
{
    [Guid("FE5C2CE7-D0C1-4F92-9C3A-6A259CB86816")]
    public class ClojureProjectFactory : ProjectFactory
    {
        private Package package;

        public ClojureProjectFactory(Package package)
            : base(package)
        {
            this.package = package;
        }
        protected override ProjectNode CreateProject()
        {
            ClojureProjectNode project = new ClojureProjectNode(this.package);
            project.SetSite((IServiceProvider)((System.IServiceProvider)this.package).GetService(typeof(IServiceProvider)));
            return project;
        }
    }
}
