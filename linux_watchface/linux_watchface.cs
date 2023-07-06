using Tizen.Applications;
using Tizen.Wearable.CircularUI.Forms;
using Tizen.Wearable.CircularUI.Forms.Renderer.Watchface;
using Xamarin.Forms;

// check font file property (build action: embedded resource)
[assembly: ExportFont("JetBrainsMono.ttf")]

namespace linux_watchface
{
    internal class FormsLinuxWatchface: FormsWatchface
    {
        ViewModelMain vmMain;

        static void Main(string[] args)
        {
            var forms = new FormsLinuxWatchface();
            Forms.Init(forms);
            FormsCircularUI.Init();
            forms.Run(args);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            vmMain = new ViewModelMain();
            LoadWatchface(new LinuxWatchfaceApp { BindingContext = vmMain });
        }

        protected override void OnTick(TimeEventArgs args)
        {
            base.OnTick(args);
            vmMain.vmDateTime.Value = args.Time.UtcTimestamp;
        }

        protected override void OnAmbientTick(TimeEventArgs args)
        {
            base.OnAmbientTick(args);
            vmMain.vmDateTime.Value = args.Time.UtcTimestamp;
        }

        protected override void OnAmbientChanged(AmbientEventArgs args)
        {
            base.OnAmbientChanged(args);
            vmMain.vmAmbientModeDisabled.Value = !args.Enabled;
        }

        protected override void OnTerminate()
        {
            base.OnTerminate();
            vmMain.OnTerminate();
        }
    }
}
