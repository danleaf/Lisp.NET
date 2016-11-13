using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project.Automation;

namespace LispExtension.Project
{
    [ComVisible(true)]
    public class OALispProject : OAProject
    {
        public OALispProject(LispProjectNode project)
            : base(project)
        {
        }
    }
}
