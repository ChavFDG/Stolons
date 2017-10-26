using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models.Messages
{
    public class ChatMessage : Message
    {
        public ChatMessage()
        {

        }

        public ChatMessage(AdherentStolon publishBy, string content)
        {
            this.PublishBy = publishBy;
            this.Content = content;
            DateOfPublication = DateTime.Now;
        }
    }
}
