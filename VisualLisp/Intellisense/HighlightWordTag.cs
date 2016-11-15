using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Dandan.VisualLisp.Intellisense
{
    //可以在 工具->选项->环境->字体和颜色 中添加配色选项
    [Export(typeof(EditorFormatDefinition))]
    [Name("dandan.highlight")]
    [UserVisible(true)]
    class HighlightFormatDefinition : MarkerFormatDefinition
    {
        public HighlightFormatDefinition()
        {
            this.BackgroundColor = Colors.LightGray;
            this.ForegroundColor = Colors.LightGray;
            this.DisplayName = "Highlight";
            this.ZOrder = 5;
        }
    }
}
