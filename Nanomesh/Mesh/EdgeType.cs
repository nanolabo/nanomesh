namespace Nanomesh
{
    public interface IEdgeType
    {

    }

    /// <summary>
    /// \       /
    ///  \_____/
    ///  /  ^  \
    /// /       \
    /// </summary>
    public readonly struct SURFACIC : IEdgeType { }

    /// <summary>
    /// ______
    ///   /\  |
    ///  / >\ |
    /// /____\|
    /// \    /|
    /// </summary>
    public readonly struct SURFACIC_BORDER_AB : IEdgeType { }

    /// <summary>
    /// _________
    ///  \  |A /
    ///   \>| /
    /// ___\|/___
    ///    /|\B
    ///   / | \
    /// </summary>
    public readonly struct SURFACIC_BORDER_A : IEdgeType { }

    /// <summary>
    /// _________
    ///  \  |B /
    ///   \>| /
    /// ___\|/___
    ///    /|\A
    ///   / | \
    /// </summary>
    public readonly struct SURFACIC_BORDER_B : IEdgeType { }

    /// <summary>
    /// _________
    ///  \  |A /
    ///   \>| /
    /// ___\|/___
    ///    /|\B (hard edge here)
    ///   / | \
    /// </summary>
    public readonly struct SURFACIC_BORDER_A_HARD_B : IEdgeType { }

    /// <summary>
    /// _________
    ///  \  |B /
    ///   \>| /
    /// ___\|/___
    ///    /|\A (hard edge here)
    ///   / | \
    /// </summary>
    public readonly struct SURFACIC_BORDER_B_HARD_A : IEdgeType { }

    /// <summary>
    /// \       /
    ///  \_____/ (hard edge on both ends)
    ///  /  ^  \
    /// /       \
    /// </summary>
    public readonly struct SURFACIC_HARD_AB : IEdgeType { }

    public readonly struct SURFACIC_HARD_EDGE : IEdgeType { }

    /// <summary>
    /// \       /
    ///  \_____/A (hard edge here)
    ///  /  ^  \
    /// /       \
    /// </summary>
    public readonly struct SURFACIC_HARD_A : IEdgeType { }

    /// <summary>
    /// \       /
    ///  \_____/B (hard edge here)
    ///  /  ^  \
    /// /       \
    /// </summary>
    public readonly struct SURFACIC_HARD_B : IEdgeType { }

    /// <summary>
    ///  A        B
    /// _____________
    ///  /\  ^   /\
    /// /  \    /  \
    ///     \  /
    ///      \/
    /// </summary>
    public readonly struct BORDER_AB : IEdgeType
    {
        public readonly int borderNodeA;
        public readonly int borderNodeB;

        public BORDER_AB(in int borderNodeA, in int borderNodeB)
        {
            this.borderNodeA = borderNodeA;
            this.borderNodeB = borderNodeB;
        }
    }

    /// <summary>
    /// This case should normally never happen
    /// </summary>
    public readonly struct UNKNOWN : IEdgeType { }
}
