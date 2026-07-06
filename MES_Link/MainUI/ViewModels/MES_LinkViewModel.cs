using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using MES_Link.MesSimulator;
using MES_Link.IniParser;
using MES_Link.Log;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MES_Link.MainUI.ViewModels
{
    public class MES_LinkViewModel : ObservableObject
    {
        // 視窗關閉的 Command
        public RelayCommand WindowClosingCommand { get; }

        // MES 模擬器
        private readonly MesSimulatorService _mesSimulatorService;
        public ObservableCollection<MesRouteBlock> MesRoutes => _mesSimulatorService.Routes;

        // 增加List Route
        public RelayCommand AddRouteCommand { get; }

        // 移除List Route
        public AsyncRelayCommand<MesRouteBlock> RemoveRouteCommand { get; }

        // 複製List Url
        public RelayCommand<MesRouteBlock> CopyRouteUrlCommand { get; }

        // Server啟動/關閉
        public RelayCommand ToggleServerCommand { get; }

        // 編輯、儲存全域按鈕
        public AsyncRelayCommand ToggleGlobalLockCommand { get; private set; }

        // Server按鈕文字
        public string ServerButtonText => IsServerRunning ? "◼️ Stop" : "▶ Start";
        // 編輯、儲存全域按鈕文字
        public string GlobalLockButtonText => _isRoutesLocked ? "✏️" : "💾";
        // UI StatusCode選項
        public List<int> StatusCodesItem { get; } = new List<int> { 200, 400, 404, 500 };
        // UI 延遲秒數選項
        public List<int> DelayMsItem { get; } = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        // UI ContentType選項
        public List<string> ContentTypesItem { get; } = new List<string> { "application/json", "text/xml; charset=utf-8", "application/xml", "application/soap+xml; charset=utf-8", "text/plain; charset=utf-8", "text/html; charset=utf-8" };

        // 按鈕的狀態
        public bool IsRoutesEditable => !_isRoutesLocked;

        public bool IsServerRunning => _mesSimulatorService?.IsRunning ?? false;

        // 版權、版號
        private string _copyRights;
        public string CopyRights
        {
            get => _copyRights;
            set => SetProperty(ref _copyRights, value);
        }

        public MES_LinkViewModel()
        {

            LogService.MainLogger.Info("Initial MES_Link...");

            _mesSimulatorService = new MesSimulatorService();
            _mesSimulatorService.OnSimulatorLogAppended += MesSimulatorService_OnSimulatorLogAppended;

            AddRouteCommand = new RelayCommand(OnAddRoute);
            RemoveRouteCommand = new AsyncRelayCommand<MesRouteBlock>(OnRemoveRouteAsync);
            ToggleServerCommand = new RelayCommand(ToggleServer);
            ToggleGlobalLockCommand = new AsyncRelayCommand(OnToggleGlobalLockAsync);
            CopyRouteUrlCommand = new RelayCommand<MesRouteBlock>(OnCopyRouteUrl);

            LoadSettingsOnStartup();

            // 唯有當從 INI 讀出來完全沒資料時，才給予一筆預設
            if (_mesSimulatorService.Routes.Count == 0)
            {
                _mesSimulatorService.Routes.Add(new MesRouteBlock());
            }

            // 綁定關閉邏輯
            WindowClosingCommand = new RelayCommand(OnWindowClosing);

            InitCopyRightsText();
        }

        // Route List鎖定狀態
        private bool _isRoutesLocked = true;
        public bool IsRoutesLocked
        {
            get => _isRoutesLocked;
            set
            {
                if (SetProperty(ref _isRoutesLocked, value))
                {
                    OnPropertyChanged(nameof(IsRoutesEditable));
                    OnPropertyChanged(nameof(GlobalLockButtonText));
                }
            }
        }

        // Base Url
        private string _baseUrlInput = "http://localhost:8080/";
        public string BaseUrlInput
        {
            get => _baseUrlInput;
            set
            {
                if (SetProperty(ref _baseUrlInput, value))
                {
                    if (_mesSimulatorService != null)
                    {
                        _mesSimulatorService.BaseUrl = value;
                    }
                }
            }
        }

        private string _simulatorLog;
        // UI 記錄 LOG
        public string SimulatorLog
        {
            get => _simulatorLog;
            set => SetProperty(ref _simulatorLog, value);
        }

        // 編輯、儲存全域按鈕
        private async Task OnToggleGlobalLockAsync()
        {
            if (!IsRoutesLocked)
            {
                // 從 編輯 切換回 儲存 時，集體存檔
                await SaveCurrentSettingsAsync();
                IniFile.Save(IniFile.Current);
            }
            IsRoutesLocked = !IsRoutesLocked;
        }

        // 複製List Url
        private void OnCopyRouteUrl(MesRouteBlock block)
        {
            if (block == null)
                return;

            try
            {
                string routeUrl = block.RouteUrl ?? string.Empty;
                string fullUrl = _baseUrlInput + routeUrl;

                // 複製到系統
                System.Windows.Forms.Clipboard.SetText(fullUrl);

                LogService.MainLogger.Info($"User copied URL: {fullUrl}");
            }
            catch (Exception ex)
            {
                LogService.MainLogger.Error(ex, "Copy url fail。");
                ShowErrorMessage("Copy route url fail" + $":{ex.Message}", "Error");
            }
        }

        // 載入初始化設定
        private void LoadSettingsOnStartup()
        {
            try
            {
                string filePath = GetJsonFilePath();

                // 檢查檔案是否存在，若不存在則直接初始化預設值並結束
                if (!File.Exists(filePath))
                {
                    LogService.MainLogger.Info("Initial default setting for MesRouteBlock (File not found)");
                    InitializeDefaultSettings();
                    LoadBaseUrl();
                    return;
                }

                // 讀取檔案
                string jsonStr = string.Empty;
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    jsonStr = reader.ReadToEnd();
                }

                if (string.IsNullOrEmpty(jsonStr))
                {
                    LogService.MainLogger.Error("jsonStr is null or empty. Fallback to default.");
                    InitializeDefaultSettings();
                    LoadBaseUrl();
                    return;
                }

                // 反序列化
                var savedRoutes = JsonConvert.DeserializeObject<List<MesRouteBlock>>(jsonStr);
                if (savedRoutes == null)
                {
                    LogService.MainLogger.Error("savedRoutes is null. Fallback to default.");
                    InitializeDefaultSettings();
                    LoadBaseUrl();
                    return;
                }

                // 檢查服務狀態
                if (_mesSimulatorService == null || _mesSimulatorService.Routes == null)
                {
                    LogService.MainLogger.Error("_mesSimulatorService or _mesSimulatorService.Routes is null");
                    return;
                }

                // 驗證並載入資料
                _mesSimulatorService.Routes.Clear();
                foreach (var route in savedRoutes)
                {
                    ValidateAndSanitizeRoute(route);
                    _mesSimulatorService.Routes.Add(route);
                }

                // 載入 BaseUrl
                LoadBaseUrl();
            }
            catch (Exception ex)
            {
                LogService.MainLogger.Fatal(ex, "Load settings on startup fail");
                ShowErrorMessage("Load settings on startup fail" + $":{ex.Message}", "Error");
            }
        }

        // 初始化預設設定
        private void InitializeDefaultSettings()
        {
            if (_mesSimulatorService?.Routes != null)
            {
                _mesSimulatorService.Routes.Clear(); // 確保清空
                _mesSimulatorService.Routes.Add(new MesRouteBlock());

                // 建構子初始化階段，使用快速背景線程同步等待存檔避免異步死鎖
                Task.Run(async () => await SaveCurrentSettingsAsync()).Wait();
            }
        }

        // 驗證並修正 Route 的欄位值
        private void ValidateAndSanitizeRoute(MesRouteBlock route)
        {
            if (!StatusCodesItem.Contains(route.StatusCode_Format1))
                route.StatusCode_Format1 = 200;
            if (!StatusCodesItem.Contains(route.StatusCode_Format2))
                route.StatusCode_Format2 = 400;
            if (!ContentTypesItem.Contains(route.ContentType))
                route.ContentType = "application/json";
            if (!DelayMsItem.Contains(route.DelayMs))
                route.DelayMs = 0;
        }

        // 載入 BaseUrl 設定
        private void LoadBaseUrl()
        {
            string lastBaseUrl = IniFile.Current.MesSimulatorSettings.BaseUrl;
            if (!string.IsNullOrEmpty(lastBaseUrl) && _mesSimulatorService != null)
            {
                BaseUrlInput = lastBaseUrl;
                _mesSimulatorService.BaseUrl = BaseUrlInput;
            }
        }

        // 儲存 INI 與 Format 設定
        public async Task SaveCurrentSettingsAsync()
        {
            try
            {
                var routeList = new List<MesRouteBlock>(_mesSimulatorService.Routes);
                string jsonStr = JsonConvert.SerializeObject(routeList, Formatting.Indented);
                string filePath = GetJsonFilePath();

                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    await writer.WriteAsync(jsonStr);
                }

                IniFile.Current.MesSimulatorSettings.BaseUrl = BaseUrlInput;
                IniFile.Save(IniFile.Current);
            }
            catch (Exception ex)
            {
                LogService.MainLogger.Fatal(ex, "Save current settings fail");
                ShowErrorMessage("Save current settings fail" + $":{ex.Message}", "Error");
            }
        }

        // 視窗關閉後
        private void OnWindowClosing()
        {
            // 視窗關閉時強制將連線監聽關閉防止卡通訊埠
            _mesSimulatorService.Stop();

            // 解除事件訂閱，防止 GC 無法回收造成記憶體洩漏
            _mesSimulatorService.OnSimulatorLogAppended -= MesSimulatorService_OnSimulatorLogAppended;

            // 確保主程式生命週期結束前，能將資料緩衝區成功 Flush 並寫入硬碟中
            Task.Run(async () => await SaveCurrentSettingsAsync()).Wait();

            LogService.MainLogger.Info("Close Window ...");
        }

        // MES 模擬器Log
        private void MesSimulatorService_OnSimulatorLogAppended(string logText)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                string newLog = SimulatorLog + logText;

                // 如果字串超過 10 萬個字元就截斷舊的
                if (newLog.Length > 100000)
                {
                    // 保留後半段
                    newLog = "Log truncated for performance...\r\n" + newLog.Substring(newLog.Length - 50000);
                }

                SimulatorLog = newLog;
            });
        }

        // 增加List Route
        private void OnAddRoute()
        {
            lock (_mesSimulatorService.RoutesLock)
            {
                MesRoutes.Add(new MesRouteBlock());
            }
            IsRoutesLocked = false;
        }

        // 移除List Route
        private async Task OnRemoveRouteAsync(MesRouteBlock block)
        {
            if (block != null)
            {
                if (!ConfirmDialog($"Delete this route?\n\nTagName: {block.TagName}", "Confirm"))
                {
                    LogService.MainLogger.Info("User cancelled route deletion.");
                    return;
                }

                MesRoutes.Remove(block);
                await SaveCurrentSettingsAsync(); // 刪除時自動更新暫存
            }
        }

        // Server啟動/關閉
        private void ToggleServer()
        {
            try
            {
                if (!_mesSimulatorService.IsRunning)
                {
                    // 啟動前強制把網址更新進 Service
                    _mesSimulatorService.BaseUrl = BaseUrlInput;

                    // Service Start 成功才處理
                    if (_mesSimulatorService.Start())
                    {
                        LogService.MainLogger.Info("Sever start successfully.");

                        // 如果未鎖定，存檔並轉為鎖定狀態
                        if (!IsRoutesLocked)
                        {
                            // 異步執行
                            ToggleGlobalLockCommand.Execute(null);
                        }
                        else
                        {
                            // 原本即為鎖定狀態
                            IsRoutesLocked = true;
                        }
                    }
                    else
                    {
                        LogService.MainLogger.Error("Sever start fail");
                        ShowErrorMessage("Sever start fail", "Error");
                    }
                }
                else
                {
                    // Service Stop 成功才處理
                    if (_mesSimulatorService.Stop())
                    {
                        LogService.MainLogger.Info("Sever stop successfully.");
                    }
                    else
                    {
                        LogService.MainLogger.Error("Sever stop fail");
                        ShowErrorMessage("Sever stop fail", "Error");
                    }
                }

                // 集中更新所有與 Server 狀態相關的 UI 通知
                RefreshServerStatusUI();
            }
            catch (Exception ex)
            {
                LogService.MainLogger.Fatal(ex, "Toggle server fail");
                ShowErrorMessage("Toggle server fail" + $":{ex.Message}", "Error");
            }
        }

        // 定義 JSON 檔案的儲存路徑
        private string GetJsonFilePath()
        {
            // 取得 bin 執行目錄下的 RouteFormat 資料夾
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RouteFormat");

            // 如果資料夾不存在就建立
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // 回傳完整檔案路徑
            return Path.Combine(folderPath, "Format.json");
        }

        private void InitCopyRightsText()
        {
            try
            {
                // 取得組件資訊
                Assembly assembly = Assembly.GetExecutingAssembly();

                // 版號
                string version = assembly.GetName().Version?.ToString() ?? "2.4.0.19";

                // 組合字串
                CopyRights = $"Version {version}";
            }
            catch (Exception)
            {
                // 預設值
                CopyRights = "Version 2.4.0.19";
            }
        }

        // Server 狀態變更時需要被通知重新渲染的 UI 屬性
        private void RefreshServerStatusUI()
        {
            OnPropertyChanged(nameof(IsServerRunning));
            OnPropertyChanged(nameof(ServerButtonText));
        }

        // 錯誤跳窗
        private void ShowErrorMessage(string message, string title, System.Windows.MessageBoxImage icon = System.Windows.MessageBoxImage.Error)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, icon);
            });
        }

        // 確認跳窗
        private bool ConfirmDialog(string message, string title)
        {
            var result = System.Windows.MessageBox.Show(
                message,
                title,
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            return result == System.Windows.MessageBoxResult.Yes;
        }
    }
}