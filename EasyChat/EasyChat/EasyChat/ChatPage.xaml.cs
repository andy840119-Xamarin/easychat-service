using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EasyChat
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        private ChatPageViewModel vm;

        public ChatPage(string username)
        {
            InitializeComponent();

            BindingContext = vm = new ChatPageViewModel(username);
        }
    }
}