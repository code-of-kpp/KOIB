using System; 
namespace Croc.Bpc.Recognizer.Ocr 
{ 
    public enum GeoResult 
    { 
        OK = 0, 
        TopMarker = -1, 
        BottomMarker = -2, 
        Markers = -3, 
        BadMarkerNum = -4, 
        LeftSide = -5, 
        BaseLine = -6, 
        BadBaseLine = -7, 
        Squares = -8, 
    }; 
}
