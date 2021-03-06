using MysticAmbient.Utils;
using System.Windows;

namespace MysticAmbient
{
    class SingleInstanceEntryPoint
    {
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "5.0.2.0")]
        public static void Main()
        {
            using (SingleInstanceMutex sim = new SingleInstanceMutex())
            {
                if (!sim.IsOtherInstanceRunning)
                {
                    App application = new App();
                    application.InitializeComponent();
                    application.Run();
                }
                else
                {
                    MessageBox.Show(
                        "Only one instance!",
                        "Error!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                        );

                    return;
                }
            }
        }
    }
}
