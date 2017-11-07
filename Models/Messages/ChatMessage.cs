using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

        [NotMapped]
        public string StringPublishBy
        {
            get
            {
                return "Par " + PublishBy.Adherent.Surname + " " + PublishBy.Adherent.Name + " le " + DateOfPublication.ToString();
            }
        }
    }
}
