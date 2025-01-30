using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

public class QRHandler : MonoBehaviour
{
    [SerializeField] TMP_Text _qRText;
    [SerializeField] string _qRFolderName;

    List<string> _jpgFiles = new List<string>();
    QRCodeReader _qRReader;
    string _qRFolderPath;
    int _qRFileCount = 0;

    void Start()
    {
        _qRReader = new QRCodeReader();

        _qRFolderPath = Path.Combine(Application.streamingAssetsPath, _qRFolderName);
    }

    public void CheckQR()
    {
        if (Directory.Exists(_qRFolderPath))
        {
            var jpegFilesInFolder = Directory.GetFiles(_qRFolderPath, "*.jpg").ToArray();

            if (jpegFilesInFolder.Length > 0)
            {
                // Add only new files that are not already in _jpgFiles
                foreach (var file in jpegFilesInFolder)
                {
                    if (!_jpgFiles.Contains(file))
                    {
                        _jpgFiles.Add(file);
                    }

                    if (_jpgFiles.Count > _qRFileCount)
                    {
                        _qRFileCount = _jpgFiles.Count;
                        LoadTexture();
                    }
                }
            }
            else
            {
                _qRText.text = "No files in the folder";
            }
        }
        else
        {
            _qRText.text = $"The folder does not exist: {_qRFolderPath}";
        }
    }

    void LoadTexture()
    {
        string filePath = System.IO.Path.Combine(_jpgFiles.Last());

        Texture2D texture = GetTextureFromFile(filePath);
        string qrScanedString = DecodeQRCode(texture);

        print(qrScanedString);
        _qRText.text = qrScanedString;
    }

    Texture2D GetTextureFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Texture file not found at path: {filePath}");
            return null;
        }

        byte[] fileData = File.ReadAllBytes(filePath);

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false); // Default shape is 2D

        if (texture.LoadImage(fileData))
        {
            texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);
            return texture;
        }
        else
        {
            Debug.LogError("Failed to load texture.");
            return null;
        }
    }

    string DecodeQRCode(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError("No texture provided for QR code decoding.");
            return null;
        }

        try
        {
            // Convert Texture2D to RGB24 byte array
            byte[] rgbRawBytes = ConvertToRGB24(texture);
            //Rect region = new Rect(0, texture.height - 300, 300, 300);

            // Create a LuminanceSource from the byte array
            var luminanceSource = new RGBLuminanceSource(rgbRawBytes, texture.width, texture.height, RGBLuminanceSource.BitmapFormat.RGB24);

            // Create a BinaryBitmap for decoding
            var binaryBitmap = new BinaryBitmap(new HybridBinarizer(luminanceSource));

            // Decode the QR code
            var result = _qRReader.decode(binaryBitmap);

            if (result != null)
            {
                Debug.Log($"QR Code detected: {result.Text}");
                return result.Text;
            }
            else
            {
                Debug.Log("No QR code detected in the image.");
                return null;

            }
        }
        catch (ReaderException ex)
        {
            Debug.LogError($"Error decoding QR code: {ex.Message}");
            return null;

        }
    }

    private byte[] ConvertToRGB24(Texture2D texture)
    {
        Color32[] colors = texture.GetPixels32();
        byte[] rgbData = new byte[colors.Length * 3]; // Each pixel takes 3 bytes (R, G, B)

        for (int i = 0; i < colors.Length; i++)
        {
            rgbData[i * 3] = colors[i].r;       // Red
            rgbData[i * 3 + 1] = colors[i].g;  // Green
            rgbData[i * 3 + 2] = colors[i].b;  // Blue
        }

        return rgbData;
    }
}
