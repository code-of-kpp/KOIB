using System; 

using System.Collections.Generic; 

using System.Text; 

 

 

namespace Croc.Bpc.Printing.Reports.Templates 

{ 

    /// <summary> 

    /// Колонки сервисной таблицы 

    /// </summary> 

    public class ServiceTableColumns 

    { 

        /// <summary> 

        /// Префикс сервисного столбца 

        /// </summary> 

        public const string SERVICE_COLUMN_PREFIX = "_"; 

        /// <summary> 

        /// имя столбца 

        /// </summary> 

        public const string Name = SERVICE_COLUMN_PREFIX + "Name"; 

 

 

        /// <summary> 

        /// Ширина столбца 

        /// </summary> 

        public const string Width = SERVICE_COLUMN_PREFIX + "Width"; 

 

 

        /// <summary> 

        /// Размер шрифта 

        /// </summary> 

        public const string FontSize = SERVICE_COLUMN_PREFIX + "FontSize"; 

 

 

        /// <summary> 

        /// Признак печати жирным шрифтом 

        /// </summary> 

        public const string IsBold = SERVICE_COLUMN_PREFIX + "Bold"; 

 

 

        /// <summary> 

        /// Признак печати наклонным шрифтом 

        /// </summary> 

        public const string IsItalic = SERVICE_COLUMN_PREFIX + "Italic"; 

 

 

        /// <summary> 

        /// Признак переноса таблицы на новую страницу 

        /// </summary> 

        public const string NewPage = SERVICE_COLUMN_PREFIX + "NewPage"; 


    } 

}


