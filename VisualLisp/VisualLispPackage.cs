using Dandan.VisualLisp.Language;
using Dandan.VisualLisp.Project;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Dandan.VisualLisp
{
    [ProvideProjectFactory(typeof(LispProjectFactory), "Visual Lisp",
        @"Lisp Project Files (*.lsproj);*.lsproj", "lsproj", "lsproj",
        @"Templates\Projects", LanguageVsTemplate = "Lisp")]
    [ProvideService(typeof(LispLangService), ServiceName = LispLangService.LangName)]
    [ProvideLanguageService(
        typeof(LispLangService),
        LispLangService.LangName,
        100,
        CodeSense = true,
        DefaultToInsertSpaces = true,
        EnableCommenting = true,
        MatchBraces = true,
        MatchBracesAtCaret = true,
        ShowCompletion = true,
        ShowMatchingBrace = true,
        QuickInfo = true,
        AutoOutlining = true,
        DebuggerLanguageExpressionEvaluator = GuidList.guidLuanguageString)]

    [ProvideLanguageExtension(typeof(LispLangService), ".ls")]
    [ProvideLanguageExtension(typeof(LispLangService), ".lss")]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidPackageString)]
    public sealed class VisualLispPackage : ProjectPackage, IOleComponent
    {
        private uint m_componentID;

        public VisualLispPackage()
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            this.RegisterProjectFactory(new LispProjectFactory(this));
            this.RegisterLangugeService(new LispLangService());
        }

        private void RegisterLangugeService(LispLangService langService)
        {
            // Proffer the service.
            IServiceContainer serviceContainer = this as IServiceContainer;
            langService.SetSite(this);
            serviceContainer.AddService(typeof(LispLangService), langService, true);

            // Register a timer to call our language service during
            // idle periods.
            IOleComponentManager mgr = GetService(typeof(SOleComponentManager))
                                       as IOleComponentManager;
            if (m_componentID == 0 && mgr != null)
            {
                OLECRINFO[] crinfo = new OLECRINFO[1];
                crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
                crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime |
                                              (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
                crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal |
                                              (uint)_OLECADVF.olecadvfRedrawOff |
                                              (uint)_OLECADVF.olecadvfWarningsOff;
                crinfo[0].uIdleTimeInterval = 1000;
                int hr = mgr.FRegisterComponent(this, crinfo, out m_componentID);
            }
        }

        public override string ProductUserContext
        {
            get { return "LispProj"; }
        }

        protected override void Dispose(bool disposing)
        {
            if (m_componentID != 0)
            {
                IOleComponentManager mgr = GetService(typeof(SOleComponentManager))
                                           as IOleComponentManager;
                if (mgr != null)
                {
                    int hr = mgr.FRevokeComponent(m_componentID);
                }
                m_componentID = 0;
            }

            base.Dispose(disposing);
        }


        #region IOleComponent Members

        public int FDoIdle(uint grfidlef)
        {
            bool bPeriodic = (grfidlef & (uint)_OLEIDLEF.oleidlefPeriodic) != 0;
            // Use typeof(LispLangService) because we need to
            // reference the GUID for our language service.
            LanguageService service = GetService(typeof(LispLangService)) as LanguageService;

            if (service != null)
            {
                service.OnIdle(bPeriodic);
            }
            return 0;
        }

        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
        {
            return 1;
        }

        public int FPreTranslateMessage(MSG[] pMsg)
        {
            return 0;
        }

        public int FQueryTerminate(int fPromptUser)
        {
            return 1;
        }

        public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }

        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved)
        {
            return IntPtr.Zero;
        }

        public void OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo,
            int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved)
        {
        }

        public void OnAppActivate(int fActive, uint dwOtherThreadID)
        {
        }

        public void OnEnterState(uint uStateID, int fEnter)
        {
        }

        public void OnLoseActivation()
        {
        }

        public void Terminate()
        {
        }

        #endregion

    }
}
