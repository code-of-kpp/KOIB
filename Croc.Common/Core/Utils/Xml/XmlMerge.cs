using System; 

using System.Collections.Generic; 

using System.Linq; 

using System.Text; 

using System.Xml; 

 

 

namespace Croc.Core.Utils.Xml 

{ 

    /// <summary> 

    /// Реализация слияния двух Xml-документов 

    /// </summary> 

    /// <remarks> 

    /// Принцип слияния: 

    /// - на вход подаются  

    ///     sourceXml - откуда брать данные 

    ///     targetXml - куда вносить данные 

    /// - в результате получается xml, который содержит: 

    ///     - все узлы из targetXml со значениями: 

    ///         - исходными (т.е. из targetXml), если в sourceXml такого узла нет или  

    ///             его значение не отличается от значения в targetXml 

    ///         - взятыми из sourceXml, если значение из targetXml отличается от значения из sourceXml 

    ///     - все узлы из sourceXml, которых нет в targetXml 

    /// - слияние реализовано для узлов с типами: Element, Text, Attribute, 

    ///     причем для узлов типа Text подразумевается, что текст, содержащийся в элементе,  

    ///     является единственным содержимым этого элемента 

    /// - если targetXml содержит список элементов с одинаковыми именами и нужно изменить, например, 

    ///     только содержимое одного элемента из списка, то в sourceXml должны быть перечислены все  

    ///     эти элементы с их значениями в таком же порядке, как в targetXml, а значение нужного элемента  

    ///     изменено на требуемое. 

    ///     Например, если требуется изменить значение элемента 'b', который идет 2-м по порядку, то: 

    ///     sourceXml = "<a><b>1</b><b>xxx</b></a>" 

    ///     targetXml = "<a><b>1</b><b>2</b></a>" 

    /// </remarks> 

    public class XmlMerge 

    { 

        /// <summary> 

        /// Xml-документ источник 

        /// </summary> 

        private XmlDocument _sourceXmlDoc; 

        /// <summary> 

        /// Xml-документ приемник 

        /// </summary> 

        private XmlDocument _targetXmlDoc; 

 

 

        /// <summary> 

        /// Полученный в результате слияния Xml-документ 

        /// </summary> 

        public XmlDocument Result 


        { 

            get 

            { 

                return _targetXmlDoc; 

            } 

        } 

 

 

        /// <summary> 

        /// Список XPath-ов элементов Xml-документа приемника,  

        /// которые при слиянии не будет изменены в случае, если в Xml-документе источнике  

        /// есть такой же элемент, но с другим содержимым  

        /// (другие значения атрибутов, новые атрибуты, другое содержимое элемента) 

        /// </summary> 

        public readonly List<string> InvariableElementXpaths; 

 

 

        /// <summary> 

        /// Таблица имен атрибутов, которые нужно использовать как ключ при поиске узла-приемника: 

        /// [имя узла, имя ключевого атрибута узла]. 

        /// </summary> 

        /// <remarks> 

        /// Эту таблицу следует использовать при слиянии списка элементов с одинаковым именем. 

        /// Например, если 

        /// xml-приемник имеет вид 

        ///     <a> 

        ///         <b k="1">1</b> 

        ///         <b k="2">2</b> 

        ///         <b k="3">3</b> 

        ///     </a> 

        /// а xml-источник: 

        ///     <a> 

        ///         <b k="2">222</b> 

        ///     </a> 

        /// и требуется получить результат: 

        ///     <a> 

        ///         <b k="1">1</b> 

        ///         <b k="2">222</b> 

        ///         <b k="3">3</b> 

        ///     </a> 

        /// то есть два варианта как это сделать: 

        /// 1) переписать xml-источник так: 

        ///     <a> 

        ///         <b/> 

        ///         <b k="2">222</b> 

        ///         <b/> 

        ///     </a> 

        /// 2) добавить в KeyAttributeNames пару ["b", "k"] 

        /// </remarks> 

        public readonly Dictionary<string, string> KeyAttributeNames; 


 
 

        /// <summary> 

        /// Конструктор 

        /// </summary> 

 

 

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

 

 

        /// <summary> 

        /// Выполнить слияние в результате которого будет изменен целевой xml-документ 

        /// </summary> 

        /// <param name="sourceXml">Xml источник</param> 

        /// <param name="targetXml">Xml приемник</param> 

        /// <returns> 

        /// true - слияние выполнено,  

        /// false - в результате слияния Xml-документ приемник не был изменен 

        /// </returns> 

        public bool Merge(string sourceXml, string targetXml) 

        { 

            return Merge(LoadXmlDocument(sourceXml), LoadXmlDocument(targetXml)); 

        } 

 

 

        /// <summary> 

        /// Загрузить Xml-документ из xml-строки 

        /// </summary> 

        /// <param name="xml"></param> 

        /// <returns></returns> 

        private static XmlDocument LoadXmlDocument(string xml) 

        { 

            var doc = new XmlDocument(); 

            doc.LoadXml(xml); 

            return doc; 

        } 


 
 

        /// <summary> 

        /// Выполнить слияние в результате которого будет изменен целевой xml-документ 

        /// </summary> 

        /// <param name="sourceXml">Xml источник</param> 

        /// <param name="targetXml">Xml приемник</param> 

        /// <returns> 

        /// true - слияние выполнено,  

        /// false - в результате слияния Xml-документ приемник не был изменен 

        /// </returns> 

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

 

 

        /// <summary> 

        /// Получить точный xpath для поиска узла с заданным именем и заданной позицией. 

        /// Позиция используется в случае, если элементов с таким именем больше одного 

        /// </summary> 

        /// <param name="parentNodeXPath"></param> 

        /// <param name="nodeName"></param> 

        /// <param name="nodePosition">позиция узла, нумерация начинается с 1</param> 

        /// <returns></returns> 

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

 

 

        /// <summary> 

        /// Получить xpath для поиска узла с заданным именем 

        /// </summary> 

        /// <param name="parentNodeXPath"></param> 

        /// <param name="nodeName"></param> 

        /// <returns></returns> 

        private string GetNodeXPath(string parentNodeXPath, string nodeName) 

        { 

            return string.Format("{0}/{1}", parentNodeXPath, nodeName); 

        } 

 

 

        /// <summary> 

        /// Выполнить слияние для узла-источника по заданному пути 

        /// </summary> 

        /// <param name="srcNode"></param> 

        /// <param name="exactSrcNodeXPath">точный путь к узлу-источнику (включает номера позиций узлов)</param> 

        /// <param name="srcNodeXPath">путь к узлу</param> 

        /// <returns> 

        /// true - в результате слияния узел-приемник был изменен 

        /// false - в результате слияния узел-приемник изменен не был</returns> 

        private bool Merge(XmlElement srcNode, string exactSrcNodeXPath, string srcNodeXPath) 

        { 

            // если узел-источник - это неизменяемый элемент 

            if (IsInvariableElementXpath(srcNodeXPath)) 

                // то ничего не сливаем 

                return false; 

 

 

            var trgNode = GetTargetNode(exactSrcNodeXPath); 

            // если получить узел не удалось 

            if (trgNode == null) 

                return false; 

 

 

            var merged = false; 

 

 

            // переберем атрибуты 

            foreach (XmlAttribute srcAtt in srcNode.Attributes) 

            { 

                var trgAtt = GetTargetNodeAttribute(trgNode, srcAtt.Name); 

                // если значение целевого атрибута отличается от значения атрибута-источника 

                if (string.CompareOrdinal(srcAtt.Value, trgAtt.Value) != 0) 


                { 

                    // то перезапишем значение целевого атрибута 

                    trgAtt.Value = srcAtt.Value; 

                    merged = true; 

                } 

            } 

 

 

            // словарь дочерних узлов [имя узла, список узлов с одинаковым именем] 

            var childElementDict = new Dictionary<string, List<XmlElement>>(); 

 

 

            // переберем дочерние узлы 

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

                        // если содержимое отличается 

                        if (string.CompareOrdinal(srcNode.InnerText, trgNode.InnerText) != 0) 

                        { 

                            // то перезапишем содержимое целевого узла 

                            trgNode.InnerText = srcNode.InnerText; 

                            merged = true; 

                        } 

                        break; 

                } 

            } 

 

 

            // выполним слияние дочерних узлов 

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

 

 

        /// <summary> 

        /// Возвращает элемент по заданному пути из целевого xml. 

        /// Если элемент отсутствует, то он будет создан, а также будут созданы все 

        /// другие элементы, в которые вложен данный, если они отсутствуют 

        /// </summary> 

        /// <param name="xpath">путь для поиска узла</param> 

        /// <returns> 

        /// элемент, если узел был найден и он типа XmlElement или не был найден и был создан, 

        /// null - если узел был найден, но его тип не XmlElement 

        /// </returns> 

        private XmlElement GetTargetNode(string xpath) 

        { 

            var node = _targetXmlDoc.SelectSingleNode(xpath); 

 

 

            // если узел есть 

            if (node != null) 

                return node is XmlElement ? (XmlElement)node : null; 

 

 

            // узел не найден => создадим его 

 

 

            // получим индекс последнего разделителя в пути 

            var lastPathDelimiterIndex = xpath.LastIndexOf('/'); 

 

 

            // выделим из пути имя искомого узла 

            var nodeName = xpath.Substring(lastPathDelimiterIndex + 1); 

            var bracket = nodeName.IndexOf('['); 

            if (bracket != -1) 

                nodeName = nodeName.Substring(0, bracket); 

 

 

            // создадим узел 

            node = _targetXmlDoc.CreateElement(nodeName); 

 

 

            // если последний разделитель - первый символ в пути 

            if (lastPathDelimiterIndex == 0) 

            { 

                // значит добрались до самого начала пути 

                _targetXmlDoc.AppendChild(node); 


            } 

            else 

            { 

                // получим родительский узел 

                var parentNodeXPath = xpath.Substring(0, lastPathDelimiterIndex); 

                var parentNode = GetTargetNode(parentNodeXPath); 

                parentNode.AppendChild(node); 

            } 

 

 

            return (XmlElement)node; 

        } 

 

 

        /// <summary> 

        /// Является ли заданный xpath путем к неизменяемому элементу? 

        /// </summary> 

        /// <param name="xpath"></param> 

        /// <returns></returns> 

        private bool IsInvariableElementXpath(string xpath) 

        { 

            foreach (var invariableXpath in InvariableElementXpaths) 

                if (xpath.StartsWith(invariableXpath)) 

                    return true; 

 

 

            return false; 

        } 

 

 

        /// <summary> 

        /// Возвращает атрибут целевого узла с заданным именем. 

        /// Если атрибут отсутствует, то он будет создан. 

        /// </summary> 

        /// <param name="trgNode"></param> 

        /// <param name="attName"></param> 

        /// <returns></returns> 

        private XmlAttribute GetTargetNodeAttribute(XmlElement trgNode, string attName) 

        { 

            var att = trgNode.Attributes[attName]; 

            // если атрибут не найден 

            if (att == null) 

            { 

                // создадим его 

                att = _targetXmlDoc.CreateAttribute(attName); 

                trgNode.SetAttributeNode(att); 

            } 

 

 

            return att; 


        } 

 

 

        #endregion 

    } 

}


