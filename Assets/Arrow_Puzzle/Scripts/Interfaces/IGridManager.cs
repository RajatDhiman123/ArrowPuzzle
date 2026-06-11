using ArrowPuzzle.Data;

namespace ArrowPuzzle.Interfaces
{
    public interface IGridManager
    {
        void GenerateGrid(int width, int height);
        void ClearGrid();
        bool CanMove(int x, int y, ArrowDirection direction);
        void ClearCell(int x, int y);
    }
}
