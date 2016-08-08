using NLog;

namespace SharedResources
{
    public class AppLogger
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public void Add(string msg)
        {
            logger.Info(msg);
        }

    }
}
