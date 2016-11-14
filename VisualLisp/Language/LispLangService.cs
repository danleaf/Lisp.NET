using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

namespace Dandan.VisualLisp.Language
{
    [Guid(GuidList.guidLuanguageString)]
    class LispLangService : LanguageService
    {
        public const string LangName = "Lisp";
        LanguagePreferences preferences = null; 
        LispScanner scanner;

        public override string GetFormatFilterList()
        {
            return "Lisp File (*.lsp) *.lss";
        }

        public override LanguagePreferences GetLanguagePreferences()
        {
            if (preferences == null)
            {
                preferences = new LanguagePreferences(this.Site, typeof(LispLangService).GUID, this.Name);

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

        public override IScanner GetScanner(IVsTextLines buffer)
        {
            if (scanner == null)
            {
                scanner = new LispScanner(buffer);
            }
            return scanner;
        }

        public override string Name
        {
            get { return LangName; }
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            return new LispAuthoringScope();
        }
    }

    internal class LispScanner : IScanner
    {
        private IVsTextBuffer buffer;
        string source;

        public LispScanner(IVsTextBuffer buffer)
        {
            this.buffer = buffer;
        }

        private string m_line;
        private int m_offset;
        private string m_source;


        /////////////////////////////////////////////////////
        // Enumerations

        private enum ParseState
        {
            InText = 0,
            InQuotes = 1,
            InComment = 2
        }

        /////////////////////////////////////////////////////
        // Private methods

        private bool GetNextToken(int startIndex,
                                 TokenInfo tokenInfo,
                                 ref int state)
        {
            bool bFoundToken = false;
            int endIndex = -1;
            int index = startIndex;
            if (index < m_source.Length)
            {
                if (state == (int)ParseState.InQuotes)
                {
                    // Find end quote. If found, set state to InText
                    // and return the quoted string as a single token.
                    // Otherwise, return the string to the end of the line
                    // and keep the same state.
                }
                else if (state == (int)ParseState.InComment)
                {
                    // Find end of comment. If found, set state to InText
                    // and return the comment as a single token.
                    // Otherwise, return the comment to the end of the line
                    // and keep the same state.
                }
                else
                {
                    // Parse the token starting at index, returning the
                    // token's start and end index in tokenInfo, along with
                    // the token's type and color to use.
                    // If the token is a quoted string and the string continues
                    // on the next line, set state to InQuotes.
                    // If the token is a comment and the comment continues
                    // on the next line, set state to InComment.
                    bFoundToken = true;
                }
            }
            return bFoundToken;
        }

        /////////////////////////////////////////////////////
        // IScanner methods

        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo,
                                                   ref int state)
        {
            bool bFound = false;
            if (tokenInfo != null)
            {
                bFound = GetNextToken(m_offset, tokenInfo, ref state);
                if (bFound)
                {
                    m_offset = tokenInfo.EndIndex + 1;
                }
            }
            return false;
            return bFound;
        }

        public void SetSource(string source, int offset)
        {
            m_offset = offset;
            m_source = source;
        }

    }

    internal class LispAuthoringScope : AuthoringScope
    {
        public override string GetDataTipText(int line, int col, out TextSpan span)
        {
            span = new TextSpan();
            return null;
        }

        public override Declarations
            GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
        {
            return null;
        }

        public override string Goto(VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
        {
            span = new TextSpan();
            return null;
        }

        public override Methods GetMethods(int line, int col, string name)
        {
            return null;
        }
    }
}
