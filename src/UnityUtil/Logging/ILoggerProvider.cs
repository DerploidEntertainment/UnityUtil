using UnityEngine;

namespace UnityUtil.Logging;

public interface ILoggerProvider
{
    public ILogger GetLogger(object source);
}
