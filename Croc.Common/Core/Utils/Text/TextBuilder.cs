using System; 
using System.Text; 
namespace Croc.Core.Utils.Text 
{ 
    public class TextBuilder 
    { 
        private readonly StringBuilder _builder; 
        private int _indent; 
        public TextBuilder() : this(new StringBuilder()) 
        { 
        } 


        public TextBuilder(StringBuilder builder) 
        { 
            _builder = builder; 
        } 
        public TextBuilder Append(String sValue) 
        { 
            _builder.Append(sValue); 
            return this; 
        } 
        public TextBuilder BeginLine(String sValue) 
        { 
            if (_indent > 0) 
                _builder.Append('\t', _indent); 
            _builder.Append(sValue); 
            return this; 
        } 
        public TextBuilder EndLine(String sValue) 
        { 
            _builder.Append(sValue); 
            _builder.Append(Environment.NewLine); 
            return this; 
        } 
        public TextBuilder Line(String sValue) 
        { 
            if (_indent > 0) 
                _builder.Append('\t', _indent); 
            _builder.Append(sValue); 
            _builder.Append(Environment.NewLine); 
            return this; 
        } 
        public TextBuilder FormatLine(String sValue, object arg0) 
        { 
            if (_indent > 0) 
                _builder.Append('\t', _indent); 
            _builder.AppendFormat(sValue, arg0); 
            _builder.Append(Environment.NewLine); 
            return this; 
        } 
        public TextBuilder EmptyLine() 
        { 
            _builder.Append(Environment.NewLine); 
            return this; 
        } 
        public TextBuilder IncreaseIndent() 
        { 
            ++_indent; 
            return this; 
        } 
        public TextBuilder DecreaseIndent() 
        { 
            if (_indent == 0) 
                throw new InvalidOperationException("Текущий отступ уже равен 0"); 
            --_indent; 
            return this; 
        } 
        public override string ToString() 
        { 
            return _builder.ToString(); 
        } 
    } 
}
