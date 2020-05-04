namespace Nanolabo
{
    public interface IEdgeType
    {
        /// <summary>
        /// \       /
        ///  \_____/
        ///  /  ^  \
        /// /       \
        /// </summary>
        public struct SURFACIC : IEdgeType { }

        /// <summary>
        /// ______
        ///   /\  |
        ///  / >\ |
        /// /____\|
        /// \    /|
        /// </summary>
        public struct SURFACIC_BORDER_AB : IEdgeType { }

        /// <summary>
        /// _________
        ///  \  |A /
        ///   \>| /
        /// ___\|/___
        ///    /|\B
        ///   / | \
        /// </summary>
        public struct SURFACIC_BORDER_A : IEdgeType { }

        /// <summary>
        /// _________
        ///  \  |B /
        ///   \>| /
        /// ___\|/___
        ///    /|\A
        ///   / | \
        /// </summary>
        public struct SURFACIC_BORDER_B : IEdgeType { }

        /// <summary>
        /// _________
        ///  \  |A /
        ///   \>| /
        /// ___\|/___
        ///    /|\B (hard edge here)
        ///   / | \
        /// </summary>
        public struct SURFACIC_BORDER_A_HARD_B : IEdgeType { }

        /// <summary>
        /// _________
        ///  \  |B /
        ///   \>| /
        /// ___\|/___
        ///    /|\A (hard edge here)
        ///   / | \
        /// </summary>
        public struct SURFACIC_BORDER_B_HARD_A : IEdgeType { }

        /// <summary>
        /// \       /
        ///  \_____/ (hard edge on both ends)
        ///  /  ^  \
        /// /       \
        /// </summary>
        public struct SURFACIC_HARD_AB : IEdgeType { }

        public struct SURFACIC_HARD_EDGE : IEdgeType { }

        /// <summary>
        /// \       /
        ///  \_____/A (hard edge here)
        ///  /  ^  \
        /// /       \
        /// </summary>
        public struct SURFACIC_HARD_A : IEdgeType { }

        /// <summary>
        /// \       /
        ///  \_____/B (hard edge here)
        ///  /  ^  \
        /// /       \
        /// </summary>
        public struct SURFACIC_HARD_B : IEdgeType { }

        /// <summary>
        ///  A        B
        /// _____________
        ///  /\  ^   /\
        /// /  \    /  \
        ///     \  /
        ///      \/
        /// </summary>
        public struct BORDER_AB : IEdgeType
        {
            public int borderNodeA;
            public int borderNodeB;
        }

        /// <summary>
        /// This case should normally never happen
        /// </summary>
        public struct UNKNOWN : IEdgeType { }
    }
}
