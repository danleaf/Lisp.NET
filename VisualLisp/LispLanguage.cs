using Microsoft.VisualStudio.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TextManager.Interop;

namespace VisualLisp
{
    [Guid(CLOJURE_LANGUAGE_GUID)]
    class LispLanguage : LanguageService
    {
        public const string CLOJURE_LANGUAGE_NAME = "Lisp";
        public const string CLOJURE_TEMPLATE_FOLDER_NAME = "Lisp";
        public const string CLOJURE_CODE_PROVIDER = "LispCodeProvider";
        public const string CLOJURE_LANGUAGE_GUID = "F6F2DFB5-2D65-49E8-A837-FE7C21D2E331";
        public const string CLJ_FILE_EXTENSION = ".lsp";
        public const string CLJS_FILE_EXTENSION = ".lss";

        LanguagePreferences preferences = null;

        public override string GetFormatFilterList()
        {
            return "Lisp File (*.lsp) *.lss";
        }
        public override LanguagePreferences GetLanguagePreferences()
        {
            if (preferences == null)
            {
                preferences = new LanguagePreferences(this.Site, typeof(LispLanguage).GUID, this.Name);

                if (preferences != null)
                {
                    preferences.Init();

                    preferences.EnableCodeSense = true;
                    preferences.EnableMatchBraces = true;
                    preferences.EnableCommenting = true;
                    preferences.EnableShowMatchingBrace = true;
                    preferences.EnableMatchBracesAtCaret = true;
                    preferences.HighlightMatchingBraceFlags = _HighlightMatchingBraceFlags.HMB_USERECTANGLEBRACES;
                    preferences.LineNumbers = true;
                    preferences.MaxErrorMessages = 100;
                    preferences.AutoOutlining = false;
                    preferences.MaxRegionTime = 2000;
                    preferences.ShowNavigationBar = true;

                    preferences.AutoListMembers = true;
                    preferences.EnableQuickInfo = true;
                    preferences.ParameterInformation = true;
                }
            }

            return preferences;
        }

        public override IScanner GetScanner(Microsoft.VisualStudio.TextManager.Interop.IVsTextLines buffer)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { return CLOJURE_LANGUAGE_NAME; }
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            try
            {
                if (preferences != null)
                {
                    preferences.Dispose();
                    preferences = null;
                }
            }
            finally
            {
                base.Dispose();
            }
        }
    }
}
