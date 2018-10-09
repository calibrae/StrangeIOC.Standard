using strange.extensions.context.api;
using Xamarin.Forms;

namespace StrangeForms.extensions.context.api
{
    public interface IXamContextView : IContextView
    {

        Page MainPage { get; set; }

    }
}