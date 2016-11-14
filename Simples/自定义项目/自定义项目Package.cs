using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Project;

namespace Company.自定义项目
{
    [ProvideProjectFactory(typeof(自定义项目Factory), "Visual L#",
    "LSharp Project Files (*.lsproj);*.lsproj", "lsproj", "lsproj",
    @"Templates\Projects", LanguageVsTemplate = "LSharp")]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidPkgString)]
    public sealed class 自定义项目Package : ProjectPackage
    {
        public 自定义项目Package()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();
            this.RegisterProjectFactory(new 自定义项目Factory(this));
        }

        public override string ProductUserContext
        {
            get { return "LSharpProj"; }
        }
    }
}
