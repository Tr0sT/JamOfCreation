using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Noesis;

namespace NuclearBand.Game
{
    public class EmojiRichTextConverter : IValueConverter
    {
        private readonly Dictionary<string, string> _emojiMap = new()
        {
            {":energy:", "Assets/_Project/Resources/GUI/MainInterfaceWindow/Energy.png"},
            {":ideas:",  "Assets/_Project/Resources/GUI/MainInterfaceWindow/Ideas.png"},
            {":code:",   "Assets/_Project/Resources/GUI/MainInterfaceWindow/Code.png"},
            {":art:",    "Assets/_Project/Resources/GUI/MainInterfaceWindow/Art.png"}
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var input = value as string ?? string.Empty;
            var sb = new StringBuilder(input.Length + 32);

            // Очень простая обработка ConverterParameter: ожидаем только число, например "80".
            int imgHeight = 60; // значение по умолчанию
            if (parameter != null)
            {
                var ps = (parameter as string ?? parameter.ToString()).Trim();
                if (int.TryParse(ps, NumberStyles.Integer, CultureInfo.InvariantCulture, out var h))
                    imgHeight = h;
            }

            int start = 0;
            while (true)
            {
                int idx = input.IndexOf(':', start);
                if (idx == -1)
                {
                    if (start < input.Length) sb.Append(input.AsSpan(start));
                    break;
                }

                int end = input.IndexOf(':', idx + 1);
                if (end != -1)
                {
                    if (idx > start)
                        sb.Append(input.AsSpan(start, idx - start));

                    string token = input.Substring(idx, end - idx + 1);
                    if (_emojiMap.TryGetValue(token, out var spritePath))
                    {
                        // Вставляем указанную высоту (или значение по умолчанию)
                        sb.Append("[img height=")
                          .Append(imgHeight)
                          .Append("]")
                          .Append(spritePath)
                          .Append("[/img]");
                    }
                    else
                    {
                        sb.Append(token);
                    }

                    start = end + 1;
                }
                else
                {
                    sb.Append(input.AsSpan(start));
                    break;
                }
            }

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
