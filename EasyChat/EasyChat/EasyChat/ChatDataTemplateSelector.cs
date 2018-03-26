using Xamarin.Forms;

namespace EasyChat
{
    internal class MyDataTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate incomingDataTemplate;
        private readonly DataTemplate outgoingDataTemplate;

        public MyDataTemplateSelector()
        {
            // Retain instances!
            incomingDataTemplate = new DataTemplate(typeof(IncomingViewCell));
            outgoingDataTemplate = new DataTemplate(typeof(OutgoingViewCell));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var messageVm = item as Message;
            if (messageVm == null)
                return null;
            return messageVm.IsIncoming ? incomingDataTemplate : outgoingDataTemplate;
        }
    }
}