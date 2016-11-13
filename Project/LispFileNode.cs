using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using System.Drawing;

namespace LispExtension.Project
{
    public class LispFileNode : FileNode
	{
		private OALispProjectFileItem automationObject;


        internal LispFileNode(ProjectNode root, ProjectElement e)
			: base(root, e)
		{
		}

		public override object GetAutomationObject()
		{
			if(automationObject == null)
			{
				automationObject = new OALispProjectFileItem(this.ProjectMgr.GetAutomationObject() as OAProject, this);
			}

			return automationObject;
		}

		public override object GetIconHandle(bool open)
		{
			Bitmap image = (Bitmap) LispProjectNode.ImageList.Images[1];
			return image.GetHicon();
		}

		internal OleServiceProvider.ServiceCreatorCallback ServiceCreator
		{
			get { return new OleServiceProvider.ServiceCreatorCallback(this.CreateServices); }
		}

		private object CreateServices(Type serviceType)
		{
			object service = null;
			if(typeof(EnvDTE.ProjectItem) == serviceType)
			{
				service = GetAutomationObject();
			}
			return service;
		}
	}
}
