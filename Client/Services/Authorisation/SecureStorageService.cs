using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;
using Client.Interfaces.Authorisation;
// Vp_todo: This is where you will create the method for "Remember me" or "Stay signed in"

namespace Client.Services.Authorisation
{
    public class SecureStorageService : ISecureStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string EncryptionKey = "YourHardcodedKeyHere"; // Vp_todo: Not perfectly secure, but better than storing in a JSON file

        public SecureStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        // Session Storage Methods
        public async Task SetAsync<T>(string key, T value)
        {
            var jsonValue = JsonSerializer.Serialize(value);
            var encryptedValue = SimpleEncrypt(jsonValue);
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", key, encryptedValue);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var encryptedValue = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", key);
            if (string.IsNullOrEmpty(encryptedValue)) return default;
            var jsonValue = SimpleDecrypt(encryptedValue);
            return jsonValue != null ? JsonSerializer.Deserialize<T>(jsonValue) : default;
        }

        public async Task RemoveAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", key);
        }

        // Local Storage Methods
        public async Task SetLocalAsync<T>(string key, T value)
        {
            var jsonValue = JsonSerializer.Serialize(value);
            var encryptedValue = SimpleEncrypt(jsonValue);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, encryptedValue);
        }

        public async Task<T?> GetLocalAsync<T>(string key)
        {
            var encryptedValue = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            if (string.IsNullOrEmpty(encryptedValue)) return default;
            var jsonValue = SimpleDecrypt(encryptedValue);
            return jsonValue != null ? JsonSerializer.Deserialize<T>(jsonValue) : default;
        }

        public async Task RemoveLocalAsync(string key)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }

        // Encryption and Decription (Vp_todo: Improve encoding level 256?)
        private string SimpleEncrypt(string text)
        {
            var textBytes = Encoding.UTF8.GetBytes(text);
            var keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);
            var result = new byte[textBytes.Length];
            for (int i = 0; i < textBytes.Length; i++)
            {
                result[i] = (byte)(textBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }
            return Convert.ToBase64String(result);
        }

        private string SimpleDecrypt(string encryptedText)
        {
            var textBytes = Convert.FromBase64String(encryptedText);
            var keyBytes = Encoding.UTF8.GetBytes(EncryptionKey);
            var result = new byte[textBytes.Length];
            for (int i = 0; i < textBytes.Length; i++)
            {
                result[i] = (byte)(textBytes[i] ^ keyBytes[i % keyBytes.Length]);
            }
            return Encoding.UTF8.GetString(result);
        }
    }
}