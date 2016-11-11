using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project.Automation;

namespace LispProject
{
    [ComVisible(true)]
    public class OAClojureProject : OAProject
    {
        public OAClojureProject(ClojureProjectNode project)
            : base(project)
        {
        }
    }
}
