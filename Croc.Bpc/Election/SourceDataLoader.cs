using System; 

using System.IO; 

using System.Xml; 

using System.Xml.Schema; 

using System.Xml.Serialization; 

using System.Threading; 

using System.Text; 

using System.Collections; 

using System.Collections.Generic; 

using System.Collections.Specialized; 

using System.Reflection; 

using System.Diagnostics; 

using System.Text.RegularExpressions; 

using System.Security.Cryptography; 

using Croc.Bpc.Election.Voting; 

using Croc.Core.Utils; 

using System.Linq; 

 

 

namespace Croc.Bpc.Election 

{ 

    /// <summary> 

    /// ????????? ?? 

    /// </summary> 

    public static class SourceDataLoader 

    { 

        /// <summary> 

        /// ??? ????? ???????? ?????? 

        /// </summary> 

        public const string SOURCEDATA_XMLNS = "http://localhost/Schemas/SIB2003/SourceData"; 

        /// <summary> 

        /// ???? ? ????? ?? ?????? 

        /// </summary> 

		public const string SOURCEDATA_SHEMA_PATH = @"Data/Schemas/SourceData.xsd"; 

 

 

        /// <summary> 

        /// ???????? ???????? ?????? ?? ????? 

        /// </summary> 

        /// <returns>??????????? ??????</returns> 

        public static SourceData LoadDataFromFile(string sourceDataFilePath) 

        { 

            try 

            { 

                using (var stream = new FileStream(sourceDataFilePath, FileMode.Open, FileAccess.Read)) 

                { 

                    // ???????????? ?????? 

                    var uncompressedStream = ZipCompressor.Uncompress(stream); 

                    // ???????? ?? 

                    var sd = DeserializeSourceData(uncompressedStream); 


                    uncompressedStream.Close(); 

 

 

                    // ???? ?? ??? ?????????????? ???????? ??? 

                    if(sd.Id == Guid.Empty) 

                        sd.Id = GenerateSourceDataId(sd); 

 

 

                    return sd; 

                } 

            } 

            catch (Exception ex) 

            { 

                throw new Exception("?????? ???????? ???????? ?????? ?? ?????: " + sourceDataFilePath, ex); 

            } 

        } 

 

 

        /// <summary> 

        /// ?????????? ?????????? ?? ??? SourceData 

        /// </summary> 

        /// <param name="sd"></param> 

        /// <returns></returns> 

        public static Guid GenerateSourceDataId(SourceData sd) 

        { 

            var hash = ComputeSourceDataHashValue(sd); 

 

 

            // ??????? ?????? 16 ???? 

            byte[] bytes = Encoding.ASCII.GetBytes(hash).Take(16).ToArray(); 

            // ?????? ?? 

            return new Guid(bytes); 

        } 

 

 

        /// <summary> 

        /// ????????? ??? ??? ?? 

        /// </summary> 

        /// <param name="sd"></param> 

        /// <returns></returns> 

        public static string ComputeSourceDataHashValue(SourceData sd) 

        { 

            var data = SerializeSourceData(sd); 

 

 

            var sha1 = SHA1.Create(); 

            byte[] hashValue = sha1.ComputeHash(Encoding.ASCII.GetBytes(data)); 

            return Encoding.ASCII.GetString(hashValue); 

        } 

 


 
        /// <summary> 

        /// ????????????? ???????? ?????? ?? xml-?????? 

        /// </summary> 

        private static SourceData DeserializeSourceData(Stream uncompressedStream) 

        { 

            Stream replacedStream; 

			//??????? ?????? ?? c ???????? xmlns 

			using (var xmlTextReader = new XmlTextReader(uncompressedStream, XmlNodeType.Document, null)) 

			{ 

				replacedStream = GetReplaceXmlStream(xmlTextReader); 

				replacedStream.Position = 0; 

			} 

 

 

			//???????? ?? 

			using (var replaceTextReader = new XmlTextReader(replacedStream, XmlNodeType.Document, null)) 

			{ 

				// ???????? XmlReader ? ????????? ????? 

				var settings = new XmlReaderSettings(); 

				settings.Schemas.Add(SOURCEDATA_XMLNS, SOURCEDATA_SHEMA_PATH); 

				var xmlReader = XmlReader.Create(replaceTextReader, settings); 

 

 

				// ????????????? 

				var serializer = new XmlSerializer(typeof(SourceData), SOURCEDATA_XMLNS); 

				var sd = (SourceData)serializer.Deserialize(xmlReader); 

 

 

                // ?????????????? ?? 

                sd.Init(); 

 

 

				return sd; 

			} 

        } 

 

 

		/// <summary> 

		/// ?????? ???????? xml ? ???????????? ??? ? ????? ????? ? ??????? xmlns 

		/// </summary> 

		/// <param name="reader">???????? xml</param> 

		/// <returns>????? ? ???????</returns> 

		private static Stream GetReplaceXmlStream(XmlTextReader reader) 

		{ 

			MemoryStream result = new MemoryStream(); ; 

			XmlTextWriter writer = new XmlTextWriter(result, Encoding.Unicode); 

			while (reader.Read()) 

			{ 

				switch (reader.NodeType) 


				{ 

					case XmlNodeType.XmlDeclaration: 

						writer.WriteStartDocument(); 

						break; 

					case XmlNodeType.Whitespace: 

						writer.WriteWhitespace(reader.Value); 

						break; 

					case XmlNodeType.Element: 

						writer.WriteStartElement(reader.Name); 

						WriteXmlElementAttributes(reader, writer); 

						if (reader.IsEmptyElement) 

							writer.WriteEndElement(); 

						break; 

					case XmlNodeType.EndElement: 

						writer.WriteEndElement(); 

						break; 

					case XmlNodeType.CDATA: 

						writer.WriteCData(reader.Value); 

						break; 

					case XmlNodeType.Text: 

						writer.WriteValue(reader.Value); 

						break; 

				} 

			} 

			writer.WriteEndDocument(); 

			writer.Flush(); 

			return result; 

		} 

 

 

		/// <summary> 

		/// ?????????? ???????? reader, ??????? "www.croc.ru" ?? "localhost" 

		/// </summary> 

		/// <param name="reader">???????? xml</param> 

		/// <param name="writer">???? ?????</param> 

		private static void WriteXmlElementAttributes(XmlTextReader reader, XmlTextWriter writer) 

		{ 

			const string oldSourceDataXmlns = "www.croc.ru"; 

			while (reader.MoveToNextAttribute()) 

			{ 

				writer.WriteStartAttribute(reader.Name); 

				writer.WriteValue(reader.Value.Replace(oldSourceDataXmlns, "localhost")); 

				writer.WriteEndAttribute(); 

			} 

			reader.MoveToElement(); 

		} 

 

 

        /// <summary> 

        /// ??????????? ?? ? xml-?????? 


        /// </summary> 

        /// <param name="sd"></param> 

        /// <returns></returns> 

        private static string SerializeSourceData(SourceData sd) 

        { 

            // ???????????? 

            var oSerializer = new XmlSerializer(typeof(SourceData), SOURCEDATA_XMLNS); 

            using (var memStream = new MemoryStream()) 

            { 

                var writer = new StreamWriter(memStream); 

                oSerializer.Serialize(writer, sd); 

 

 

                memStream.Seek(0, SeekOrigin.Begin); 

 

 

                var reader = new StreamReader(memStream); 

                return reader.ReadToEnd(); 

            } 

        } 

    } 

}


