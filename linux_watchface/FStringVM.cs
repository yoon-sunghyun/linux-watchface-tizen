using linux_watchface;
using Xamarin.Forms;

namespace linux_watchface
{
    internal class FStringVM: ViewModel<FormattedString>
    {
        public FStringVM(double fontSize, string t1, Color c1, string t2 = "", Color c2 = default)
        {
            Value = new FormattedString()
            {
                Spans = {
                    new Span() {
                        FontSize = fontSize,
                        Text = t1,
                        TextColor = c1
                    },
                    new Span() {
                        FontSize = fontSize,
                        Text = t2,
                        TextColor = c2
                    }
                }
            };
        }

        public void Edit(string text, Color textColor)
        {
            Value.Spans[1].Text = text;
            Value.Spans[1].TextColor = textColor;
        }
    }
}
