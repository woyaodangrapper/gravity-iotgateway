﻿#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 数据库写入器
/// </summary>
public class TGRunTimeDatabaseLoggingWriter : IDatabaseLoggingWriter
{
    private readonly SqlSugarScope _db;

    /// <inheritdoc cref="TGRunTimeDatabaseLoggingWriter"/>
    public TGRunTimeDatabaseLoggingWriter()
    {
        _db = DbContext.Db;

        Task.Factory.StartNew(LogInsertAsync);
    }
    private ConcurrentQueue<RuntimeLog> _logQueues = new();

    private async Task LogInsertAsync()
    {
        var db = _db.CopyNew();
        while (true)
        {
            if (_logQueues.Count > 0)
            {
                try
                {
                    var data = _logQueues.ToListWithDequeue();
                    db.InsertableWithAttr(data).ExecuteCommand();//入库
                }
                catch
                {
                }

            }

            await Task.Delay(3000);
        }
    }
    /// <inheritdoc/>
    public void Write(LogMessage logMsg, bool flush)
    {
        var customLevel = App.GetConfig<LogLevel?>("Logging:LogLevel:RunTimeLogCustom") ?? LogLevel.Trace;
        if (logMsg.LogLevel >= customLevel)
        {
            var logRuntime = new RuntimeLog
            {
                LogLevel = logMsg.LogLevel,
                LogMessage = logMsg.State.ToString(),
                LogSource = logMsg.LogName,
                LogTime = logMsg.LogDateTime.ToUniversalTime(),
                Exception = logMsg.Exception?.ToString(),
            };
            //_db.InsertableWithAttr(logRuntime).ExecuteCommand();//入库
            _logQueues.Enqueue(logRuntime);
        }


    }

}