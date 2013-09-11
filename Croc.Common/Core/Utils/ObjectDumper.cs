using System; 

using System.Collections; 

using System.Collections.Generic; 

using System.IO; 

using System.Linq; 

using System.Reflection; 

using Croc.Core.Utils.Text; 

using Croc.Core.Diagnostics.Default; 

 

 

namespace Croc.Core.Utils 

{ 

	/// <summary> 

	/// ??????? ?????????? ?? ??????? ? ????????? ??? ??????? ? ???? ?????? 

	/// </summary> 

	public static class ObjectDumper 

	{ 

		private class DumpContext 

		{ 

			public ObjectDumperSettings Settings; 

			public TextBuilder Builder; 

			public Int32 Depth; 

			public Type RootType; 

 

 

			private Boolean m_bHasLines; 

 

 

			public void NewLine() 

			{ 

				if (m_bHasLines) 

					Builder.EmptyLine().BeginLine(String.Empty); 

				m_bHasLines = true; 

			} 

 

 

			public Boolean CanUseToStringForType(Type type) 

			{ 

				return !Settings.DoNotUseToStringMethod || RootType != type; 

			} 

		} 

 

 

		/// <summary> 

		/// ?????????? ????????? ????????????? ???????. 

		/// </summary> 

		/// <param name="obj"></param> 

		public static String DumpObject(Object obj) 

		{ 

			var builder = new TextBuilder(); 


			DumpObject(obj, builder); 

			return builder.ToString(); 

		} 

 

 

        /// <summary> 

        /// ?????????? ????????? ????????????? ???????. 

        /// </summary> 

        /// <param name="obj"></param> 

        /// <param name="settings"></param> 

        public static String DumpObject(Object obj, ObjectDumperSettings settings) 

        { 

            var builder = new TextBuilder(); 

            DumpObject(obj, builder, settings ?? ObjectDumperSettings.Default); 

            return builder.ToString(); 

        } 

 

 

        /// <summary> 

		/// ????????? ? <paramref name="builder"/> ?????????? ?? ??????? 

		/// </summary> 

		/// <param name="obj"></param> 

		/// <param name="builder"></param> 

		public static void DumpObject(Object obj, TextBuilder builder) 

		{ 

			DumpObject(obj, builder, ObjectDumperSettings.Default); 

		} 

 

 

		/// <summary> 

		/// ????????? ? <paramref name="builder"/> ?????????? ?? ???????, ????????? ???????? ????????? 

		/// </summary> 

		/// <param name="obj"></param> 

		/// <param name="builder"></param> 

		/// <param name="settings"></param> 

		public static void DumpObject(Object obj, TextBuilder builder, ObjectDumperSettings settings) 

		{ 

			var ctx = new DumpContext 

			          	{ 

			          		Builder = builder, 

                            Settings = settings ?? ObjectDumperSettings.Default, 

			          		RootType = (!settings.DoNotUseToStringMethod || obj == null) ? null : obj.GetType() 

			          	}; 

			dumpObject(obj, /*bNeedTypeName*/false, ctx); 

		} 

 

 

		public static String DumpWcfObject(Object obj) 

		{ 

			var builder = new TextBuilder(); 


			var settings = new ObjectDumperSettings 

			{ 

				DoNotUseToStringMethod = true, 

				PropsToIgnore = new[] { "ExtensionData" } 

			}; 

			DumpObject(obj, builder, settings); 

			return builder.ToString(); 

		} 

 

 

		private static void dumpObject(Object obj, Boolean bNeedTypeName, DumpContext ctx) 

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

				writeObject(obj, type, ctx); 

				return; 

			} 

 

 

			if (typeCode != TypeCode.Object) 

			{ 

				ctx.Builder.Append(toStringSafe(obj)); 

				return; 

			} 

 

 

			if (ctx.Depth > ctx.Settings.MaxDepth) 

			{ 

				writeObject(obj, type, ctx); 

				return; 

			} 

 

 

			if (ctx.CanUseToStringForType(type)) 

			{ 

				// ???? ????? ToString(), ??????????? ??????????????? ? ?????????????? ???? 

				var toStringMethod = type.GetMethod( 

					"ToString", 

					BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, 

					null, 

					Type.EmptyTypes, 


					null); 

 

 

				if (toStringMethod != null) 

				{ 

					writeObject(obj, type, ctx); 

					return; 

				} 

			} 

 

 

			// ?? ????? ????? ?????? ???????, ??????????? ?? Stream 

			if (typeof (Stream).IsAssignableFrom(type)) 

			{ 

				writeObject(obj, type, ctx); 

				return; 

			} 

 

 

			if (bNeedTypeName) 

				ctx.Builder.Append("{").Append(type.Name).Append("}: "); 

 

 

			if (ctx.Depth > 0) 

				ctx.Builder.IncreaseIndent(); 

			ctx.Depth++; 

 

 

			IEnumerable enumerable; 

			if ((enumerable = obj as IEnumerable) != null) 

				dumpEnumerable(enumerable, ctx); 

			else 

				dumpProps(obj, type, ctx); 

 

 

			ctx.Depth--; 

			if (ctx.Depth > 0) 

				ctx.Builder.DecreaseIndent(); 

		} 

 

 

		private static void dumpEnumerable(IEnumerable enumerable, DumpContext ctx) 

		{ 

			Int32 nCount = 0; 

			Boolean isSimpleType = true; 

			Boolean isKeyValuePairs = false; 

			PropertyInfo keyProp = null; 

			PropertyInfo valueProp = null; 

 

 


			// ???? ?????? ?? null ??????? ? ?? ???? ?????????? ??????????? ????????? 

			foreach (Object value in enumerable) 

			{ 

				if (value == null) 

					continue; 

 

 

				Type type = value.GetType(); 

				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)) 

				{ 

					keyProp = type.GetProperty("Key"); 

					valueProp = type.GetProperty("Value"); 

					isKeyValuePairs = true; 

				} 

				isSimpleType = Type.GetTypeCode(type) != TypeCode.Object; 

				break; 

			} 

 

 

			foreach (Object value in enumerable) 

			{ 

				// ??????????? 

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

					dumpObject(value, true, ctx); 

				else 

				{ 

					ctx.Builder.Append(toStringSafe(keyProp.GetValue(value, null))); 

					ctx.Builder.Append(": "); 

					dumpObject(valueProp.GetValue(value, null), false, ctx); 

				} 

 

 

				nCount++; 


			} 

 

 

			if (nCount == 0) 

				ctx.Builder.Append("<EMPTY>"); 

		} 

 

 

		private static void dumpProps(object obj, Type type, DumpContext ctx) 

		{ 

			IDictionary<String, Object> propValues = new Dictionary<String, Object>(); 

			PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public); 

			foreach (PropertyInfo prop in props) 

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

                    propValues[prop.Name] = String.Format("?????? ??? ????????? ???????? {0} ??????? ???? {1} : {2}", 

                        prop.Name, 

                        type.FullName, 

                        ex.InnerException != null ? ex.InnerException.Message : ex.Message 

                        ); 

                } 

			} 

 

 

			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public); 

			foreach (FieldInfo field in fields) 

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


				writeObject(obj, type, ctx); 

			else 

			{ 

				foreach (KeyValuePair<String, Object> pair in propValues) 

				{ 

					ctx.NewLine(); 

					ctx.Builder.Append(pair.Key).Append(": "); 

					dumpObject(pair.Value, false, ctx); 

				} 

			} 

		} 

 

 

		private static void writeObject(Object obj, Type type, DumpContext ctx) 

		{ 

			if (!ctx.CanUseToStringForType(type)) 

			{ 

				ctx.Builder.Append(type.FullName); 

				return; 

			} 

 

 

			String text = toStringSafe(obj); 

 

 

			// ???? ????????? ?????? ToString() - ????????????? ?????, ?? ??????? ??? ? ????????? 

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

 

 

		private static String toStringSafe(Object obj) 

		{ 

			try 

			{ 

				return obj.ToString(); 

			} 

			catch (Exception ex) 

			{ 

				return "?????? ??? ?????? ToString() ??? " + obj.GetType().FullName + " : " + ex.Message; 


			} 

		} 

	} 

}


