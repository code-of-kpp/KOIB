using System; 
using System.Xml.Serialization; 
namespace Croc.Bpc.Printing.Reports.Templates 
{ 
    [Serializable] 
    public abstract class FontDefinition : BasePlainElement 
    { 
        [XmlAttribute("fontSize")] 
        public string RelativeFontSize; 
        [XmlIgnore] 
        public string FontSize 
        { 
            get 
            { 
                return RelativeFontSize; 
            } 
        } 
        [XmlAttribute("bold")] 
        public bool IsBold; 
        [XmlAttribute("italic")] 
        public bool IsItalic; 
        [XmlAttribute("align")] 
        public LineAlign AlignFromTemplate; 
        [XmlIgnore] 
        private bool _emptyAlign; 
        [XmlIgnore]  
        public LineAlign? Align 
        { 
            get 
            { 
                return _emptyAlign ? (LineAlign?)null : AlignFromTemplate; 
            }  
            set 
            { 
                if(value.HasValue) 
                { 
                    AlignFromTemplate = value.Value; 
                    _emptyAlign = false; 
                } 
                else 
                { 
                    _emptyAlign = true; 
                } 
            } 
        } 
    } 
}
