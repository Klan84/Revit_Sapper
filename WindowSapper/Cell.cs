using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindowSapper
{
    public class Cell : INotifyPropertyChanged
    {
        private bool isRevealed;
        public bool IsRevealed
        {
            get => isRevealed;
            set
            {
                isRevealed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
                OnPropertyChanged(nameof(BackgroundColor));
                OnPropertyChanged(nameof(ForegroundColor));
            }
        }

        private int number;
        public int Number
        {
            get => number;
            set 
            { 
                number = value; 
                OnPropertyChanged(); 
            }
        }
        public int Position { get; set; }
        public string DisplayText
        {
            get
            {
                if (!IsRevealed) return "";
                if (Number == 9) return "💣";
                return Number > 0 ? Number.ToString() : "";
            }
        }
        private string? backgroundColor;
        public string? BackgroundColor
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
                OnPropertyChanged();
            }
        }
        private string? foregroundColor;
        public string? ForegroundColor
        {
            get => foregroundColor;
            set
            {
                foregroundColor = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
