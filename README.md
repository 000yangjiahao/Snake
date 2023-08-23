# SnakeEat
1. MVVM框架的搭建
在开发初期因为对MVVM框架一知半解，导致MVVM框架结构混乱，代码编写到ChangeDirection（）方法时就已经编写不下来了，然后是重新学习了MVVM的基础知识，做了一遍教程上的案例，一个简单的客户提交订单的系统，学习网址是https://www.tutorialspoint.com/mvvm/index.htm。
在这次学习中，我认识到了正确的MVVM框架的雏形应该是怎么样的，学习了包括但不限于数据绑定，命令绑定，各种控件的使用知识。而后学习了MVVM框架的轻量化框架Calibrun.Mirco（以下简称CM），CM使用了先VM再V的思想，使用命名绑定的方法减轻了大量绑定方面的难度。
最终我使用了3个Model：Snake，Cloud，Ranking，3个V：GameView，MainView，RankingView和对应的3个VM：GameViewModel，MainViewModel，RankingViewModel。其中MainViewModel代替了原先WPF MainWindow的工作，GameViewModel整合了Snake和Cloud的数据，提供给GameView。Ranking，RankingViewModel和RankingView也是一样的关系。
[图片]
3. 游戏本身的开发
2.1 编写3个Model
  创建3个类命名为Snake，Cloud，Ranking，并添加需要的属性。
```C#
  public class Snake : Caliburn.Micro.PropertyChangedBase
    {
        public Point SnakePosition { get; set; }
        public int InitialLength { get; set; } = 2;
        //蛇的身体大小，用例统一生成的食物和蛇一格占用游戏区域的大小
        public int SnakeSquareSize { get; set; } = 20;

        private SolidColorBrush colorBrush;
        //用于后面给蛇身体附上食物的颜色
        public SolidColorBrush SnakeBodyColor {
            get 
            {
                return colorBrush;
            }
            set 
            {
                if (colorBrush != value) 
                {
                    colorBrush = value;
                    NotifyOfPropertyChange(() => SnakeBodyColor);
                }
            }
        }
    }
 public class Cloud : Caliburn.Micro.PropertyChangedBase
    {
        public Point CloudPosition { get; set; }
        public SolidColorBrush CloudRandomColor { get; set; }
    }
 public class Ranking
    {
        public int Rank { get;  set; }
        public string PlayerName { get;  set; }
        public int Score { get;  set; }
    }
```
2.2 MainViewModel和MainView的编写。
  因为大量的初始化信息放在Caliburn生成的Bootstrapper类中，所以MainViewModel十分简单。
  Bootstrapper类中的代码，主要功能是将所有的VM和V放进容器中，方便后面的对应生成。
```C#
public class Bootstrapper : BootstrapperBase
    {
        private readonly SimpleContainer _container = new SimpleContainer();
        public Bootstrapper()
        {
            Initialize();
            StartDebugLogger();
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            await DisplayRootViewForAsync(typeof(MainViewModel));
        }

        public static void StartDebugLogger()
        {
            LogManager.GetLog = type => new DebugLog(type);
        }

        protected override void Configure()
        {
            _container.Instance(_container);
            _container
              .Singleton<IWindowManager, WindowManager>()
              .Singleton<IEventAggregator, EventAggregator>();

            foreach (var assembly in SelectAssemblies())
            {
                assembly.GetTypes()
               .Where(type => type.IsClass)
               .Where(type => type.Name.EndsWith("ViewModel"))
               .ToList()
               .ForEach(viewModelType => _container.RegisterPerRequest(
                   viewModelType, viewModelType.ToString(), viewModelType));
            }
        }
        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }
    }
```
  为了启动时先调用Bootstrapper，需要在App.XAML中添加以下代码
```C#
<Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary>
                    <local:Bootstrapper x:Key="Bootstrapper" />
                </ResourceDictionary>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
```
  MainViewModel的代码，包括初始化，和另外两个View的切换方法。
```C#
 private readonly IWindowManager _windowManager;
        public MainViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }
        public Task Start()
        {
            return _windowManager.ShowDialogAsync(IoC.Get<GameViewModel>());
        }
        public Task StartRanking()
        {
            return _windowManager.ShowDialogAsync(IoC.Get<RankingViewModel>());
        }
```
  得益于CM的简便，我只需要在MainView中将button的x：name设置成对应的方法名Start，StartRanking即可完成绑定非常的方便。
```C#
<Grid>
        <Grid.Background >
            <ImageBrush ImageSource="\images\云朵背景图.jpg" />
        </Grid.Background>
        <TextBlock HorizontalAlignment="Center" Margin="0,103,0,0" TextWrapping="Wrap" Text="Snake" VerticalAlignment="Top" Height="118" Width="274" Cursor="None" FontFamily="Mistral" TextTrimming="CharacterEllipsis" FontSize="90" FontWeight="Bold" FontStyle="Italic" Block.TextAlignment="Center" OpacityMask="Black">
        </TextBlock>
        <Button x:Name="Start" Content="Start" HorizontalAlignment="Center" VerticalAlignment="Center" Height="96" Width="180" FontSize="90" FontFamily="Mistral">
            <Button.BorderBrush>
                <ImageBrush/>
            </Button.BorderBrush>
            <Button.Background>
                <ImageBrush/>
            </Button.Background>
        </Button>
        <Button x:Name="StartRanking" Content="Ranking" HorizontalAlignment="Center" Margin="0,362,0,0" VerticalAlignment="Top" Height="73" Width="180" FontFamily="Mistral" FontSize="60">
            <Button.BorderBrush>
                <ImageBrush/>
            </Button.BorderBrush>
            <Button.Background>
                <ImageBrush/>
            </Button.Background>
        </Button>

    </Grid>
```
[图片]
  
2.3 RankingView和RankingViewModel的编写
      因为GameViewModel体量过长，先进行了RankingViewModel的编写。
      在RankingViewModel中先获取Ranking中的属性，并创建了一个BindableCollection<Ranking>用于保存前10名的数据，以及完成数据更新后的通知工作。
```C#
        private BindableCollection<Ranking> _RankingList = new BindableCollection<Ranking>();
        private Ranking _Ranking;
        private int _rank;
        private string _playerName;
        private int _score;
        
        //因为BindableCollection自带NotifyOfPropertyChange的作用所以没有写
        public BindableCollection<Ranking> RankingList
        {
            get { return _RankingList; }
            set { _RankingList = value; }
        }
        
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
```
创建了ObservableCollection<string> Difficulty 用于给RankingView中的ComboBox提供选项，创建了selectedDifficulty用于获取View中选择的难度。
```C#
        private string _selectedDifficulty = "Normal";
        public ObservableCollection<string> Difficulty { get; } = new ObservableCollection<string>
        {
            "Easy",
            "Normal",
            "Hard"
        };
        public string SelectedDifficulty
        {
            get { return _selectedDifficulty; }
            set
            {
                _selectedDifficulty = value;
                NotifyOfPropertyChange(nameof(SelectedDifficulty));
            }
        }
        ```
最后是根据selectedDifficulty的值进入对应的数据库查找数据，并将里面的数据添加到BindableCollection<Ranking>中。
```C#
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
```
RankingView的XAML代码
```C#
    <Grid>
        //给整体添加背景
        <Grid.Background>
            <ImageBrush ImageSource="\Images\云朵背景图.jpg"/>
        </Grid.Background>
        //分成上下两大层，第一层用于放置标题，第二层用于放置ComboBox，button和itemscontrol
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        //标题
        <TextBlock Grid.Row="0" HorizontalAlignment="Center" Margin="0,40,0,0" Block.TextAlignment="Center" TextWrapping="Wrap" VerticalAlignment="Top" Height="73" Width="260" FontSize="52" FontFamily="Mistral"><Run Language="zh-cn" Text="Ranking"/></TextBlock>
        //第二层再分为左右两半，左边放置ConboBox和button，右边放置itemscontrol
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="150"/>
                <ColumnDefinition/>
            </Grid.ColumnDefinitions>
            //展示三个可选难度
            <ComboBox x:Name="Difficulty" Height="55" FontFamily="Mistral" FontSize="40" Margin="5,100,0,0" SelectedItem="{Binding SelectedDifficulty}"/>
            //调用后台更新数据的方法
            <Button x:Name="LoadRankingData" Content="View" FontFamily="Mistral" FontSize="45" BorderBrush="White" Grid.ColumnSpan="2" Margin="5,249,645,-191">
                <Button.Background>
                    <ImageBrush/>
                </Button.Background>
            </Button>
            //展示排名的控件
            <ItemsControl Grid.Column="1" Margin="10,10,10,0" ItemsSource="{Binding RankingList}" VerticalContentAlignment="Stretch" FontSize="8">
                <ItemsControl.Foreground>
                    <LinearGradientBrush EndPoint="0.5,1" StartPoint="0.5,0">
                        <GradientStop Color="#FFFF0A0A"/>
                        <GradientStop Color="Black" Offset="1"/>
                    </LinearGradientBrush>
                </ItemsControl.Foreground>
                <ItemsControl.ItemsPanel>
                    <ItemsPanelTemplate>
                        <UniformGrid Rows="1" Columns="4"/>
                    </ItemsPanelTemplate>
                </ItemsControl.ItemsPanel>
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Border  Margin="2" Width="125">
                            <StackPanel>
                                <TextBlock HorizontalAlignment="Center" FontSize="20" FontWeight="Bold" Margin="0,10,0,0" Text="{Binding Rank}"/>
                                <TextBlock HorizontalAlignment="Center" FontSize="14" Margin="0,5,0,0" Text="{Binding PlayerName}"/>
                                <TextBlock HorizontalAlignment="Center" FontSize="14" Margin="0,5,0,10" Text="{Binding Score}"/>
                            </StackPanel>
                        </Border>
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </Grid>
    </Grid>
```
实际运行效果：
一开始什么都不点击：
[图片]
点击View按钮后跳转到对应排行榜：
[图片]
2.4 GameViewModel和GameView
2.4.1 先获取Snake和Cloud的属性
```C#
        //用于获得食物的随机颜色和位置
        private Random random = new Random();
        
        private Cloud _cloud;
        private Point _cloudPosition;
        private SolidColorBrush _cloudRandomColor;


        private Snake _snake;
        private Point _snakePosition;
        private SolidColorBrush _snakeBodyColour;
        //初始化的蛇有一节头和一节身体，先将他们的颜色放在专门放置蛇身体颜色的List中
        private List<SolidColorBrush> _snakeBodyColours { get; set; } = new List<SolidColorBrush>()
        {
            new SolidColorBrush(Colors.Red),
            new SolidColorBrush(Colors.Green),
        };


        public enum SnakeDirection { Left, Right, Up, Down };
        private SnakeDirection _snakeInitialDirection { get; set; } = SnakeDirection.Right;

        //存放蛇和食物的两个ObservableCollection，用于获得他们的位置，颜色等信息
        private ObservableCollection<Snake> _snakeParts = new ObservableCollection<Snake>();
        private ObservableCollection<Cloud> _clouds=new ObservableCollection<Cloud>();
        //以下是当属性更新时的通知。
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
        
        public ObservableCollection<Cloud> Clouds
        {
            get { return _clouds; }
            set { _clouds = value; NotifyOfPropertyChange(nameof(Clouds)); }
        }
```
2.4.2 编写蛇移动的两个方法AddSnakeBodyPart（），MoveSnake()。
```C#
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
            //这个是计数器，用于在同一个计时器中修改食物生成的频率
            num++;
            AddSnakeBodyPart(SnakePosition);
            //这个是碰撞检查的方法，后面会展示
            CollisionCheck();
        }
        private void AddSnakeBodyPart(Point position)
        {
            //对蛇先进行初始化
            if (SnakeParts.Count < Snake.InitialLength)
            {
                Snake newBodyPart = new Snake
                {
                    SnakePosition = position,
                };
                SnakeParts.Insert(0, newBodyPart);
            }
            //当初始化完成后，在移动时删除蛇的尾巴，并在新的位置重新生成一个蛇头
            else
            {
                SnakeParts.RemoveAt(SnakeParts.Count - 1);
                Snake newBodyPart = new Snake
                {
                    SnakePosition = position,
                };
                SnakeParts.Insert(0, newBodyPart);
            }
            //给蛇重新刷一遍颜色
            for (int i = 0; i < SnakeParts.Count; i++)
            {
                SnakeParts[i].SnakeBodyColor = _snakeBodyColours[i];
            }
        }
```
2.4.3 编写转向的方法ChangeDirection（）
```C#
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
```
2.4.4 编写随机颜色的方法：
```C#
        private SolidColorBrush RandomColor()
        {
            byte r = (byte)random.Next(0, 256);
            byte g = (byte)random.Next(0, 256);
            byte b = (byte)random.Next(0, 256);
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }
```
2.4.5 编写食物生成的方法 
      添加了一个HashSet<Point>用于快速查找蛇和食物的位置：
```C#
private HashSet<Point> occupiedPositions = new HashSet<Point>();

private void UpdateCloud()
        {
            //将蛇的位置添加到不可用
            foreach (var snakePart in SnakeParts)
            {
                occupiedPositions.Add(snakePart.SnakePosition);
            }
            //将云的位置添加到不可用
            foreach (var cloud in Clouds)
            {
                occupiedPositions.Add(cloud.CloudPosition);
            }

            //创建一个可用位置的List
            List<Point> availablePositions = new List<Point>();
            //遍历所有游戏区域，将不可用的区域去除
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
            //根据可用的区域随机生成云的位置
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
```
2.4.6 编写吃食物的方法：
```C#
        private void EatFood()
        {
            //获得被吃掉食物的位置
            Cloud cloudToRemove = Clouds.FirstOrDefault(cloud =>
                cloud.CloudPosition.X == SnakeParts[0].SnakePosition.X && cloud.CloudPosition.Y == SnakeParts[0].SnakePosition.Y);
            //为蛇添加新的身体
            Snake newBody = new Snake();

            newBody.SnakePosition = SnakeParts.Last().SnakePosition;
            SnakeParts.Add(newBody);
            //将被吃掉的云的颜色添加到蛇身体颜色的列表中
            _snakeBodyColours.Add(cloudToRemove.CloudRandomColor);
            //删除被吃掉的云
            if (cloudToRemove != null)
            {
                Clouds.Remove(cloudToRemove);
            }
            Score++;
        }
```
2.4.7 编写碰撞检测
```C#
private void CollisionCheck()
        {
            //判断蛇有没有吃到自己
            for (int i = 1; i < SnakeParts.Count; i++)
            {
                if (SnakeParts[0].SnakePosition == SnakeParts[i].SnakePosition)
                {
                    GameOver();
                    return;
                }
            }
            //判断蛇有没有超出游戏区域
            if (SnakeParts[0].SnakePosition.X > 380 || SnakeParts[0].SnakePosition.X < 0 ||
                SnakeParts[0].SnakePosition.Y > 400 || SnakeParts[0].SnakePosition.Y < 0)
            {
                GameOver();
                return;
            }
            //判断蛇是不是吃到食物
            foreach (Cloud cloud in Clouds.ToList()) 
            {
                if (cloud.CloudPosition == SnakeParts[0].SnakePosition)
                {
                    EatFood();
                    return;
                }
            }
        }
```
2.4.8 编写开始，结束和暂停的方法
```C#
        private void GameOver()
        {
            MessageBox.Show("游戏结束");
            SnakeParts.Clear();
            Clouds.Clear();
            _gameTimer.Enabled = false;
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
```
2.4.9 编写保存方法和保存的条件
        主要是在进行数据库连接，根据选择的难度，查找对应的数据库，如果满足条件就删除最后一名，并保存当前的成绩。
```C#
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
 public bool CanSave
        {
            get
            {
                return !string.IsNullOrEmpty(PlayerName);
            }
        }
```
2.4.10 编写View中需要的ComboBox数据和接收选择的难度
```C#
 private string _selectedDifficulty = "Normal";
 public string SelectedDifficulty
        {
            get { return _selectedDifficulty; }
            set
            {
                _selectedDifficulty = value;
                NotifyOfPropertyChange(() => SelectedDifficulty);
            }
        }
  public ObservableCollection<string> Difficulty { get; } = new ObservableCollection<string>
        {
            "Easy",
            "Normal",
            "Hard"
        };
```
2.4.11 添加计时器让蛇自己移动
WPF常用的计时器有两种：DispatcherTimer定时器和System.Timers.Timer定时器
两者的差别为：
DispatcherTimer定时器和UI为同一个线程。
System.Timers.Timer定时器为不同线程。
原先的卡顿问题可能是因为使用了DispatcherTimer定时器导致的，在换成System.Timers.Timer定时器后卡顿问题解决。
```C#
private System.Timers.Timer _gameTimer;
public GameViewModel(IEventAggregator eventAggregator)
{
    _gameTimer = new System.Timers.Timer();
    _gameTimer.Elapsed += GameTimer_Tick;
    _snake = new Snake();
    _cloud = new Cloud();

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
```
2.4.12 GameView的XAML代码
        大体和RankingView类似就不多做解释了。
```C#
    //绑定KeyDown事件的方法
    <i:Interaction.Triggers>
        <i:EventTrigger EventName="KeyDown">
            <cal:ActionMessage MethodName="ChangeDirection" >
                <cal:Parameter Value="$eventArgs"/>
            </cal:ActionMessage>
        </i:EventTrigger>
    </i:Interaction.Triggers>
    //主Grid控件
    <Grid Focusable="False">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="400"/>
            <ColumnDefinition Width="240"/>
        </Grid.ColumnDefinitions>
        //游戏区域的Canvas控件
        <Canvas x:Name="GameArea" Width="400">
            <Canvas.Background>
                <ImageBrush ImageSource="\Images\天空背景.jpg"/>
            </Canvas.Background>
            <ItemsControl ItemsSource="{Binding SnakeParts}">
                <ItemsControl.ItemsPanel>
                    <ItemsPanelTemplate>
                        <Canvas IsItemsHost="True"/>
                    </ItemsPanelTemplate>
                </ItemsControl.ItemsPanel>
                <ItemsControl.ItemContainerStyle>
                    <Style>
                        <Setter Property="Canvas.Left" Value="{Binding SnakePosition.X}" />
                        <Setter Property="Canvas.Top" Value="{Binding SnakePosition.Y}" />
                    </Style>
                </ItemsControl.ItemContainerStyle>
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Rectangle Width="{Binding SnakeSquareSize}" Height="{Binding SnakeSquareSize}" Fill="{Binding SnakeBodyColor}" />
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
            <ItemsControl ItemsSource="{Binding Clouds}">
                <ItemsControl.ItemsPanel>
                    <ItemsPanelTemplate>
                        <Canvas IsItemsHost="True"/>
                    </ItemsPanelTemplate>
                </ItemsControl.ItemsPanel>
                <ItemsControl.ItemContainerStyle>
                    <Style>
                        <Setter Property="Canvas.Left" Value="{Binding CloudPosition.X}" />
                        <Setter Property="Canvas.Top" Value="{Binding CloudPosition.Y}" />
                    </Style>
                </ItemsControl.ItemContainerStyle>
                <ItemsControl.ItemTemplate>
                    <DataTemplate>
                        <Ellipse Width="20" Height="20" Fill="{Binding CloudRandomColor}" />
                    </DataTemplate>
                </ItemsControl.ItemTemplate>
            </ItemsControl>
        </Canvas>
        //用于难度选择，得分展示等功能性的区域。
        <Grid Grid.Column="1">
            <Grid.RowDefinitions>
                <RowDefinition Height="210"/>
                <RowDefinition Height="175"/>
            </Grid.RowDefinitions>
            <Grid Grid.Column="1" Grid.Row="0" Width="200">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="82"/>
                    <ColumnDefinition Width="118"/>
                    <ColumnDefinition Width="0*"/>
                </Grid.ColumnDefinitions>
                <Grid.RowDefinitions>
                    <RowDefinition Height="70" />
                    <RowDefinition Height="70" />
                    <RowDefinition/>
                </Grid.RowDefinitions>
                <TextBlock Grid.Row="0" Grid.Column="0" Margin="0,20,0,10" FontFamily="Mistral" FontSize="25" >Difficulty</TextBlock>
                <ComboBox x:Name="Difficulty" Grid.Row="0" Grid.Column="1" Width="100" FontFamily="Mistral" FontSize="30" VerticalAlignment="Center" SelectedItem="{Binding SelectedDifficulty}">
                </ComboBox>
                <TextBlock Grid.Row="1" Grid.Column="0" Margin="0,15,10,0" FontFamily="Mistral" FontSize="33" RenderTransformOrigin="0.297,0.701">Scores</TextBlock>
                <TextBlock Grid.Row="1" Grid.Column="1" Margin="0,23,10,27" Text="{Binding Score, Mode=TwoWay}" FontSize="15" FontFamily="Arial Black" TextAlignment="Center" />
                <TextBlock Grid.Row="2" Grid.Column="0" Margin="0,16,0,4" FontFamily="Mistral" FontSize="30" RenderTransformOrigin="0.486,0.418">UserID</TextBlock>
                <TextBox Grid.Row="2" Grid.Column="1" Margin="5,23,10,27" Text="{Binding PlayerName ,Mode=TwoWay}" FontSize="15" FontFamily="Arial Black"/>
            </Grid>
            <Grid Grid.Row="1" Width="200">
                <Grid.RowDefinitions>
                    <RowDefinition />
                    <RowDefinition />
                    <RowDefinition />
                </Grid.RowDefinitions>
                <Button x:Name="StartGame" Content="StartNewGame" Width="160" FontFamily="Mistral" FontSize="30" BorderBrush="White">
                    <Button.Background>
                        <ImageBrush/>
                    </Button.Background>
                </Button>
                <Button Grid.Row="1" x:Name="Pause" Content="Pause" Width="160" FontFamily="Mistral" FontSize="40" BorderBrush="White">
                    <Button.Background>
                        <ImageBrush/>
                    </Button.Background>
                </Button>
                <Button x:Name="Save" Content="Save" Grid.Row="2" Width="160" FontFamily="Mistral" FontSize="45" BorderBrush="White">
                    <Button.Background>
                        <ImageBrush/>
                    </Button.Background>
                </Button>
            </Grid>
        </Grid>
    </Grid>
```
2.4.13 运行效果
[图片]
[图片]
