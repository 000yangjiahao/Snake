using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Caliburn.Micro;
using SnakeRemake.Models;
using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;
using System.Collections.ObjectModel;

namespace SnakeRemake.ViewModels
{
    public class RankingViewModel : Screen
    {
        private BindableCollection<Ranking> _RankingList = new BindableCollection<Ranking>();
        private Ranking _Ranking;
        private int _rank;
        private string _playerName;
        private int _score;
        private string _selectedDifficulty = "Normal";

        public BindableCollection<Ranking> RankingList
        {
            get { return _RankingList; }
            set { _RankingList = value; }
        }
        public ObservableCollection<string> Difficulty { get; } = new ObservableCollection<string>
        {
            "Easy",
            "Normal",
            "Hard"
        };
        public Ranking Ranking
        {
            get { return _Ranking; }
            set {_Ranking = value;NotifyOfPropertyChange(nameof(Ranking));}
        }
        public int Rank
        {
            get => _rank;
            set {_rank = value;NotifyOfPropertyChange(nameof(Rank));}
        }
        public string PlayerName
        {
            get => _playerName;
            set {_playerName = value;NotifyOfPropertyChange(nameof(PlayerName));}
        }
        public int Score
        {
            get => _score;
            set {_score = value;NotifyOfPropertyChange(nameof(Score));}
        }
        public string SelectedDifficulty
        {
            get { return _selectedDifficulty; }
            set
            {
                _selectedDifficulty = value;
                NotifyOfPropertyChange(nameof(SelectedDifficulty));
            }
        }

        public void LoadRankingData()
        {
            if (!string.IsNullOrEmpty(SelectedDifficulty))
            {
                string connectString = "Server=localhost;DataBase=ScoreRanking;User Id=sa;Password=111";
                using (SqlConnection connection = new SqlConnection(connectString))
                {
                    connection.Open();
                    string sortQuery = $"select *,Rank()over(order by Score Desc) as Rank from {SelectedDifficulty}";
                    using (SqlCommand command = new SqlCommand(sortQuery, connection))
                    {
                        RankingList.Clear();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int rank = Convert.ToInt32(reader["Rank"]);
                                string playerName = reader["PlayerName"].ToString();
                                int score = Convert.ToInt32(reader["Score"]);

                                RankingList.Add(new Ranking { Rank = rank, PlayerName = playerName, Score = score });
                            }
                        }
                    }
                }
            }
        }
    }
}
