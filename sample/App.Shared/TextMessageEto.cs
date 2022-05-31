using System;

namespace App.Shared
{
    [Serializable]
    public class TextMessageEto
    {
        public TextMessageEto(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}
