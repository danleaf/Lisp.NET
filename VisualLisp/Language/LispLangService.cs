
using Lexer;
using Microsoft.FSharp.Collections;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
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
            return "Lisp File (*.ls) *.lss";
        }

        public override LanguagePreferences GetLanguagePreferences()
        {
            if (preferences == null)
            {
                preferences = new LanguagePreferences(this.Site, typeof(LispLangService).GUID, this.Name);

                if (preferences != null)
                {
                    preferences.Init();
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
        private int offset;
        private FSharpList<char> source = FSharpList<char>.Empty;

        Lexer.Lexer lexer = new Lexer.Lexer(
                            new List<Regex>(){
                                new Regex("delimiter", @"[\[\]{}\(\)]"),
                                new Regex("keyword", @"ns|class|defn|filed"),
                                new Regex("string", @"""([^""\r\n\\]*|\\.)*"""),
                                new Regex("errstring", @"""([^""\r\n\\]*|\\.)*"),
                                new Regex ("sepor",@";"),
                                new Regex("line", @"\r?\n"),
                                new Regex("blank", @"[ \t]+"),
                                new Regex("identifier",@"[a-zA-Z_][\w]*"),
                                new Regex("number",@"[0-9]+(\.[0-9]+)?"),
                                new Regex("point",@"\."),
                                new Regex("Error",@".")});

        public LispScanner(IVsTextBuffer buffer)
        {
            this.buffer = buffer;
        }
        
        public void SetSource(string source, int offset)
        {
            this.source = ListModule.OfArray(source.Substring(offset).ToCharArray());
            this.offset = offset;
        }

        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            if (tokenInfo == null)
                return false;

            var ret = lexer.GetNextToken(source);
            if (ret.Length == 0)
            {
                return false;
            }
            else
            {
                tokenInfo.StartIndex = offset;
                tokenInfo.EndIndex = offset + ret.Length - 1;
                tokenInfo.Type = TokenType.Keyword;
                if (ret.Token == "string" || ret.Token == "errstring")
                    tokenInfo.Color = TokenColor.String;
                else if (ret.Token == "number")
                    tokenInfo.Color = TokenColor.Number;
                else if (ret.Token == "keyword")
                    tokenInfo.Color = TokenColor.Keyword;
                else
                    tokenInfo.Color = TokenColor.Text;
                offset += ret.Length;
                source = ret.Rest;
                return true;
            }
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
