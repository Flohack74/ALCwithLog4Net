using System.Drawing;
using log4net;

namespace Plugin
{
    public class TestPlugin
    {
        private ILog _testLogger;

        private static Size IconSize => new Size(18, 18);

        public TestPlugin()
        {
            _testLogger = LogManager.GetLogger("PluginLogger");
            _testLogger.Debug("Just a test");
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
            _testLogger = null!;
        }
    }
}
