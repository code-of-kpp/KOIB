using System; 
namespace Croc.Bpc.Utils.Images.Tiff 
{ 
    public struct TiffImageInfo 
    { 
        public UInt32 width; 
        public UInt32 height; 
        public UInt16 bps; 
        public UInt16 spp; 
        public Photometric photometric; 
        public PlanarConfig config; 
    } 
    public enum FieldName : int 
    { 
        IMAGEWIDTH = 256, 
        IMAGELENGTH = 257, 
        BITSPERSAMPLE = 258, 
        COMPRESSION = 259, 
        PHOTOMETRIC = 262, 
        THRESHHOLDING = 263, 
        CELLWIDTH = 264, 
        CELLLENGTH = 265, 
        FILLORDER = 266, 
        DOCUMENTNAME = 269, 
        IMAGEDESCRIPTION = 270, 
        MAKE = 271, 
        MODEL = 272, 
        STRIPOFFSETS = 273, 
        ORIENTATION = 274, 
        SAMPLESPERPIXEL = 277, 
        ROWSPERSTRIP = 278, 
        STRIPBYTECOUNTS = 279, 
        MINSAMPLEVALUE = 280, 
        MAXSAMPLEVALUE = 281, 
        XRESOLUTION = 282, 
        YRESOLUTION = 283, 
        PLANARCONFIG = 284, 
        RESOLUTIONUNIT = 296 
    } 
    public enum CompressionType : int 
    { 
        NONE = 1, 
        CCITTRLE = 2, 
        CCITTFAX3 = 3, 
        CCITTFAX4 = 4, 
        LZW = 5, 
        OJPEG = 6, 
        JPEG = 7, 
        NEXT = 32766, 
        CCITTRLEW = 32771, 
        PACKBITS = 32773, 
        THUNDERSCAN = 32809, 
        IT8CTPAD = 32895, 
        IT8LW = 32896, 
        IT8MP = 32897, 
        IT8BL = 32898, 
        PIXARFILM = 32908, 
        PIXARLOG = 32909, 
        DEFLATE = 32946, 
        DCS = 32947, 
        JBIG = 34661 
    } 
    public enum Photometric : int 
    { 
        MINISWHITE = 0, 
        MINISBLACK = 1, 
        RGB = 2, 
        PALETTE = 3, 
        MASK = 4, 
        SEPARATED = 5, 
        YCBCR = 6, 
        CIELAB = 8 
    } 
    public enum Treesholding : int 
    { 
        BILEVEL = 1, 
        HALFTONE = 2, 
        ERRORDIFFUSE = 3, 
    } 
    public enum Fillorder : int 
    { 
        MSB2LSB = 1, 
        LSB2MSB = 2 
    } 
    public enum Orientation : int 
    { 
        TOPLEFT = 1, 
        TOPRIGHT = 2, 
        BOTRIGHT = 3, 
        BOTLEFT = 4, 
        LEFTTOP = 5, 
        RIGHTTOP = 6, 
        RIGHTBOT = 7, 
        LEFTBOT = 8 
    } 
    public enum PlanarConfig : int 
    { 
        CONTIG = 1, 
        SEPARATE = 2 
    } 
    public enum ResolutionUnit : int 
    { 
        NONE = 1, 
        INCH = 2, 
        CENTIMETER = 3 
    } 
}
