using System; 

 

 

namespace Croc.Bpc.Recognizer 

{ 

    /// <summary> 

    /// Типы сохранения изображения 

    /// </summary> 

    public enum ImageSavingType 

    { 

        /// <summary> 

        ///	не сохранять  

        /// </summary> 

        None = 0, 

        /// <summary> 

        ///	сохранять бинар 

        /// </summary> 

        Binary = 1, 

        /// <summary> 

        ///	сохранять полутон 

        /// </summary> 

        Halftone = 2, 

        /// <summary> 

        ///	сохранять бинар и полутон 

        /// </summary> 

        All = 3 

    } 

}


