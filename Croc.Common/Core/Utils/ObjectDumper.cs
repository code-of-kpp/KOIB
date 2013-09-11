using System; 
using System.Collections; 
using System.Collections.Generic; 
using System.IO; 
using System.Linq; 
using System.Reflection; 
using Croc.Core.Utils.Text; 
namespace Croc.Core.Utils 
{ 
    public static class ObjectDumper 
    { 
        private class DumpContext 
        { 
            public ObjectDumperSettings Settings; 
            public TextBuilder Builder; 
            public Int32 Depth; 
            public Type RootType; 
            private Boolean _hasLines; 
            public void NewLine() 
            { 
                if (_hasLines) 
                    Builder.EmptyLine().BeginLine(String.Empty); 
                _hasLines = true; 
            } 
            public Boolean CanUseToStringForType(Type type) 
            { 
                return !Settings.DoNotUseToStringMethod || RootType != type; 
            } 
        } 
        public static String DumpObject(Object obj) 
        { 
            var builder = new TextBuilder(); 
            DumpObject(obj, builder); 
            return builder.ToString(); 
        } 
        public static String DumpObject(Object obj, ObjectDumperSettings settings) 
        { 
            var builder = new TextBuilder(); 
            DumpObject(obj, builder, settings ?? ObjectDumperSettings.Default); 
            return builder.ToString(); 
        } 
        public static void DumpObject(Object obj, TextBuilder builder) 
        { 
            DumpObject(obj, builder, ObjectDumperSettings.Default); 
        } 
        public static void DumpObject(Object obj, TextBuilder builder, ObjectDumperSettings settings) 
        { 
            var ctx = new DumpContext 
                          { 
                              Builder = builder, 
                            Settings = settings ?? ObjectDumperSettings.Default, 
                              RootType = (!settings.DoNotUseToStringMethod || obj == null) ? null : obj.GetType() 
                          }; 
            DumpObject(obj, /*bNeedTypeName*/false, ctx); 
        } 
        private static void DumpObject(Object obj, Boolean bNeedTypeName, DumpContext ctx) 
        { 
            if (obj == null) 
            { 
                ctx.Builder.Append("<NULL>"); 
                return; 
            } 
            Type type = obj.GetType(); 
            TypeCode typeCode = Type.GetTypeCode(type); 
            if (typeCode == TypeCode.String) 
            { 
                WriteObject(obj, type, ctx); 
                return; 
            } 
            if (typeCode != TypeCode.Object) 
            { 
                ctx.Builder.Append(ToStringSafe(obj)); 
                return; 
            } 
            if (ctx.Depth > ctx.Settings.MaxDepth) 
            { 
                WriteObject(obj, type, ctx); 
                return; 
            } 
            if (ctx.CanUseToStringForType(type)) 
            { 
                var toStringMethod = type.GetMethod( 
                    "ToString", 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, 
                    null, 
                    Type.EmptyTypes, 
                    null); 
                if (toStringMethod != null) 
                { 
                    WriteObject(obj, type, ctx); 
                    return; 
                } 
            } 
            if (typeof (Stream).IsAssignableFrom(type)) 
            { 
                WriteObject(obj, type, ctx); 
                return; 
            } 
            if (bNeedTypeName) 
                ctx.Builder.Append("{").Append(type.Name).Append("}: "); 
            if (ctx.Depth > 0) 
                ctx.Builder.IncreaseIndent(); 
            ctx.Depth++; 
            IEnumerable enumerable; 
            if ((enumerable = obj as IEnumerable) != null) 
                DumpEnumerable(enumerable, ctx); 
            else 
                DumpProps(obj, type, ctx); 
            ctx.Depth--; 
            if (ctx.Depth > 0) 
                ctx.Builder.DecreaseIndent(); 
        } 
        private static void DumpEnumerable(IEnumerable enumerable, DumpContext ctx) 
        { 
            Int32 nCount = 0; 
            Boolean isSimpleType = true; 
            Boolean isKeyValuePairs = false; 
            PropertyInfo keyProp = null; 
            PropertyInfo valueProp = null; 
            var type = (from object value in enumerable  
                        where value != null  
                        select value.GetType()).FirstOrDefault(); 
            if (type != null) 
            { 
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (KeyValuePair<,>)) 
                { 
                    keyProp = type.GetProperty("Key"); 
                    valueProp = type.GetProperty("Value"); 
                    isKeyValuePairs = true; 
                } 
                isSimpleType = Type.GetTypeCode(type) != TypeCode.Object; 
            } 
            foreach (Object value in enumerable) 
            { 
                if (!isSimpleType) 
                    ctx.NewLine(); 
                else if (nCount > 0) 
                    ctx.Builder.Append(ctx.Settings.EnumerableDelimiter); 
                if (nCount >= ctx.Settings.MaxEnumerableItems) 
                { 
                    ctx.Builder.Append("... (first ").Append(nCount.ToString()).Append(" items"); 
                    var collection = enumerable as ICollection; 
                    if (collection != null) 
                        ctx.Builder.Append(", ").Append(collection.Count.ToString()).Append(" items total"); 
                    ctx.Builder.Append(")"); 
                    break; 
                } 
                if (!isKeyValuePairs) 
                    DumpObject(value, true, ctx); 
                else 
                { 
                    ctx.Builder.Append(ToStringSafe(keyProp.GetValue(value, null))); 
                    ctx.Builder.Append(": "); 
                    DumpObject(valueProp.GetValue(value, null), false, ctx); 
                } 
                nCount++; 
            } 
            if (nCount == 0) 
                ctx.Builder.Append("<EMPTY>"); 
        } 
        private static void DumpProps(object obj, Type type, DumpContext ctx) 
        { 
            var propValues = new Dictionary<String, Object>(); 
            PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public); 
            foreach (var prop in props) 
            { 
                if (!prop.CanRead || prop.GetIndexParameters().Length != 0) 
                    continue; 
                if (ctx.Settings.PropsToIgnore.Contains(prop.Name)) 
                    continue; 
                try 
                { 
                    propValues[prop.Name] = prop.GetValue(obj, null); 
                } 
                catch (TargetInvocationException ex) 
                { 
                    propValues[prop.Name] = String.Format("Ошибка при получении свойства {0} объекта типа {1} : {2}", 
                        prop.Name, 
                        type.FullName, 
                        ex.InnerException != null ? ex.InnerException.Message : ex.Message 
                        ); 
                } 
            } 
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public); 
            foreach (var field in fields) 
            { 
                if (ctx.Settings.PropsToIgnore.Contains(field.Name)) 
                    continue; 
                try 
                { 
                    propValues[field.Name] = field.GetValue(obj); 
                } 
                catch (FieldAccessException ex) 
                { 
                    propValues[field.Name] = ex.Message; 
                } 
            } 
            if (propValues.Count == 0 || propValues.Count > ctx.Settings.MaxProps) 
                WriteObject(obj, type, ctx); 
            else 
            { 
                foreach (KeyValuePair<String, Object> pair in propValues) 
                { 
                    ctx.NewLine(); 
                    ctx.Builder.Append(pair.Key).Append(": "); 
                    DumpObject(pair.Value, false, ctx); 
                } 
            } 
        } 
        private static void WriteObject(Object obj, Type type, DumpContext ctx) 
        { 
            if (!ctx.CanUseToStringForType(type)) 
            { 
                ctx.Builder.Append(type.FullName); 
                return; 
            } 
            String text = ToStringSafe(obj); 
            String[] lines = text.Split(new[] {Environment.NewLine}, StringSplitOptions.None); 
            if (lines.Length <= 1) 
                ctx.Builder.Append(text); 
            else 
            { 
                ctx.Builder.IncreaseIndent(); 
                foreach (String line in lines) 
                { 
                    ctx.Builder.EmptyLine().BeginLine(line); 
                } 
                ctx.Builder.DecreaseIndent(); 
            } 
        } 
        private static String ToStringSafe(Object obj) 
        { 
            try 
            { 
                return obj.ToString(); 
            } 
            catch (Exception ex) 
            { 
                return "Ошибка при вызове ToString() для " + obj.GetType().FullName + " : " + ex.Message; 
            } 
        } 
    } 
}
