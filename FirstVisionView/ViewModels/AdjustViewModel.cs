using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using VisionView.Core;
using VisionView.DataModel;
using VisionView.HardWare;
using VisionView.ParameterUILibary.Core;

namespace VisionView.ViewModels
{
    public partial class AdjustViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<WireDataModel> _allWires = new();
        [ObservableProperty] private ObservableCollection<CardDataModel> _allCards = new();
        [ObservableProperty] private bool _delePopup = false;
        [ObservableProperty] private bool _parameterPopupStatus = false;
        [ObservableProperty] private bool _cap = false;
        [ObservableProperty] private ObservableObject? _currentEditVM = null;
        public ObservableCollection<ImageModel> ImageQueue { get; } = new();
        [ObservableProperty] private ImageModel _selectedImage;
        [ObservableProperty] private bool _isImagesourceVisible = false;//图片列表是否可见
        //引用目录树的数据，暴露给xaml绑定
        public ObservableCollection<MenuCategory> OperatorMenuTree => OperatorRegistry.GlobalMenuTree;
        public static Dictionary<string, int> SerialNumber = new();
        //用来存储组合框的选项
        public ObservableCollection<string> CombBoxItems = new();
        public bool CanRename => (AllCards.Count(c => c.IsSelected) == 1);
        // 用来记录刚才右键点击的位置
        public System.Drawing.PointF CurrentMousePoint { get; set; }
        private int _topZIndex = 0;
        public AdjustViewModel()
        {
            AllWires.CollectionChanged += AllWires_CollectionChanged;
            WeakReferenceMessenger.Default.Register<CameraGrabMessage>(this, (r, message) =>
                _ = CaptureCameraFrameAsync(message.Camera));
        }
        private async Task CaptureCameraFrameAsync(CameraInfo cameraInfo)
        {
            try
            {
                BitmapSource? frame = await Task.Run(() =>
                {
                    using var camera = new HikCamera(cameraInfo);
                    camera.Open();
                    return camera.GrabOne();
                });
                if (frame == null) return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var image = new ImageModel
                    {
                        Name = string.IsNullOrWhiteSpace(cameraInfo.Name) ? cameraInfo.Model : cameraInfo.Name,
                        Bitmap = frame
                    };
                    ImageQueue.Add(image);
                    SelectedImage = image;
                });
            }
            catch
            {
                // 相机被占用或取图失败时，保持当前预览不变
            }
        }
        /// <summary>
        /// 刷新连线集合的变化，更新卡片的上游关系和输入选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AllWires_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (WireDataModel wire in e.NewItems)
                {
                    if (!wire.TargetCard.UpstreamCards.Contains(wire.SourceCard))
                    {
                        //把连接的卡片存储在当前卡片的来源里
                        wire.TargetCard.UpstreamCards.Add(wire.SourceCard);
                    }
                    //刷新
                    wire.TargetCard.RefreshInputOptions();
                }
            }
            if (e.OldItems != null)
            {
                foreach (WireDataModel wire in e.OldItems)
                {
                    // 断开连线时，关系并重新刷新
                    wire.TargetCard.UpstreamCards.Remove(wire.SourceCard);
                    wire.TargetCard.RefreshInputOptions();
                }
            }
        }
        /// <summary>
        /// 计算卡片的执行进度
        /// </summary>
        /// <param name="card"></param>
        private void figureRunProgeress(CardDataModel card)
        {

        }
        /// <summary>
        /// 新建卡片，设置卡片的名字，
        /// </summary>
        /// <param name="CardType"></param>
        //添加卡片名称
        [RelayCommand]
        private void AddCard(MenuOperator SubOperator)
        {
            // 1. 获取鼠标点击的初始期待坐标
            double spawnX = CurrentMousePoint.X;
            double spawnY = CurrentMousePoint.Y;

            // 找到空位
            while (AllCards.Any(c => Math.Abs(c.X - spawnX) < 10 && Math.Abs(c.Y - spawnY) < 10))
            {
                spawnX += 30; // X 向右偏
                spawnY += 30; // Y 向下偏
            }
            // 3. 找到空位新建卡片并赋坐标
            CardDataModel newCard = new CardDataModel()
            {
                CardName = ParameterCardName(SubOperator.DispalyName),
                X = spawnX,
                Y = spawnY,
                IsEnable = true,
                ParameterVM = OperatorFactory.CreateOperator(SubOperator.ParameterType)//
                                                                                       // ... 其他属性
            };

            AllCards.Add(newCard);
        }
        //卡片名字生成
        public String ParameterCardName(string CardName)
        {
            if (SerialNumber.TryGetValue(CardName, out var SerialNum))
            {
                SerialNumber[CardName] = SerialNum + 1;
                CardName = CardName + SerialNumber[CardName].ToString();
            }
            else
            {
                SerialNumber[CardName] = 1;
                CardName = CardName + "1";

            }
            return CardName;
        }
        //删除卡片
        [RelayCommand]
        private void DeleteCards()
        {
            //把符合选中条件的选出并生成一个列表，防止直接操作原列表导致崩溃
            var deleteCard = AllCards.Where(s => s.IsSelected).ToList();
            var deleteWire = AllWires.Where(s => s.IsSelected).ToList();
            if (deleteCard.Count != 0)
            {
                foreach (var card in deleteCard)//删除卡片
                {
                    AllCards.Remove(card);
                    var WireList = AllWires.ToList();
                    foreach (var wire in WireList)
                    {
                        if (wire.SourceCard.CardID == card.CardID || wire.TargetCard.CardID == card.CardID)
                        {
                            AllWires.Remove(wire);
                        }
                    }

                }
            }

            if (deleteWire.Count != 0)
            {
                foreach (var wire in deleteWire)//删除线
                {
                    AllWires.Remove(wire);

                }

            }
            DelePopup = false;//关闭删除菜单
        }
        //清空卡片选择状态
        [RelayCommand]
        private void ClearSeletionStatus(CardDataModel? card = null)

        {
            if (card == null)//如果未指定卡片，则清除所有选择状态的卡片，否则清除传入卡片的状态
            {
                foreach (var SetCard in AllCards.Where(c => c.IsSelected))
                {
                    SetCard.IsSelected = false;
                }
            }
            else
            {
                card.IsSelected = false;
            }
            OnPropertyChanged(nameof(CanRename));

            return;
        }
        //卡片选择状态
        [RelayCommand]
        private void AddSeletionStatus(CardDataModel? card = null)

        {
            if (card == null) return;//如果未指定卡片，返回，否则设置传入卡片的状态
            _topZIndex++;
            card.TopZIndex = _topZIndex;
            card.IsSelected = true;
            if (_topZIndex >= 1000000)
            {
                _topZIndex = 0;
                var list = AllCards.OrderBy(c => c.TopZIndex).ToList();
                foreach (var cardZIndex in list)
                {

                    _topZIndex++;
                    cardZIndex.TopZIndex = _topZIndex;

                }
            }

            OnPropertyChanged(nameof(CanRename));

        }
        //卡片重命名
        [RelayCommand]
        private void CardRename()
        {
            var card = AllCards.FirstOrDefault(c => c.IsSelected);
            if (card == null) return;
            card.IsRenaming = true;
            DelePopup = false;
        }
        //添加线段选择状态
        [RelayCommand]
        private void WireAddStatus(WireDataModel? wire = null)
        {
            if (wire == null) return;
            wire.IsSelected = true;
        }
        //清除线段选择状态
        [RelayCommand]
        private void WireCleanStatus(WireDataModel? wire = null)
        {
            if (wire == null)
            {
                var WireList = AllWires.Where(s => s.IsSelected).ToList();
                foreach (var Cleanwire in WireList)
                {
                    Cleanwire.IsSelected = false;
                }
            }
            else wire.IsSelected = false;
        }
        [RelayCommand]
        private void ParameterClick()
        {
            Cap = false;
        }
        [RelayCommand]
        private void ChangeStatus(CardDataModel card)
        {
            if (card == null) return;
            card.IsEnable = !card.IsEnable;//反转启用状态
        }
        //添加单张图片
        [RelayCommand]
        private void AddImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "选择图像文件",
                Multiselect = true,
                Filter = "图像文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif;"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filePath in openFileDialog.FileNames)
                {
                    if (!ImageQueue.Any(i => i.Path == filePath))//如果不存在重复路径的图片，则添加到列表
                    {
                        string fileName = Path.GetFileName(filePath);//从路径中提取文件名
                        ImageQueue.Add(new ImageModel
                        { Name = fileName, Path = filePath });
                    }
                }
            }
        }
        //添加文件夹
        [RelayCommand]
        private void AddFile()
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog
            {
                Title = "选择文件",
                Multiselect = false,
            };
            if (openFolderDialog.ShowDialog() == true)
            {
                string folderPath = openFolderDialog.FolderName;
                string[] imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp" };//定义支持的图片格式
                string[] allFiles = Directory.GetFiles(folderPath);//获取文件夹下的所有文件
                foreach (var filePath in allFiles)
                {
                    string extension = Path.GetExtension(filePath).ToLower();//获取文件扩展名并转换为小写
                    if (imageExtensions.Contains(extension) && !ImageQueue.Any(i => i.Path == filePath))//如果文件是图片且不存在重复路径的图片，则添加到列表
                    {
                        var fileName = Path.GetFileName(filePath);//从路径中提取文件名
                        ImageQueue.Add(new ImageModel
                        { Name = fileName, Path = filePath });
                    }
                }
            }

        }
        //删除图片队列
        [RelayCommand]
        private void DeleteImage()
        {
            if (SelectedImage == null)
            {
                //此处添加全部删除提示
                ImageQueue.Clear();
            }
            else
            {
                ImageQueue.Remove(SelectedImage);
            }
            WeakReferenceMessenger.Default.Send("ImageDelete");

        }
        //图片队列选中项改变时的处理函数
        partial void OnSelectedImageChanged(ImageModel value)
        {
            if (value != null)
            {
                WeakReferenceMessenger.Default.Send("ImageSelected");
            }
        }
    }
    //图片数据模型，包含图片名字和路径
    public partial class ImageModel : ObservableObject
    {
        [ObservableProperty] private string _name = "";
        [ObservableProperty] private string _path = "";
        [ObservableProperty] private BitmapSource? _bitmap;

        public BitmapSource? DisplaySource => Bitmap ?? LoadFileSource();

        private BitmapSource? _fileSource;
        private BitmapSource? LoadFileSource()
        {
            if (string.IsNullOrWhiteSpace(Path)) return null;
            if (_fileSource != null) return _fileSource;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(Path, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                _fileSource = bitmap;
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        partial void OnBitmapChanged(BitmapSource? value) => OnPropertyChanged(nameof(DisplaySource));

        partial void OnPathChanged(string value)
        {
            _fileSource = null;
            OnPropertyChanged(nameof(DisplaySource));
        }
    }

}
