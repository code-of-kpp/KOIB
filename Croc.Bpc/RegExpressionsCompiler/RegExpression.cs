using System.Text.RegularExpressions; 
using Croc.Core.Utils.Text.RegExpressions; 
namespace Croc.Bpc.RegExpressionsCompiler 
{ 
    public enum RegExpression 
    { 
        [RegExpression(@"(\[\d+\w*\])|\[S\]|\[M\]|\[P\]|S|M|P")] 
        CheckExpressionLineReferenceRegex, 
        [RegExpression(@"\A(\w|-)*\z")] 
        SourceDataModeFileSuffixRegex, 
        [RegExpression("{Chairman:[_]*}|{ChairmanAssistant:[_]*}|{Secretary:[_]*}")] 
        UikMemberNamesRegex, 
        [RegExpression(@"^\w+-(?<uik>\d{1,4})\.bin$")] 
        SourceDataFileNameRegex, 
        [RegExpression(@"^p((?<IndexAll>\*)|(?<IndexFrom>\d+)(-(?<IndexTo>\*|\d+))?)(:(?<Modifier>\w+)(\[(?<ModifierAtt>.+)\])?)?$")] 
        ParseParameter, 
        [RegExpression(@"(\{@?\w*=((\d+)|(\w*))(,\w*=((\d+)|(\w*)))*\})|(\[(M|S|P|(\w+\.\w+))\])")] 
        VotingCountReferenceRegex, 
        [RegExpression(@"\A\{@?(\s*(Scanner|VotingMode|Candidate|BlankType|Election|Blank|Type)\s*=\s*(\d+|\w+)\s*(\,|(\}$)))+\Z")] 
        LineRestrictionRegex, 
        [RegExpression(@"=(\d+|\w+)")] 
        LineRestrictionValueRegex, 
        [RegExpression(@"(\{|\,)\s*Scanner\s*=\s*\d+")] 
        LineRestrictionScannerRegex, 
        [RegExpression(@"(\{|\,)\s*VotingMode\s*=\s*\w+")] 
        LineRestrictionVotingModeRegex, 
        [RegExpression(@"(\{|\,)\s*Type\s*=\s*\w+")] 
        LineRestrictionTypeRegex, 
        [RegExpression(@"(\{|\,)\s*Candidate\s*=\s*\d+")] 
        LineRestrictionCandidateRegex, 
        [RegExpression(@"(\{|\,)\s*BlankType\s*=\s*\w+")] 
        LineRestrictionBlankTypeRegex, 
        [RegExpression(@"(\{|\,)\s*Election\s*=\s*\d+")] 
        LineRestrictionElectionRegex, 
        [RegExpression(@"(\{|\,)\s*Blank\s*=\s*\d+")] 
        LineRestrictionBlankRegex, 
        [RegExpression(@"\s+|\n|\r")] 
        CtrlSimbolsRegex, 
        [RegExpression(@"\{\$([^}]*)\}", Options = RegexOptions.IgnoreCase | RegexOptions.Singleline)] 
        ReportTemplateVariableRegex, 
        [RegExpression(@"\$([^;]*)", Options = RegexOptions.IgnoreCase | RegexOptions.Singleline)] 
        ReportTemplateVariableInFormatStrRegex, 
    } 
}
