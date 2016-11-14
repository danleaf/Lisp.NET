using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisualLisp.Project
{
    public class OALispProjectFileItem : OAFileItem
    {
        public OALispProjectFileItem(OAProject project, FileNode node)
            : base(project, node)
        {
        }
    }
}
