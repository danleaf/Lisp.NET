using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VisualLisp.Editor
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("plaintext")]
    [Name("LispCompletion")]
    public class LispCompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            //Entity<LinkedList<Token>> tokenizedBuffer = TokenizedBufferBuilder.TokenizedBuffers[textBuffer];
            return new LispCompletionSource(this, textBuffer);
        }
    }

    public class LispCompletionSource : ICompletionSource
    {
        private LispCompletionSourceProvider sourceProvider;
        private ITextBuffer textBuffer;
        private List<Completion> compList;

        public LispCompletionSource(LispCompletionSourceProvider sourceProvider, ITextBuffer textBuffer)
        {
            this.sourceProvider = sourceProvider;
            this.textBuffer = textBuffer;
        }

        public void Dispose()
        {
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            List<string> strList = new List<string>();
            strList.Add("addition");
            strList.Add("adaptation");
            strList.Add("subtraction");
            strList.Add("summation");
            strList.Add("bubtraction");
            strList.Add("cummation");
            compList = new List<Completion>();
            foreach (string str in strList)
                compList.Add(new Completion(str, "sdfsf", "dsfsf", (ImageSource)null, "sddsf"));

            completionSets.Add(new CompletionSet(
                "Tokens",    //the non-localized title of the tab
                "Tokens",    //the display title of the tab
                FindTokenSpanAtPosition(session.GetTriggerPoint(textBuffer),
                    session),
                compList,
                null));
        }

        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession session)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            ITextStructureNavigator navigator = sourceProvider.NavigatorService.GetTextStructureNavigator(textBuffer);
            TextExtent extent = navigator.GetExtentOfWord(currentPoint);
            return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }
    }
}
