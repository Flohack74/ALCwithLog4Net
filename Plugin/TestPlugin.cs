using System.Drawing;
using DevExpress.Utils;
using DevExpress.Utils.Svg;
using log4net;

namespace Plugin
{
    public class TestPlugin
    {
        private ILog _testLogger;

        private SvgImageCollection _icons;

        private static Size IconSize => new Size(18, 18);

        public TestPlugin()
        {
            _testLogger = LogManager.GetLogger("PluginLogger");
            _testLogger.Debug("Just a test");
            _icons = new SvgImageCollection { ImageSize = IconSize };
            _icons.Add("Test", new SvgImage());
        }

        public string TestMe(string input)
        {
            var resolvedMsg = $"Plugin received: {input}";
            _testLogger.Info(resolvedMsg);
            return resolvedMsg;
        }

        public void Shutdown()
        {
            LogManager.Shutdown();
            _icons.Clear();
            _icons = null!;
            _testLogger = null!;
        }
    }
}
