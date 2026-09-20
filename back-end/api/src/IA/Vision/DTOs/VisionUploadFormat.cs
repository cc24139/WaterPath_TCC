using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.src.Vison.DTOs
{
    public class VisionUploaFormat
    {
        public string fileName { get; set; }
        public string formatBase64Data { get; set; }

        public VisionUploaFormat() { }
        public VisionUploaFormat(string nome = "")
        {
            fileName = nome;
        }

        public bool VerifyBase64(string image)
        {
            if (string.IsNullOrEmpty(image))
                return false;
            if (image.Contains(","))
                formatBase64Data = image.Split(",")[1];
            else
                formatBase64Data = image;
            Span<byte> buffer = new Span<byte>(new byte[formatBase64Data.Length]);
            if (!Convert.TryFromBase64String(formatBase64Data, buffer, out int byteWritten))
                return false;
            var imageByte = Convert.FromBase64String(formatBase64Data);
            if (!IsValidImageHeader(imageByte))
                return false;
            return true;
            //fazer parte de salvar imagem
         }
        
    private bool IsValidImageHeader(byte[] bytes)
    {
        if (bytes.Length < 4) return false;
        if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return true;
        if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47) return true;

        return false;
    }
    }
}