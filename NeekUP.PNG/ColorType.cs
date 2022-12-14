namespace NeekUP.PNG;

[Flags]
public enum ColorType : byte
{
    /// <summary>
    /// Greyscale: each pixel consists of a single sample: grey. The alpha channel may be represented by a single pixel value
    /// as in the previous case. If the alpha channel is not represented in this way, all pixels are fully opaque.
    /// </summary>
    Greyscale = 0,
    /// <summary>
    /// Truecolour: each pixel consists of three samples: red, green, and blue.
    /// The alpha channel may be represented by a single pixel value.
    /// Matching pixels are fully transparent, and all others are fully opaque.
    /// If the alpha channel is not represented in this way, all pixels are fully opaque.
    /// </summary>
    Truecolour = 2,
    /// <summary>
    /// Indexed-colour: each pixel consists of an index into a palette (and into an associated table of alpha values, if present).
    /// </summary>
    IndexedColour = 3,
    /// <summary>
    /// Greyscale with alpha: each pixel consists of two samples: grey and alpha.
    /// </summary>
    GreyscaleA = 4,
    /// <summary>
    /// Truecolour with alpha: each pixel consists of four samples: red, green, blue, and alpha.
    /// </summary>
    TruecolourA = 6
}