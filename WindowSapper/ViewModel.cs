using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace WindowSapper
{
    public class ViewModel
    {
        public ObservableCollection<Cell> Cells { get; set; }
        public ICommand CellClickCommand { get; }

        public event EventHandler? RequestClose;
        private static readonly int maxRow = 9;
        private static readonly int maxCollumn = 9;
        private readonly int[,] _cellsValue = new int[maxRow + 1, maxCollumn + 1];
        private readonly int _mine = 15;
        private readonly string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Autodesk\");

        public ViewModel()
        {
            Cells = [];
            CellClickCommand = new RelayCommand(CellClick);
            FillingCells();
            InitializeField();
            SavePosition();
        }

        private void InitializeField()
        {
            Cells.Clear();
            for (int i = 0; i < 100; i++)
            {
                Cells.Add(new Cell
                {
                    Position = i,
                    IsRevealed = false,
                    Number = _cellsValue[i / 10, i % 10],
                    BackgroundColor = "Gray",
                    ForegroundColor = ForegroundValue(_cellsValue[i / 10, i % 10])
                });
            }
        }
        private static string ForegroundValue(int num)
        {
            if (num == 1)
                return "Blue";
            else if (num == 2)
                return "Green";
            else if (num == 3)
                return "Red";
            else if (num == 4)
                return "BlueViolet";
            else if (num == 5)
                return "Brown";
            else
                return "Black";
        }
        private void CellClick(object parameter)
        {
            if (parameter is Cell cell && !cell.IsRevealed)
            {
                OpenCells(cell);
                SavePosition();
                LoseGame(cell);
                WinGame();
            }
        }

        private void FillingCells()
        {
            RandomMine();
            RemainingCells();
        }
        private void RandomMine()
        {
            int i = 0;
            var random = new Random();
            while (i < _mine)
            {
                int row = random.Next(0, 10);
                int collumn = random.Next(0, 10);
                if (_cellsValue[row, collumn] == 9)
                    continue;
                _cellsValue[row, collumn] = 9;
                i++;
            }
        }
        private void RemainingCells()
        {
            for (int i = 0; i < 100; i++)
            {
                if (_cellsValue[i / 10, i % 10] == 9)
                {
                    RemainingCellsHelper(i / 10, i % 10);
                }
            }
        }
        private void RemainingCellsHelper(int row, int collumn)
        {
            if (row > 0 && _cellsValue[row - 1, collumn] != 9)
                _cellsValue[row - 1, collumn]++;
            if (collumn > 0 && _cellsValue[row, collumn - 1] != 9)
                _cellsValue[row, collumn - 1]++;
            if (row > 0 && collumn > 0 && _cellsValue[row - 1, collumn - 1] != 9)
                _cellsValue[row - 1, collumn - 1]++;
            if (row < maxRow && _cellsValue[row + 1, collumn] != 9)
                _cellsValue[row + 1, collumn]++;
            if (collumn < maxCollumn && _cellsValue[row, collumn + 1] != 9)
                _cellsValue[row, collumn + 1]++;
            if (row < maxRow && collumn < maxCollumn && _cellsValue[row + 1, collumn + 1] != 9)
                _cellsValue[row + 1, collumn + 1]++;
            if (row < maxRow && collumn > 0 && _cellsValue[row + 1, collumn - 1] != 9)
                _cellsValue[row + 1, collumn - 1]++;
            if (row > 0 && collumn < maxCollumn && _cellsValue[row - 1, collumn + 1] != 9)
                _cellsValue[row - 1, collumn + 1]++;
        }
        private void LoseGame(Cell cell)
        {
            if (cell.Number == 9)
            {
                foreach (var cellItem in Cells)
                {
                    if (cellItem.Number == 9)
                        cellItem.IsRevealed = true;
                }
                SavePosition();
                MessageBox.Show("Вы проиграли!");
                DeleteFile();
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
        }
        private void WinGame()
        {
            List<Cell> mineList = [];
            int searchIsRevealed = 0;
            foreach (var cellItem in Cells)
            {
                if (cellItem.IsRevealed)
                    searchIsRevealed++;
                else
                    mineList.Add(cellItem);
            }
            if (searchIsRevealed == 100 - _mine)
            {
                foreach (var cellItem in mineList)
                {
                    cellItem.IsRevealed = true;
                    cellItem.ForegroundColor = "White";
                    cellItem.BackgroundColor = "Green";
                }
                SavePosition();
                MessageBox.Show("Вы выиграли!");
                DeleteFile();
                RequestClose?.Invoke(this, EventArgs.Empty);
            }
        }
        private void OpenCells(Cell cell)
        {
            cell.IsRevealed = true;
            cell.BackgroundColor = (cell.Number == 9) ? "Red" : "Light Gray";
            if (cell.Number != 0)
                return;
            int row = cell.Position / 10;
            int collumn =cell.Position % 10;
            if (row > 0 && _cellsValue[row - 1, collumn] != 9 && !Cells.First(x => x.Position == cell.Position - 10).IsRevealed)
                OpenCells(Cells.First(x => x.Position == cell.Position - 10));
            if (collumn > 0 && _cellsValue[row, collumn - 1] != 9 && !Cells.First(x => x.Position == cell.Position - 1).IsRevealed)
                OpenCells(Cells.First(x => x.Position == cell.Position - 1));
            if (row > 0 && collumn > 0 && _cellsValue[row - 1, collumn - 1] != 9 && !Cells.First(x => x.Position == cell.Position - 11).IsRevealed)
                OpenCells(Cells.First(x => x.Position == cell.Position - 11));
            if (row < maxRow && _cellsValue[row + 1, collumn] != 9 && !Cells.First(x => x.Position == cell.Position + 10).IsRevealed)
                OpenCells(Cells.First(x => x.Position == cell.Position + 10));
            if (collumn < maxCollumn && _cellsValue[row, collumn + 1] != 9 && !Cells.First(x => x.Position == cell.Position + 1).IsRevealed)
                OpenCells(Cells.First(x => x.Position == cell.Position + 1));
            if (row < maxRow && collumn < maxCollumn && _cellsValue[row + 1, collumn + 1] != 9 && !Cells.First(x => x.Position == cell.Position + 11).IsRevealed)
                OpenCells(Cells.First(x => x.Position == cell.Position + 11));
            if (row < maxRow && collumn > 0 && _cellsValue[row + 1, collumn - 1] != 9 && !Cells.First(x => x.Position == cell.Position + 9).IsRevealed)
                OpenCells(Cells.First(x => x.Position == cell.Position + 9));
            if (row > 0 && collumn < maxCollumn && _cellsValue[row - 1, collumn + 1] != 9 && !Cells.First(x => x.Position == cell.Position - 9).IsRevealed)
                OpenCells(Cells.First(x => x.Position == cell.Position - 9));
        }
        private void SavePosition()
        {
            int[] array = new int[100];
            foreach (var cellItem in Cells)
            {
                if (!cellItem.IsRevealed)
                    array[cellItem.Position] = -1;
                else
                    array[cellItem.Position] = cellItem.Number;
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllText(Path.Combine(path, @"setting.db"), string.Join(",", array));
        }
        public void DeleteFile()
        {
            if (File.Exists(Path.Combine(path, @"setting.db")))
            {
                File.Delete(Path.Combine(path, @"setting.db"));
            }
        }
    }
}
