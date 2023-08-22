using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnakeRemake.Models;
using System.Windows.Media;
using System.Windows;
using Caliburn.Micro;
using static SnakeRemake.Models.Snake;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Shapes;
using static SnakeRemake.ViewModels.GameViewModel;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Data.SqlClient;
using System.Threading;
using System.Timers;

namespace SnakeRemake.ViewModels
{
    public class GameViewModel  : Conductor<object>
    {
        private Random random = new Random();
        private Cloud _cloud;
        private Point _cloudPosition;
        private SolidColorBrush _cloudRandomColor;


        private Snake _snake;
        private Point _snakePosition;
        private SolidColorBrush _snakeBodyColour;
        private List<SolidColorBrush> _snakeBodyColours { get; set; } = new List<SolidColorBrush>()
        {
            new SolidColorBrush(Colors.Red),
            new SolidColorBrush(Colors.Green),
        };


        public enum SnakeDirection { Left, Right, Up, Down };
        private SnakeDirection _snakeInitialDirection { get; set; } = SnakeDirection.Right;


        private ObservableCollection<Snake> _snakeParts = new ObservableCollection<Snake>();
        private ObservableCollection<Cloud> _clouds=new ObservableCollection<Cloud>();
        private System.Timers.Timer _gameTimer;
        private HashSet<Point> occupiedPositions = new HashSet<Point>();


        private int _score;
        private int minScore { get; set; }
        private string _playerName ;
        private int num { get; set; } = 0;
        private int _speed { get; set; }
        private string _selectedDifficulty = "Normal";


        public GameViewModel(IEventAggregator eventAggregator)
        {
            _gameTimer = new System.Timers.Timer();
            _gameTimer.Elapsed += GameTimer_Tick;
            _snake = new Snake();
            _cloud = new Cloud();

        }

        public Snake Snake
        {
            get { return _snake; }
            set { _snake = value; NotifyOfPropertyChange(nameof(Snake)); }
        }

        public Point SnakePosition
        {
            get { return _snakePosition; }
            set { _snakePosition = value; NotifyOfPropertyChange(nameof(SnakePosition)); }
        }
        public SolidColorBrush SnakeBodyColour
        {
            get { return _snakeBodyColour; }
            set { _snakeBodyColour = value; NotifyOfPropertyChange(nameof(SnakeBodyColour)); }
        }

        public ObservableCollection<Snake> SnakeParts
        {
            get { return _snakeParts; }
            set { _snakeParts = value; NotifyOfPropertyChange(nameof(SnakeParts)); }
        }

        public SnakeDirection SnakeInitialDirection
        {
            get { return _snakeInitialDirection; }
            set { _snakeInitialDirection = value; NotifyOfPropertyChange(nameof(SnakeInitialDirection)); }
        }

        public string SelectedDifficulty
        {
            get { return _selectedDifficulty; }
            set
            {
                _selectedDifficulty = value;
                NotifyOfPropertyChange(() => SelectedDifficulty);
            }
        }

        public int Score
        {
            get { return _score; }
            set { _score = value; NotifyOfPropertyChange(nameof(Score)); }
        }
        public string PlayerName
        {
            get { return _playerName; }
            set 
            { 
                _playerName = value; 
                NotifyOfPropertyChange(nameof(PlayerName)); 
                NotifyOfPropertyChange(nameof(CanSave)); 
            }
        }
        public ObservableCollection<Cloud> Clouds
        {
            get { return _clouds; }
            set { _clouds = value; NotifyOfPropertyChange(nameof(Clouds)); }
        }
        public ObservableCollection<string> Difficulty { get; } = new ObservableCollection<string>
        {
            "Easy",
            "Normal",
            "Hard"
        };


        public Cloud Cloud
        {
            get { return _cloud; }
            set { _cloud = value; NotifyOfPropertyChange(nameof(Cloud)); }
        }

        public Point CloudPosition
        {
            get { return _cloudPosition; }
            set { _cloudPosition = value; NotifyOfPropertyChange(nameof(CloudPosition)); }
        }

        

        public SolidColorBrush CloudRandomColor
        {
            get { return _cloudRandomColor; }
            set { _cloudRandomColor = RandomColor(); NotifyOfPropertyChange(nameof(CloudRandomColor)); }
        }

        private SolidColorBrush RandomColor()
        {
            byte r = (byte)random.Next(0, 256);
            byte g = (byte)random.Next(0, 256);
            byte b = (byte)random.Next(0, 256);
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
        private void UpdateCloud()
        {
            foreach (var snakePart in SnakeParts)
            {
                occupiedPositions.Add(snakePart.SnakePosition);
            }

            foreach (var cloud in Clouds)
            {
                occupiedPositions.Add(cloud.CloudPosition);
            }


            List<Point> availablePositions = new List<Point>();

            for (int x = 0; x <= 380; x += 20)
            {
                for (int y = 20; y <= 400; y += 20)
                {
                    Point position = new Point(x, y);

                    if (!occupiedPositions.Contains(position))
                    {
                        availablePositions.Add(position);
                    }
                }
            }

            if (availablePositions.Count > 0)
            {
                int randomIndex = random.Next(availablePositions.Count);
                Point randomCloudPosition = availablePositions[randomIndex];

                Clouds.Add(new Cloud()
                {
                    CloudPosition = randomCloudPosition,
                    CloudRandomColor = RandomColor(),
                });
            }
        }
        private void AddSnakeBodyPart(Point position)
        {

            if (SnakeParts.Count < Snake.InitialLength)
            {
                Snake newBodyPart = new Snake
                {
                    SnakePosition = position,
                };
                SnakeParts.Insert(0, newBodyPart);
            }
            else
            {
                SnakeParts.RemoveAt(SnakeParts.Count - 1);
                Snake newBodyPart = new Snake
                {
                    SnakePosition = position,
                };
                SnakeParts.Insert(0, newBodyPart);
            }

            for (int i = 0; i < SnakeParts.Count; i++)
            {
                SnakeParts[i].SnakeBodyColor = _snakeBodyColours[i];
            }
        }

        private void MoveSnake()
        {
            switch (SnakeInitialDirection)
            {
                case SnakeDirection.Left:
                    _snakePosition.X -= Snake.SnakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    _snakePosition.X += Snake.SnakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    _snakePosition.Y -= Snake.SnakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    _snakePosition.Y += Snake.SnakeSquareSize;
                    break;
            }
            num++;
            AddSnakeBodyPart(SnakePosition);
            CollisionCheck();
        }
        public void ChangeDirection(KeyEventArgs key)
        {
            if (_gameTimer.Enabled)
            {
                switch (key.Key)
                {
                    case Key.W:
                        if (SnakeInitialDirection != SnakeDirection.Down)
                            SnakeInitialDirection = SnakeDirection.Up;
                        break;
                    case Key.S:
                        if (SnakeInitialDirection != SnakeDirection.Up)
                            SnakeInitialDirection = SnakeDirection.Down;
                        break;
                    case Key.A:
                        if (SnakeInitialDirection != SnakeDirection.Right)
                            SnakeInitialDirection = SnakeDirection.Left;
                        break;
                    case Key.D:
                        if (SnakeInitialDirection != SnakeDirection.Left)
                            SnakeInitialDirection = SnakeDirection.Right;
                        break;
                }
            }
        }
        private void CollisionCheck()
        {
            for (int i = 1; i < SnakeParts.Count; i++)
            {
                if (SnakeParts[0].SnakePosition == SnakeParts[i].SnakePosition)
                {
                    GameOver();
                    return;
                }
            }

            if (SnakeParts[0].SnakePosition.X > 380 || SnakeParts[0].SnakePosition.X < 0 ||
                SnakeParts[0].SnakePosition.Y > 400 || SnakeParts[0].SnakePosition.Y < 0)
            {
                GameOver();
                return;
            }

            foreach (Cloud cloud in Clouds.ToList()) 
            {
                if (cloud.CloudPosition == SnakeParts[0].SnakePosition)
                {
                    EatFood();
                    return;
                }
            }
        }
        private void EatFood()
        {
            Cloud cloudToRemove = Clouds.FirstOrDefault(cloud =>
                cloud.CloudPosition.X == SnakeParts[0].SnakePosition.X && cloud.CloudPosition.Y == SnakeParts[0].SnakePosition.Y);

            Snake newBody = new Snake();


            newBody.SnakePosition = SnakeParts.Last().SnakePosition;
            SnakeParts.Add(newBody);

            _snakeBodyColours.Add(cloudToRemove.CloudRandomColor);
            if (cloudToRemove != null)
            {
                Clouds.Remove(cloudToRemove);
            }
            Score++;
        }

        private void GameOver()
        {
            SnakeParts.Clear();
            Clouds.Clear();
            _gameTimer.Enabled = false;
            MessageBox.Show("游戏结束");
        }

        public void Save()
        {
            if(_score != 0)
            {
            string connectString = "Server=localhost;DataBase=ScoreRanking;User Id=sa;Password=111";
                using (SqlConnection connection = new SqlConnection(connectString))
                {
                    connection.Open();
                    string sortQuery = $"SELECT TOP 1 * FROM {SelectedDifficulty} ORDER BY Score ASC;";
                    using (SqlCommand command = new SqlCommand(sortQuery, connection))
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            minScore = Convert.ToInt32(reader["Score"]);

                            if (_score > minScore)
                            {
                                reader.Close();
                                string deleteQuery = $"DELETE FROM {SelectedDifficulty} WHERE Score = @minScore;";
                                using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                                {
                                    deleteCommand.Parameters.AddWithValue("@minScore", minScore);
                                    deleteCommand.ExecuteNonQuery();
                                }
                                string insertQuery = $"Insert into {SelectedDifficulty} (PlayerName,Score) values (@_playerName,@_score)";
                                using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                                {
                                    insertCommand.Parameters.AddWithValue("@_score", _score);
                                    insertCommand.Parameters.AddWithValue("@_playerName", _playerName);
                                    insertCommand.ExecuteNonQuery();
                                }
                                MessageBox.Show("Save Sucessfully!");
                            }
                            else
                            {
                                MessageBox.Show("Sorry, your score does not reach the top 10 on this difficulty.Please try again!");
                            }
                        }
                        reader.Close();
                    }
                }
            }
            
        } 
        public void StartGame()
        {
            switch (SelectedDifficulty)
            {
                case "Easy":
                    _speed = 500;
                    break;
                case "Normal":
                    _speed = 300;
                    break;
                case "Hard":
                    _speed = 100;
                    break ;
            }
            SnakeParts.Clear();
            Clouds.Clear();
            Score = 0;
            SnakeInitialDirection = SnakeDirection.Right;
            _snakePosition = new Point(200, 200);
            _gameTimer.Interval = _speed;
            _gameTimer.Enabled = true;
            _gameTimer.Start();
        }
        public void Pause()
        {
            if (_gameTimer.Enabled == true)
                _gameTimer.Enabled = false;
            else if (_gameTimer.Enabled == false)
                _gameTimer.Enabled = true;
        }
        public bool CanSave
        {
            get
            {
                return !string.IsNullOrEmpty(PlayerName);
            }
        }

        private void GameTimer_Tick(object sender, ElapsedEventArgs e)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                MoveSnake();
                if (num % 3 == 0)
                    UpdateCloud();
            });
        }

    }
}
