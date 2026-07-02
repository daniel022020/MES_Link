using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MES_Link.Log;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace MES_Link.MesSimulator
{
    public class MesSimulatorService
    {
        private HttpListener _listener;
        private bool _isRunning;
        public string BaseUrl { get; set; } = "http://localhost:8080/";

        // 存放使用者動態新增的 Block 區塊集合
        public ObservableCollection<MesRouteBlock> Routes { get; set; } = new ObservableCollection<MesRouteBlock>();

        // 用於跨執行緒鎖定 ObservableCollection 的鎖定物件
        public readonly object RoutesLock = new object();

        // 當模擬器收到連線或發生事件時，通知 ViewModel 的事件
        public event Action<string> OnSimulatorLogAppended;

        public bool IsRunning => _isRunning;

        // 啟動伺服器
        public bool Start()
        {
            if (_isRunning)
                return false;

            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(BaseUrl);
                _listener.Start();
                _isRunning = true;

                ShowLog(eLogType.INFO, $"MES Simulator Started at {BaseUrl}");

                Task.Run(ListenLoop);
                return true;
            }
            catch (Exception ex)
            {
                ShowLog(eLogType.FATAL, $"Failed to start server: {ex.Message}");
                _isRunning = false;
                return false;
            }
        }

        // 停止伺服器
        public bool Stop()
        {
            if (!_isRunning)
                return false;

            try
            {
                _isRunning = false;
                _listener?.Stop();
                _listener?.Close();
                ShowLog(eLogType.INFO, "MES Simulator Stopped.");

                return true;
            }
            catch (Exception ex)
            {
                ShowLog(eLogType.FATAL, $"Failed to stop server: {ex.Message}");
                _isRunning = false;
                return false;
            }
        }

        // 背景監聽迴圈
        private async Task ListenLoop()
        {
            while (_isRunning && _listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();

                    // 多執行緒處理每個客戶端連線
                    _ = Task.Run(async () => await ProcessRequestAsync(context));
                }
                catch (Exception ex) when (ex is HttpListenerException ||
                                           ex is ObjectDisposedException ||
                                           ex is InvalidOperationException)
                {
                    // 檢查是否是主動關閉，如果是就退出迴圈
                    if (!_isRunning)
                    {
                        break;
                    }

                    // 如果不是主動關閉，才記錄下來
                    ShowLog(eLogType.FATAL,  $"Unexpected listener error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // 擷取其他嚴重錯誤
                    ShowLog(eLogType.FATAL, $"Listen loop error: {ex.Message}");
                }
            }
        }

        // 處理 HTTP POST/GET 請求核心
        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                // 取得路徑
                string rawPath = request.Url.AbsolutePath.Trim('/');

                ShowLog(eLogType.INFO, $"Received request: {request.HttpMethod} /{rawPath}");

                // 讀取 Authorization 標頭
                string auth = request.Headers["Authorization"];

                if (!string.IsNullOrEmpty(auth))
                    ShowLog(eLogType.INFO, $"Authorization: {auth}");

                // 讀取請求內容
                string body = string.Empty;

                // 接 HttpPost 傳來的訊息
                if (request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    Encoding encoding;

                    if (request.ContentEncoding != null)
                    {
                        encoding = request.ContentEncoding;
                    }
                    else
                    {
                        encoding = Encoding.UTF8;
                    }

                    // POST: 從 InputStream 讀取 Body 內容
                    using (var reader = new StreamReader(request.InputStream, encoding))
                    {
                        body = await reader.ReadToEndAsync();
                    }

                    if (!string.IsNullOrEmpty(body))
                        ShowLog(eLogType.INFO, $"\n[GET REQUEST (POST)]:\n{body}");
                }
                // 接 HttpGet 傳來的訊息
                else if (request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    // GET: 從 QueryString 讀取網址參數
                    if (request.QueryString.Count > 0)
                    {
                        var queryPairs = request.QueryString.AllKeys.Select(k => $"{k}={request.QueryString[k]}");
                        body = string.Join("&", queryPairs);
                        ShowLog(eLogType.INFO, $"\n[GET REQUEST (GET)]:\n{body}");
                    }
                }

                // 匹配 Route
                MesRouteBlock matchedRoute = Routes.FirstOrDefault(r => r.RouteUrl.Equals(rawPath, StringComparison.OrdinalIgnoreCase));

                string responseText = string.Empty;

                if (matchedRoute != null)
                {
                    if (matchedRoute.IsSelected_Format1)
                    {
                        responseText = matchedRoute.Format1;
                        response.StatusCode = matchedRoute.StatusCode_Format1;
                    }
                    else
                    {
                        responseText = matchedRoute.Format2;
                        response.StatusCode = matchedRoute.StatusCode_Format2;
                    }

                    // 設定 ContentType
                    response.ContentType = matchedRoute.ContentType;

                    string side = matchedRoute.IsSelected_Format1 ? "Format 1" : "Format 2";
                    ShowLog(eLogType.INFO, $"Status Code: {response.StatusCode}. Responding with {side} \n[Response]:\n{responseText}");

                    // 模擬延遲回傳，設0則默認關閉功能
                    if (matchedRoute.DelayMs > 0)
                    {
                        ShowLog(eLogType.INFO, $"Simulating delay response for {rawPath}: Waiting {matchedRoute.DelayMs}ms...");
                        int delayMilliseconds = matchedRoute.DelayMs * 1000;
                        await Task.Delay(delayMilliseconds);
                    }
                }
                else
                {
                    // 找不到註冊的路徑
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.ContentType = "text/plain; charset=utf-8";
                    responseText = "Error 404: Route not registered. (Path: /" + rawPath + ")";
                    ShowLog(eLogType.ERROR, $"No matching URL path found: /{rawPath}");
                }

                // 回傳結果給 Client
                byte[] buffer = Encoding.UTF8.GetBytes(responseText);
                response.ContentEncoding = Encoding.UTF8;
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.OutputStream.Close();
                response.Close();

                ShowLog(eLogType.INFO, $"Response sent successfully for /{rawPath}.");
            }
            catch (Exception ex)
            {
                ShowLog(eLogType.FATAL, $"ProcessRequest error: {ex.Message}");
                try
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.OutputStream.Close();
                }
                catch { }
            }
        }

        // 寫Log，UI同步新增
        private void ShowLog(eLogType _eLogType, string message)
        {
            string formattedLog = $"{DateTime.Now:MM/dd HH:mm:ss.fff} [{_eLogType}] {message}{Environment.NewLine}";
            OnSimulatorLogAppended?.Invoke(formattedLog);

            // 依照類別寫Log
            switch (_eLogType)
            {
                case eLogType.INFO:
                    LogService.MesSimulatorLogger.Info(message);
                    break;
                case eLogType.WARN:
                    LogService.MesSimulatorLogger.Warn(message);
                    break;
                case eLogType.ERROR:
                    LogService.MesSimulatorLogger.Error(message);
                    break;
                case eLogType.FATAL:
                    LogService.MesSimulatorLogger.Fatal(message);
                    break;
            }
        }
    }
}