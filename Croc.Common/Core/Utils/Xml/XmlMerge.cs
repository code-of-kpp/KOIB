using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Text; 
using System.Xml; 
namespace Croc.Core.Utils.Xml 
{ 
    public class XmlMerge 
    { 
        private XmlDocument _sourceXmlDoc; 
        private XmlDocument _targetXmlDoc; 
        public XmlDocument Result 
        { 
            get 
            { 
                return _targetXmlDoc; 
            } 
        } 
        public readonly List<string> InvariableElementXpaths; 
        public readonly Dictionary<string, string> KeyAttributeNames; 
        public XmlMerge() 
            : this(new List<string>(), new Dictionary<string, string>()) 
        { 
        } 
        public XmlMerge(List<string> invariableElementXpaths, Dictionary<string, string> keyAttributeNames) 
        { 
            CodeContract.Requires(invariableElementXpaths != null); 
            CodeContract.Requires(keyAttributeNames != null); 
            InvariableElementXpaths = invariableElementXpaths; 
            KeyAttributeNames = keyAttributeNames; 
        } 
        public bool Merge(string sourceXml, string targetXml) 
        { 
            return Merge(LoadXmlDocument(sourceXml), LoadXmlDocument(targetXml)); 
        } 
        private static XmlDocument LoadXmlDocument(string xml) 
        { 
            var doc = new XmlDocument(); 
            doc.LoadXml(xml); 
            return doc; 
        } 
        public bool Merge(XmlDocument sourceXmlDoc, XmlDocument targetXmlDoc) 
        { 
            CodeContract.Requires(sourceXmlDoc != null); 
            CodeContract.Requires(targetXmlDoc != null); 
            _sourceXmlDoc = sourceXmlDoc; 
            _targetXmlDoc = targetXmlDoc; 
            return Merge( 
                _sourceXmlDoc.DocumentElement,  
                GetExactNodeXPath("", _targetXmlDoc.DocumentElement, 1), 
                GetNodeXPath("", _targetXmlDoc.DocumentElement.Name)); 
        } 
        #region Реализация слияния 
        private string GetExactNodeXPath(string parentNodeXPath, XmlElement node, int nodePosition) 
        { 
            if (!KeyAttributeNames.ContainsKey(node.Name)) 
                return string.Format("{0}/{1}[{2}]", parentNodeXPath, node.Name, nodePosition); 
            var attName = KeyAttributeNames[node.Name]; 
            var att = node.Attributes[attName]; 
            if (att == null) 
                throw new Exception(string.Format("Не найден ключевой атрибут '{0}' для узла '{1}'",  
                    attName, node.Name)); 
            return string.Format("{0}/{1}[@{2}='{3}']", parentNodeXPath, node.Name, attName, att.Value); 
        } 
        private string GetNodeXPath(string parentNodeXPath, string nodeName) 
        { 
            return string.Format("{0}/{1}", parentNodeXPath, nodeName); 
        } 
        private bool Merge(XmlElement srcNode, string exactSrcNodeXPath, string srcNodeXPath) 
        { 
            if (IsInvariableElementXpath(srcNodeXPath)) 
                return false; 
            var trgNode = GetTargetNode(exactSrcNodeXPath); 
            if (trgNode == null) 
                return false; 
            var merged = false; 
            foreach (XmlAttribute srcAtt in srcNode.Attributes) 
            { 
                var trgAtt = GetTargetNodeAttribute(trgNode, srcAtt.Name); 
                if (string.CompareOrdinal(srcAtt.Value, trgAtt.Value) != 0) 
                { 
                    trgAtt.Value = srcAtt.Value; 
                    merged = true; 
                } 
            } 
            var childElementDict = new Dictionary<string, List<XmlElement>>(); 
            foreach (XmlNode childSrcNode in srcNode.ChildNodes) 
            { 
                switch (childSrcNode.NodeType) 
                { 
                    case XmlNodeType.Element: 
                        if (!childElementDict.ContainsKey(childSrcNode.Name)) 
                            childElementDict[childSrcNode.Name] = new List<XmlElement>(); 
                        childElementDict[childSrcNode.Name].Add((XmlElement)childSrcNode); 
                        break; 
                    case XmlNodeType.Text: 
                        if (string.CompareOrdinal(srcNode.InnerText, trgNode.InnerText) != 0) 
                        { 
                            trgNode.InnerText = srcNode.InnerText; 
                            merged = true; 
                        } 
                        break; 
                } 
            } 
            foreach (var item in childElementDict) 
                for (int i = 0; i < item.Value.Count; i++) 
                { 
                    var elem = item.Value[i]; 
                    var exactXPath = GetExactNodeXPath(exactSrcNodeXPath, elem, i + 1); 
                    var xpath = GetNodeXPath(srcNodeXPath, item.Key); 
                    if (Merge(elem, exactXPath, xpath)) 
                        merged = true; 
                } 
            return merged; 
        } 
        private XmlElement GetTargetNode(string xpath) 
        { 
            var node = _targetXmlDoc.SelectSingleNode(xpath); 
            if (node != null) 
                return node is XmlElement ? (XmlElement)node : null; 
            var lastPathDelimiterIndex = xpath.LastIndexOf('/'); 
            var nodeName = xpath.Substring(lastPathDelimiterIndex + 1); 
            var bracket = nodeName.IndexOf('['); 
            if (bracket != -1) 
                nodeName = nodeName.Substring(0, bracket); 
            node = _targetXmlDoc.CreateElement(nodeName); 
            if (lastPathDelimiterIndex == 0) 
            { 
                _targetXmlDoc.AppendChild(node); 
            } 
            else 
            { 
                var parentNodeXPath = xpath.Substring(0, lastPathDelimiterIndex); 
                var parentNode = GetTargetNode(parentNodeXPath); 
                parentNode.AppendChild(node); 
            } 
            return (XmlElement)node; 
        } 
        private bool IsInvariableElementXpath(string xpath) 
        { 
            foreach (var invariableXpath in InvariableElementXpaths) 
                if (xpath.StartsWith(invariableXpath)) 
                    return true; 
            return false; 
        } 
        private XmlAttribute GetTargetNodeAttribute(XmlElement trgNode, string attName) 
        { 
            var att = trgNode.Attributes[attName]; 
            if (att == null) 
            { 
                att = _targetXmlDoc.CreateAttribute(attName); 
                trgNode.SetAttributeNode(att); 
            } 
            return att; 
        } 
        #endregion 
    } 
}
