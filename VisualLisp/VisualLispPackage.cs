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
using LispProject;

namespace VisualLisp
{
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]

    [Guid("87A7B551-2144-4AD9-B3AA-1117EE0B5686")]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\12.0")]
    [ProvideService(typeof(LispLanguage), ServiceName = LispLanguage.CLOJURE_LANGUAGE_NAME)]
    [ProvideLanguageService(typeof(LispLanguage), LispLanguage.CLOJURE_LANGUAGE_NAME, 100, CodeSense = true, DefaultToInsertSpaces = true, EnableCommenting = true, MatchBraces = true, MatchBracesAtCaret = true, ShowCompletion = true, ShowMatchingBrace = true, QuickInfo = true, AutoOutlining = true, DebuggerLanguageExpressionEvaluator = LispLanguage.CLOJURE_LANGUAGE_GUID)]
    [ProvideLanguageExtension(typeof(LispLanguage), LispLanguage.CLJ_FILE_EXTENSION)]
    [ProvideLanguageExtension(typeof(LispLanguage), LispLanguage.CLJS_FILE_EXTENSION)]
    //[RegisterSnippetsAttribute(ClojureLanguage.CLOJURE_LANGUAGE_GUID, false, 131, ClojureLanguage.CLOJURE_LANGUAGE_NAME, @"CodeSnippets\SnippetsIndex.xml", @"CodeSnippets\Snippets\", @"CodeSnippets\Snippets\")]
    //[ProvideObject(typeof(GeneralPropertyPageAdapter))]
    [ProvideProjectFactory(typeof(ClojureProjectFactory), "Lisp", "Lisp Project Files (*.lsproj);*.lsproj", "lsproj", "lsproj", @"Templates\Projects", LanguageVsTemplate = "Lisp", NewProjectRequireNewFolderVsTemplate = false)]
    [ProvideProjectItem(typeof(ClojureProjectFactory), "Lisp Items", @"Templates\ProjectItems\Lisp", 500)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    //[ProvideToolWindow(typeof(ReplToolWindow))]
    //[RegisterExpressionEvaluator(typeof(ExpressionEvaluator), ClojureLanguage.CLOJURE_LANGUAGE_NAME, ExpressionEvaluator.CLOJURE_DEBUG_EXPRESSION_EVALUATOR_GUID, ExpressionEvaluator.MICROSOFT_VENDOR_GUID)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    public sealed class VisualLispPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public VisualLispPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            //RegisterProjectFactory(new ClojureProjectFactory(this));
        }
        #endregion

    }
}
