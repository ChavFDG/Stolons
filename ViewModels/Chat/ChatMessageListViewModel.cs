using Stolons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.ViewModels.Chat
{
    public class ChatMessageListViewModel : BaseViewModel
    {
        public List<Models.Messages.ChatMessage> Messages { get; set; }
        
        public ChatMessageListViewModel()
        {

        }
        public ChatMessageListViewModel(AdherentStolon activeAdherentStolon, List<Models.Messages.ChatMessage> messages)
        {
            Messages = messages;
            ActiveAdherentStolon = activeAdherentStolon;
        }
    }
}
