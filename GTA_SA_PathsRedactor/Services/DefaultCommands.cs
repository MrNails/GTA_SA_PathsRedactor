using System.Windows.Input;

namespace GTA_SA_PathsRedactor.Services
{
    public static class DefaultCommands
    {
        static DefaultCommands()
        {
            Undo = new RoutedCommand("Undo", typeof(MainWindow));
            Redo = new RoutedCommand("Redo", typeof(MainWindow));
            DeleteSelectedPoints = new RoutedCommand("DeleteSelectedPoints", typeof(MainWindow));
            SaveCurrentPath = new RoutedCommand("SaveCurrentPath", typeof(MainWindow));
            SaveCurrentPathAs = new RoutedCommand("SaveCurrentPathAs", typeof(MainWindow));
            Help = new RoutedCommand("Help", typeof(MainWindow));
            About = new RoutedCommand("About", typeof(MainWindow));
        }

        public static RoutedCommand Undo { get; set; }
        public static RoutedCommand Redo { get; set; }
        public static RoutedCommand DeleteSelectedPoints { get; set; }
        public static RoutedCommand SaveCurrentPath { get; set; }
        public static RoutedCommand SaveCurrentPathAs { get; set; }
        public static RoutedCommand Help { get; set; }
        public static RoutedCommand About { get; set; }
    }
}
