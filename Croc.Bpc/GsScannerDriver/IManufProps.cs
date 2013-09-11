namespace Croc.Bpc.GsScannerDriver 

{ 

    /// <summary> 

    /// Интерфейс к мануфактурным параметрам 

    /// </summary> 

    public interface IManufProps 

    { 

        /// <summary> 

        /// Серийный номер сканера, считанный из параметров 

        /// </summary> 

        int SerialNumber { get; } 

 

 

        /// <summary> 

        /// Расстояние между линейками 

        /// </summary> 

        short ShiftLines { get; } 

    } 

}


