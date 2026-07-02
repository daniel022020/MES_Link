using CommunityToolkit.Mvvm.ComponentModel;

namespace MES_Link.MesSimulator
{
    // MES模擬器動態區塊的資料結構
    public class MesRouteBlock : ObservableObject
    {
        // UI自訂義接口名稱
        private string _tagName = "test";
        public string TagName
        {
            get => _tagName;
            set => SetProperty(ref _tagName, value);
        }

        // UI自訂義Route
        private string _routeUrl = "api/test";
        public string RouteUrl
        {
            get => _routeUrl;
            set => SetProperty(ref _routeUrl, value);
        }

        // UI選擇 ContentType
        private string _contentType = "application/json";
        public string ContentType
        {
            get => _contentType;
            set => SetProperty(ref _contentType, value);
        }

        // UI選擇 Delay回傳時間 (模擬延遲傳送)
        private int _delayMs = 0;
        public int DelayMs
        {
            get => _delayMs;
            set => SetProperty(ref _delayMs, value);
        }

        // 回傳格式_Format1
        private string _format1 = "{\n  \"status\": \"OK\",\n  \"msg\": \"pass\"\n}";
        public string Format1
        {
            get => _format1;
            set => SetProperty(ref _format1, value);
        }

        // UI選擇回傳的StatusCode，預設 200
        private int _statusCode_Format1 = 200;
        public int StatusCode_Format1
        {
            get => _statusCode_Format1;
            set => SetProperty(ref _statusCode_Format1, value);
        }

        // UI選擇的回傳格式_Format1
        private bool _isSelected_Format1 = true;
        public bool IsSelected_Format1
        {
            get => _isSelected_Format1;
            set
            {
                if (SetProperty(ref _isSelected_Format1, value))
                {
                    OnPropertyChanged(nameof(IsSelected_Format2));
                }
            }
        }

        // 回傳格式_Format2
        private string _format2 = "{\n  \"status\": \"NG\",\n  \"msg\": \"fail\"\n}";
        public string Format2
        {
            get => _format2;
            set => SetProperty(ref _format2, value);
        }

        // UI選擇回傳的StatusCode，預設 400
        private int _statusCode_Format2 = 400;
        public int StatusCode_Format2
        {
            get => _statusCode_Format2;
            set => SetProperty(ref _statusCode_Format2, value);
        }

        // UI選擇的回傳格式_Format2
        public bool IsSelected_Format2
        {
            get => !_isSelected_Format1;
            set
            {
                if (value)
                {
                    IsSelected_Format1 = false;
                }
            }
        }
    }
}