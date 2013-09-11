using System; 

using System.Xml; 

 

 

namespace Croc.Workflow.ComponentModel.Compiler 

{ 

    /// <summary> 

    /// Расширеный XmlReader. Добавлено управление глубиной. 

    /// </summary> 

    public class XmlReaderEx : XmlReader, IXmlLineInfo 

    { 

        private XmlReader _reader; 

        private int _currentDepth; 

        private bool _moveToNextElementOnCurrentDepthFailed = false; 

 

 

        public new static XmlReaderEx Create(string inputUri, XmlReaderSettings settings) 

        { 

            var reader = XmlReader.Create(inputUri, settings); 

            return new XmlReaderEx(reader); 

        } 

 

 

        private XmlReaderEx(XmlReader reader) 

        { 

            CodeContract.Requires(reader != null); 

 

 

            _reader = reader; 

            _currentDepth = _reader.Depth; 

        } 

 

 

        /// <summary> 

        /// Перейти к первому элементу 

        /// </summary> 

        /// <returns></returns> 

        public bool MoveToFirstElement() 

        { 

            if (!MoveToNextElement()) 

                return false; 

 

 

            _currentDepth = _reader.Depth; 

            return true; 

        } 

 

 

 

 


        /// <summary> 

        /// Подняться на глубину выше 

        /// </summary> 

        public void UpDepth() 

        { 

            if (_currentDepth == 0) 

                return; 

 

 

            _currentDepth--; 

        } 

 

 

        /// <summary> 

        /// Опуститься на глубину ниже 

        /// </summary> 

        public void DownDepth() 

        { 

            _currentDepth++; 

        } 

 

 

        /// <summary> 

        /// Перейти к следующему элементу на текущей глубине вложенности 

        /// </summary> 

        /// <returns></returns> 

        public bool MoveToNextElementOnCurrentDepth() 

        { 

            // если до этого была неудачная попытка перейти к элементу на текущей или более низкой глубине 

            if (_moveToNextElementOnCurrentDepthFailed) 

            { 

                // если глубина ридера соотв. текущей 

                if (_reader.Depth == _currentDepth) 

                { 

                    // то значит ридер уже перешел к требуемому элементу 

                    _moveToNextElementOnCurrentDepthFailed = false; 

                    return true; 

                } 

 

 

                // иначе - ридер перешел к более высокому элементу 

                return false; 

            } 

 

 

            // иначе, будем переходить до тех пор, пока не перейдем к след. элементу  

            // на глубине не ниже текущей 

            while (MoveToNextElement() && _reader.Depth > _currentDepth) 

            { 

            } 


 
 

            // если нашли элемент на глубине более высокой 

            if (_reader.Depth < _currentDepth) 

            { 

                // то говорим, что перейти не можем 

                _moveToNextElementOnCurrentDepthFailed = true; 

                return false; 

            } 

 

 

            // иначе - успешно перешли, если элементы не кончились 

            return !_reader.EOF; 

        } 

 

 

        /// <summary> 

        /// Перейти к следующему элементу 

        /// </summary> 

        /// <returns></returns> 

        private bool MoveToNextElement() 

        { 

            while (_reader.Read() && _reader.NodeType != XmlNodeType.Element) 

            { 

            } 

 

 

            return !_reader.EOF; 

        } 

 

 

        #region XmlReader 

        public override int AttributeCount 

        { 

            get { return _reader.AttributeCount; } 

        } 

 

 

        public override string BaseURI 

        { 

            get { return _reader.BaseURI; } 

        } 

 

 

        public override void Close() 

        { 

            _reader.Close(); 

        } 

 

 


        public override int Depth 

        { 

            get { return _reader.Depth; } 

        } 

 

 

        public override bool EOF 

        { 

            get { return _reader.EOF; } 

        } 

 

 

        public override string GetAttribute(int i) 

        { 

            return _reader.GetAttribute(i); 

        } 

 

 

        public override string GetAttribute(string name, string namespaceURI) 

        { 

            return _reader.GetAttribute(name, namespaceURI); 

        } 

 

 

        public override string GetAttribute(string name) 

        { 

            return _reader.GetAttribute(name); 

        } 

 

 

        public override bool HasValue 

        { 

            get { return _reader.HasValue; } 

        } 

 

 

        public override bool IsEmptyElement 

        { 

            get { return _reader.IsEmptyElement; } 

        } 

 

 

        public override string LocalName 

        { 

            get { return _reader.LocalName; } 

        } 

 

 

        public override string LookupNamespace(string prefix) 

        { 


            return _reader.LookupNamespace(prefix); 

        } 

 

 

        public override bool MoveToAttribute(string name, string ns) 

        { 

            return _reader.MoveToAttribute(name, ns); 

        } 

 

 

        public override bool MoveToAttribute(string name) 

        { 

            return _reader.MoveToAttribute(name); 

        } 

 

 

        public override bool MoveToElement() 

        { 

            return _reader.MoveToElement(); 

        } 

 

 

        public override bool MoveToFirstAttribute() 

        { 

            return _reader.MoveToFirstAttribute(); 

        } 

 

 

        public override bool MoveToNextAttribute() 

        { 

            return _reader.MoveToNextAttribute(); 

        } 

 

 

        public override XmlNameTable NameTable 

        { 

            get { return _reader.NameTable; } 

        } 

 

 

        public override string NamespaceURI 

        { 

            get { return _reader.NamespaceURI; } 

        } 

 

 

        public override XmlNodeType NodeType 

        { 

            get { return _reader.NodeType; } 

        } 


 
 

        public override string Prefix 

        { 

            get { return _reader.Prefix; } 

        } 

 

 

        public override bool Read() 

        { 

            return _reader.Read(); 

        } 

 

 

        public override bool ReadAttributeValue() 

        { 

            return _reader.ReadAttributeValue(); 

        } 

 

 

        public override ReadState ReadState 

        { 

            get { return _reader.ReadState; } 

        } 

 

 

        public override void ResolveEntity() 

        { 

            _reader.ResolveEntity(); 

        } 

 

 

        public override string Value 

        { 

            get { return _reader.Value; } 

        } 

        #endregion 

 

 

        #region IXmlLineInfo Members 

 

 

        public bool HasLineInfo() 

        { 

            var lineInfo = _reader as IXmlLineInfo; 

            return lineInfo != null ? lineInfo.HasLineInfo() : false; 

        } 

 

 

        public int LineNumber 


        { 

            get 

            { 

                var lineInfo = _reader as IXmlLineInfo; 

                return lineInfo != null ? lineInfo.LineNumber : 0; 

            } 

        } 

 

 

        public int LinePosition 

        { 

            get 

            { 

                var lineInfo = _reader as IXmlLineInfo; 

                return lineInfo != null ? lineInfo.LinePosition : 0; 

            } 

        } 

 

 

        #endregion 

    } 

}


