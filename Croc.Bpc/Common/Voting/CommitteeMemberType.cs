namespace Croc.Bpc.Voting 
{ 
    public enum CommitteeMemberType 
    { 
        [Printing.PresentationForReport("Председатель УИК")] 
        Chairman, 
        [Printing.PresentationForReport("Заместитель председателя УИК")] 
        ChairmanAssistant, 
        [Printing.PresentationForReport("Секретарь УИК")] 
        Secretary 
    } 
}
