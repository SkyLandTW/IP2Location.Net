namespace IP2Location.Net
{
    internal struct BinarySearchResult
    {
        public readonly bool Found;
        public readonly int Index;

        public BinarySearchResult(bool found, int index)
        {
            Found = found;
            Index = index;
        }
    }
}