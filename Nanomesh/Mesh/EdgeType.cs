namespace Nanomesh
{
    public enum NodeTopology
    {
        Surface = 0,
        Hard = 1,
        Border = 2
    }

    public enum EdgeTopology
    {
        Unknown,
        /// <summary>
        /// \       /
        ///  \_____/
        ///  /  ^  \
        /// /       \
        /// </summary>
        SurfacicSmooth,
        /// <summary>
        /// ______
        ///   /\  |
        ///  / >\ |
        /// /____\|
        /// \    /|
        /// </summary>
        SurfacicBorderAB,
        /// <summary>
        /// _________
        ///  \  |A /
        ///   \>| /
        /// ___\|/___
        ///    /|\B
        ///   / | \
        /// </summary>
        SurfacicBorderA,
        /// <summary>
        /// _________
        ///  \  |B /
        ///   \>| /
        /// ___\|/___
        ///    /|\A
        ///   / | \
        /// </summary>
        SurfacicBorderB,
        /// <summary>
        /// _________
        ///  \  |A /
        ///   \>| /
        /// ___\|/___
        ///    /|\B (hard edge here)
        ///   / | \
        /// </summary>
        SurfacicBorderAHardB,
        /// <summary>
        /// _________
        ///  \  |B /
        ///   \>| /
        /// ___\|/___
        ///    /|\A (hard edge here)
        ///   / | \
        /// </summary>
        SurfacicBorderBHardA,
        /// <summary>
        /// \       /
        ///  \_____/ (edge is smooth but A and B are connected to hard edges)
        ///  /A ^  \B
        /// /       \ 
        /// </summary>
        SurfacicHardAB,
        /// <summary>
        /// \       /
        ///  \_____/ (hard edge on both ends)
        ///  /A ^  \B
        /// /       \
        /// </summary>
        SurfacicHardEdge,
        /// <summary>
        /// \       /
        ///  \_____/A (hard edge here)
        ///  /  ^  \
        /// /       \
        /// </summary>
        SurfacicHardA,
        /// <summary>
        /// \       /
        ///  \_____/B (hard edge here)
        ///  /  ^  \
        /// /       \
        /// </summary>
        SurfacicHardB,
        /// <summary>
        ///  A        B
        /// _____________
        ///  /\  ^   /\
        /// /  \    /  \
        ///     \  /
        ///      \/
        /// </summary>
        BorderAB
    }
}