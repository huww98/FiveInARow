using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FiveInARow
{
    internal class Player : INotifyPropertyChanged
    {
        public Chessboard Chessboard { get; set; }
        public string Name { get; set; }

        private PlayerKind _kind;

        public PlayerKind Kind
        {
            get { return _kind; }
            set
            {
                _kind = value;
                NotifyPropertyChanged();
            }
        }

        public ChessPieceKind UsingChessPiece { get; set; }
        public bool IsPlaying { get { return timer.IsEnabled; } }

        private TimeSpan _usedTime = new TimeSpan();

        public TimeSpan UsedTime
        {
            get { return _usedTime; }
            set
            {
                _usedTime = value;
                NotifyPropertyChanged();
            }
        }

        private DateTime lastStartTime;
        private TimeSpan lastUsedTime;
        private DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

        public Player()
        {
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UsedTime += timer.Interval;
        }

        public void StartPlay()
        {
            lastStartTime = DateTime.Now;
            lastUsedTime = UsedTime;
            timer.Start();
            PlayStarted?.Invoke(this, EventArgs.Empty);
        }

        public bool PlaceChessPiece(int x, int y)
        {
            if (!IsPlaying)
            {
                throw new InvalidOperationException("不在游戏中的玩家不能落子");
            }
            timer.Stop();
            UsedTime = lastUsedTime + (DateTime.Now - lastStartTime);
            bool isWin = Chessboard.PlaceChessPiece(x, y);
            return isWin;
        }

        public event EventHandler PlayStarted;

        protected void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    internal enum PlayerKind
    {
        Human,
        AI
    }
}