using System; 

using Croc.Core.Configuration; 

using System.Configuration; 

 

 

namespace Croc.Bpc.Recognizer.Config 

{ 

    /// <summary> 

    /// Конфиг-элемент менеджера распознавания 

    /// </summary> 

    public class RecognitionManagerConfig : SubsystemConfig 

    { 

        /// <summary> 

        /// Запустить GC.Collect сразу после распознавания бюллетеня 

        /// </summary> 

        [ConfigurationProperty("GCCollect", IsRequired = true)] 

        public EnabledConfig GCCollect 

        { 

            get 

            { 

                return (EnabledConfig)this["GCCollect"]; 

            } 

            set 

            { 

                this["GCCollect"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Минимальный размер памяти под изображение 

        /// </summary> 

		[ConfigurationProperty("MinFreeSpaceForImageKb", IsRequired = true)] 

		public ValueConfig<int> MinFreeSpaceForImageKb 

        { 

            get 

            { 

				return (ValueConfig<int>)this["MinFreeSpaceForImageKb"]; 

            } 

            set 

            { 

				this["MinFreeSpaceForImageKb"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Разрешено ли использовать печати вышестоящих комиссий 

        /// </summary> 

        [ConfigurationProperty("superiorStamp", IsRequired = true)] 


        public EnabledConfig SuperiorStamp 

        { 

            get 

            { 

                return (EnabledConfig)this["superiorStamp"]; 

            } 

            set 

            { 

                this["superiorStamp"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Настройки отладочного сохранения изображений 

        /// </summary> 

        [ConfigurationProperty("debugImageSaving", IsRequired = true)] 

        public DebugImageSavingConfig DebugImageSaving 

        { 

            get 

            { 

                return (DebugImageSavingConfig)this["debugImageSaving"]; 

            } 

            set 

            { 

                this["debugImageSaving"] = value; 

            } 

        } 

 

 

        /// <summary> 

        /// Настройки обработки бланков в зависимости от типа бланка 

        /// </summary> 

        [ConfigurationProperty("blankProcessing", IsDefaultCollection = false, IsRequired = true)] 

        [ConfigurationCollection(typeof(BlankConfig), AddItemName = "blank")] 

        public BlankConfigCollection Blanks 

        { 

            get 

            { 

                return (BlankConfigCollection)base["blankProcessing"]; 

            } 

        } 

 

 

        /// <summary> 

        /// Настройки драйвера распознавалки 

        /// </summary> 

        [ConfigurationProperty("ocr", IsRequired = true)] 

        public OcrConfig Ocr 

        { 


            get 

            { 

                return (OcrConfig)this["ocr"]; 

            } 

            set 

            { 

                this["ocr"] = value; 

            } 

        } 

    } 

}


