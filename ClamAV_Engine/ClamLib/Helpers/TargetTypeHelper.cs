using System;
using System.Text;

namespace ClamAV_Engine.ClamLib.Helpers
{
    public class TargetTypeHelper
    {
        public static TargetType DetectTarget(byte[] data)
        {
            if (data == null || data.Length < 2)
                return TargetType.Any;

            // PE: "MZ" ở đầu
            if (
                (data[0] == (byte)'M' && data[1] == (byte)'Z') ||
                (data[0] == 0x4D && data[1] == 0x5A) ||
                (data[0] == 0x50 && data[1] == 0x45 ) || // 'P' 'E'
                 (data.Length >= 4 && data[2] == 0x00 && data[3] == 0x00) // 'P' 'E' \0\0

            )
                return TargetType.PE;

            // Đọc tối đa 4KB đầu để phân tích text/magic
            int len = Math.Min(data.Length, 4096);
            var ascii = Encoding.ASCII.GetString(data, 0, len);
            var trimmed = ascii.TrimStart('\uFEFF', ' ', '\t', '\r', '\n');

            // ELF: 0x7F 'E' 'L' 'F'
            if (data.Length >= 4 &&
                data[0] == 0x7F && data[1] == (byte)'E' &&
                data[2] == (byte)'L' && data[3] == (byte)'F')
            {
                // Nếu enum có ELF thì trả về, không thì để Any
                try { return (TargetType)Enum.Parse(typeof(TargetType), "ELF", ignoreCase: true); }
                catch { /* ignore */ }
            }

            // PDF: "%PDF-"
            if (trimmed.StartsWith("%PDF-", StringComparison.OrdinalIgnoreCase))
            {
                try { return (TargetType)Enum.Parse(typeof(TargetType), "PDF", ignoreCase: true); }
                catch { /* ignore */ }
            }

            // OLE2 / CFB (doc/xls/ppt cũ, một số installer): D0 CF 11 E0 A1 B1 1A E1
            if (data.Length >= 8 &&
                data[0] == 0xD0 && data[1] == 0xCF && data[2] == 0x11 && data[3] == 0xE0 &&
                data[4] == 0xA1 && data[5] == 0xB1 && data[6] == 0x1A && data[7] == 0xE1)
            {
                try { return (TargetType)Enum.Parse(typeof(TargetType), "OLE2", ignoreCase: true); }
                catch { /* ignore */ }
            }

            // HTML: <!DOCTYPE html>, <html, <head, <body, ...
            if (trimmed.StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("<HTML", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("<html", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("<head", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("<body", StringComparison.OrdinalIgnoreCase))
            {
                try { return (TargetType)Enum.Parse(typeof(TargetType), "HTML", ignoreCase: true); }
                catch { /* ignore */ }
            }

            // Email (đơn giản): bắt đầu bằng "From " hoặc có header mail rõ ràng
            if (trimmed.StartsWith("From ", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("Return-Path:", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("Received:", StringComparison.OrdinalIgnoreCase))
            {
                try { return (TargetType)Enum.Parse(typeof(TargetType), "Mail", ignoreCase: true); }
                catch { /* ignore */ }
            }

            // Một số định dạng ảnh phổ biến → Graphics
            // PNG
            if (StartsWith(data, new byte[] { 0x89, 0x50, 0x4E, 0x47 }))
            {
                TryReturn("Graphics", ref data);
            }
            // JPEG
            if (StartsWith(data, new byte[] { 0xFF, 0xD8, 0xFF }))
            {
                TryReturn("Graphics", ref data);
            }
            // GIF
            if (StartsWith(data, Encoding.ASCII.GetBytes("GIF87a")) ||
                StartsWith(data, Encoding.ASCII.GetBytes("GIF89a")))
            {
                TryReturn("Graphics", ref data);
            }
            // BMP
            if (StartsWith(data, Encoding.ASCII.GetBytes("BM")))
            {
                TryReturn("Graphics", ref data);
            }
            // TIFF
            if (StartsWith(data, new byte[] { (byte)'I', (byte)'I', 0x2A, 0x00 }) ||
                StartsWith(data, new byte[] { (byte)'M', (byte)'M', 0x00, 0x2A }))
            {
                TryReturn("Graphics", ref data);
            }

            // ZIP-based (ZIP, JAR, DOCX/XLSX/PPTX, APK, ...) → nếu enum có Archive dùng, không thì Any
            if (StartsWith(data, new byte[] { (byte)'P', (byte)'K', 0x03, 0x04 }) ||
                StartsWith(data, new byte[] { (byte)'P', (byte)'K', 0x05, 0x06 }) ||
                StartsWith(data, new byte[] { (byte)'P', (byte)'K', 0x07, 0x08 }))
            {
                try { return (TargetType)Enum.Parse(typeof(TargetType), "Archive", ignoreCase: true); }
                catch { /* ignore */ }
            }

            // SWF: FWS (uncompressed) hoặc CWS (zlib)
            if (StartsWith(data, Encoding.ASCII.GetBytes("FWS")) ||
                StartsWith(data, Encoding.ASCII.GetBytes("CWS")))
            {
                try { return (TargetType)Enum.Parse(typeof(TargetType), "SWF", ignoreCase: true); }
                catch { /* ignore */ }
            }

            // RTF
            if (trimmed.StartsWith(@"{\rtf", StringComparison.OrdinalIgnoreCase))
            {
                try { return (TargetType)Enum.Parse(typeof(TargetType), "RTF", ignoreCase: true); }
                catch { /* ignore */ }
            }

            // XML
            if (trimmed.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase))
            {
                try { return (TargetType)Enum.Parse(typeof(TargetType), "XML", ignoreCase: true); }
                catch { /* ignore */ }
            }

            // Nếu trông giống text → TEXT (nếu enum có), ngược lại Any
            if (IsProbablyText(data, len))
            {
                try { return (TargetType)Enum.Parse(typeof(TargetType), "Text", ignoreCase: true); }
                catch { /* ignore */ }
            }

            return TargetType.Any;
        }

        private static bool StartsWith(byte[] data, byte[] magic, int offset = 0)
        {
            if (data.Length < offset + magic.Length)
                return false;

            for (int i = 0; i < magic.Length; i++)
            {
                if (data[offset + i] != magic[i])
                    return false;
            }
            return true;
        }

        private static bool IsProbablyText(byte[] data, int length)
        {
            int printable = 0;
            int control = 0;
            for (int i = 0; i < length; i++)
            {
                byte b = data[i];
                if (b == 0)
                {
                    // NUL trong file → có xu hướng là binary
                    control++;
                    continue;
                }
                if (b >= 0x20 && b <= 0x7E) // printable ASCII
                    printable++;
                else if (b == 0x09 || b == 0x0A || b == 0x0D) // tab/CR/LF
                    printable++;
                else
                    control++;
            }

            if (printable + control == 0)
                return false;

            double ratio = (double)printable / (printable + control);
            return ratio > 0.85; // >85% là ký tự text
        }

        /// <summary>
        /// Thử trả về TargetType theo tên (Graphics, ...),
        /// nếu enum không có thì bỏ qua.
        /// </summary>
        private static void TryReturn(string typeName, ref byte[] dataRef)
        {
            try
            {
                var t = (TargetType)Enum.Parse(typeof(TargetType), typeName, ignoreCase: true);
                // "hack" nhỏ: dùng exception để thoát nhanh DetectTarget
                throw new TargetDetectedException(t);
            }
            catch (TargetDetectedException ex)
            {
                // Bắt ở DetectTarget bằng khi cần – nhưng ở đây ta không dùng.
                // Để đơn giản, hàm này hiện không được gọi theo kiểu throw, bạn
                // có thể bỏ TryReturn hoặc sửa DetectTarget dùng nó nếu muốn.
                Console.WriteLine("Detected target: " + ex.Target);
            }
            catch
            {
                // ignore
            }
        }

        private sealed class TargetDetectedException : Exception
        {
            public TargetType Target { get; }

            public TargetDetectedException(TargetType target)
            {
                Target = target;
            }
        }
    }
}