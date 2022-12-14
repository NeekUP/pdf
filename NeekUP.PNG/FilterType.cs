namespace NeekUP.PNG;


public enum FilterType
{
    /// <summary>
    /// Filt(x) = Orig(x)
    /// </summary>
    None = 0,
    /// <summary>
    /// Filt(x) = Orig(x) - Orig(a)
    /// </summary>
    Sub = 1,
    /// <summary>
    /// Filt(x) = Orig(x) - Orig(b)
    /// </summary>
    Up = 2,
    /// <summary>
    /// Filt(x) = Orig(x) - floor((Orig(a) + Orig(b)) / 2)
    /// </summary>
    Avg = 3,
    /// <summary>
    /// Filt(x) = Orig(x) - PaethPredictor(Orig(a), Orig(b), Orig(c))
    /// </summary>
    Paeth = 4
}