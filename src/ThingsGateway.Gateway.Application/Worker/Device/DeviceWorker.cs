﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Furion.Logging.Extensions;

using Mapster;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ThingsGateway.Core.Extension;
using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备服务
/// </summary>
public abstract class DeviceWorker : BackgroundService
{
    protected ILogger _logger;

    /// <summary>
    /// 全部重启锁
    /// </summary>
    protected readonly EasyLock restartLock = new();

    /// <summary>
    /// 单个重启锁
    /// </summary>
    protected readonly EasyLock singleRestartLock = new();

    protected IPluginService PluginService;
    protected GlobalData GlobalData;
    protected ManagementWoker ManagementWoker;
    protected IServiceScope _serviceScope;
    private readonly IHostApplicationLifetime _appLifetime;

    /// <inheritdoc/>
    public DeviceWorker(IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime appLifetime)
    {
        _serviceScope = serviceScopeFactory.CreateScope();
        _appLifetime = appLifetime;
    }

    /// <summary>
    /// 插件列表
    /// </summary>
    public IEnumerable<DriverBase> DriverBases => ChannelThreads.SelectMany(a => a.GetDriverEnumerable()).Where(a => a.CurrentDevice != null).OrderByDescending(a => a.CurrentDevice.DeviceStatus);

    /// <summary>
    /// 设备子线程列表
    /// </summary>
    protected ConcurrentList<ChannelThread> ChannelThreads { get; set; } = new();

    /// <summary>
    /// 控制设备线程启停
    /// </summary>
    /// <param name="deviceId">传入0时全部设备都会执行</param>
    /// <param name="isStart"></param>
    /// <returns></returns>
    public void PasueThread(long deviceId, bool isStart)
    {
        if (deviceId == 0)
            DriverBases.ForEach(a => a.PasueThread(isStart));
        else
            DriverBases.FirstOrDefault(it => it.DeviceId == deviceId)?.PasueThread(isStart);
    }

    #region protected

    /// <summary>
    /// 根据设备生成/获取通道线程管理器
    /// </summary>
    /// <param name="driverBase"></param>
    /// <returns></returns>
    protected ChannelThread GetChannelThread(DriverBase driverBase)
    {
        lock (this)
        {
            long channelId = driverBase.CurrentDevice.ChannelId;
            var channelThread = ChannelThreads.FirstOrDefault(t => t.ChannelId == channelId);
            if (channelThread != null)
            {
                if (channelThread.IsCollect != driverBase.IsCollect)
                {
                    _logger.LogWarning($"设备{driverBase.DeviceName}与通道{channelId}的其他设备类型不相同，不能选择同一个通道");
                    return null;
                }
                channelThread.AddDriver(driverBase);
                return channelThread;
            }

            return NewChannelThread(driverBase, channelId);

            ChannelThread NewChannelThread(DriverBase driverBase, long channelId)
            {
                var channelService = _serviceScope.ServiceProvider.GetService<IChannelService>();
                var channel = channelService.GetChannelById(channelId);
                if (channel == null)
                {
                    _logger.LogWarning($"设备{driverBase.CurrentDevice.Name}-通道{channelId}不能为null");
                    return null;
                }
                if (!channel.Enable)
                {
                    _logger.LogWarning($"设备{driverBase.CurrentDevice.Name}-通道{channelId}-{channel.Name}未启用");
                    return null;
                }
                ArgumentNullException.ThrowIfNull(channel, "");
                ChannelThread channelThread = new ChannelThread(channel, (a =>
                {
                    return channelService.GetChannel(channel, a);
                }));
                channelThread.AddDriver(driverBase);
                ChannelThreads.Add(channelThread);
                return channelThread;
            }
        }
    }

    /// <summary>
    /// 删除通道线程，并且释放资源
    /// </summary>
    protected async Task RemoveAllChannelThreadAsync(bool isRemoveDevice)
    {
        await BeforeRemoveAllChannelThreadAsync();

        await ChannelThreads.ParallelForEachAsync(async (channelThread, cancellationToken) =>
        {
            try
            {
                await channelThread.StopThreadAsync(isRemoveDevice);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, channelThread.ToString());
            }
        }, Environment.ProcessorCount / 2);
        if (isRemoveDevice)
            ChannelThreads.Clear();
    }

    protected async Task BeforeRemoveAllChannelThreadAsync()
    {
        _ = Task.Run(() =>
        {
            ChannelThreads.ForEach((a) =>
          {
              try
              {
                  a.BeforeStopThread();
              }
              catch (Exception ex)
              {
                  _logger?.LogError(ex, a.ToString());
              }
          });
        });

        await Task.Delay(100);
    }

    /// <summary>
    /// 开始通道线程
    /// </summary>
    /// <returns></returns>
    protected async Task StartAllChannelThreadsAsync()
    {
        if (!_stoppingToken.IsCancellationRequested)
        {
            foreach (var item in ChannelThreads)
            {
                if (!_stoppingToken.IsCancellationRequested)
                {
                    await StartChannelThreadAsync(item);
                }
            }
        }
    }

    private async Task StartChannelThreadAsync(ChannelThread item)
    {
        if (item.IsCollect)
        {
            if (ManagementWoker.IsStart)
            {
                //采集设备启动
                await item.StartThreadAsync();
            }
        }
        else
        {
            if (ManagementWoker.IsStart)
            {
                //业务设备启动
                await item.StartThreadAsync();
            }
            else
            {
                if (ManagementWoker.Options.Redundancy.IsStartBusinessDevice)
                {
                    //业务设备启动
                    await item.StartThreadAsync();
                }
            }
        }
    }

    #endregion protected

    #region 单个重启

    private void SetRedundantDevice(DeviceRunTime? dev, Device? newDev)
    {
        dev.DevicePropertys = newDev.DevicePropertys;
        dev.Description = newDev.Description;
        dev.ChannelId = newDev.ChannelId;
        dev.Enable = newDev.Enable;
        dev.IntervalTime = newDev.IntervalTime;
        dev.Name = newDev.Name;
        dev.PluginName = newDev.PluginName;
    }

    /// <summary>
    /// 更新设备线程
    /// </summary>
    public async Task RestartChannelThreadAsync(long deviceId, bool isChanged = true)
    {
        try
        {
            await singleRestartLock.WaitAsync();

            if (!_stoppingToken.IsCancellationRequested)
            {
                if (isChanged)
                    await ProtectedStoping();
                var channelThread = ChannelThreads.FirstOrDefault(it => it.Has(deviceId)) ?? throw new($"更新设备线程失败，不存在{deviceId}为id的设备");

                var dev = isChanged ? (await GetDeviceRunTimeAsync(deviceId)).FirstOrDefault() : channelThread.GetDriver(deviceId).CurrentDevice;

                //先停止采集，操作会使线程取消，需要重新恢复线程
                await channelThread.RemoveDriverAsync(deviceId);
                if (isChanged)
                    await ProtectedStoped();
                if (dev != null)
                {
                    //初始化
                    DriverBase newDriverBase = dev.CreatDriver(PluginService);
                    var newChannelThread = GetChannelThread(newDriverBase);
                    if (newChannelThread != null)
                    {
                        if (isChanged)
                            await ProtectedStarting();
                        try
                        {
                            await StartChannelThreadAsync(newChannelThread);
                            if (isChanged)
                                await ProtectedStarted();
                        }
                        finally
                        {
                            if (isChanged)
                                await ProtectedStarted();
                        }
                    }
                    else
                    {
                        if (isChanged)
                            await ProtectedStarting();
                        if (isChanged)
                            await ProtectedStarted();
                    }
                }
                else
                {
                    if (isChanged)
                        await ProtectedStarting();
                    if (isChanged)
                        await ProtectedStarted();
                }
            }
        }
        finally
        {
            singleRestartLock.Release();
        }
    }

    /// <summary>
    /// 更新设备线程,切换为冗余通道
    /// </summary>
    public async Task DeviceRedundantThreadAsync(long deviceId)
    {
        try
        {
            await singleRestartLock.WaitAsync();

            if (!_stoppingToken.IsCancellationRequested)
            {
                var channelThread = ChannelThreads.FirstOrDefault(it => it.Has(deviceId)) ?? throw new($"更新设备线程失败，不存在{deviceId}为id的设备");
                //这里先停止采集，操作会使线程取消，需要重新恢复线程
                await channelThread.RemoveDriverAsync(deviceId);

                var dev = channelThread.GetDriver(deviceId).CurrentDevice;

                if (dev.IsRedundant)
                {
                    if (dev.RedundantType == RedundantTypeEnum.Standby)
                    {
                        var newDev = _serviceScope.ServiceProvider.GetService<IDeviceService>().GetDeviceById(deviceId);
                        if (dev == null)
                        {
                            _logger.LogError($"更新设备线程失败，不存在{deviceId}为id的设备");
                        }
                        else
                        {
                            //冗余切换时，改变全部属性，但不改变变量信息
                            SetRedundantDevice(dev, newDev);
                            dev.RedundantType = RedundantTypeEnum.Primary;
                            _logger?.LogInformation($"{dev.Name}：切换到主通道");
                        }
                    }
                    else
                    {
                        try
                        {
                            var newDev = _serviceScope.ServiceProvider.GetService<IDeviceService>().GetDeviceById(dev.RedundantDeviceId);
                            if (newDev == null)
                            {
                                _logger.LogError($"更新设备线程失败，不存在{deviceId}为id的设备");
                            }
                            else
                            {
                                SetRedundantDevice(dev, newDev);
                                dev.RedundantType = RedundantTypeEnum.Standby;
                                _logger?.LogInformation($"{dev.Name}：切换到备用通道");
                            }
                        }
                        catch
                        {
                        }
                    }
                }

                //初始化
                DriverBase newDriverBase = dev.CreatDriver(PluginService);
                var newChannelThread = GetChannelThread(newDriverBase);
                if (newChannelThread != null && newChannelThread.DriverTask == null)
                {
                    await StartChannelThreadAsync(newChannelThread);
                }
            }
        }
        finally
        {
            singleRestartLock.Release();
        }
    }

    #endregion 单个重启

    #region 设备信息获取

    /// <summary>
    /// GetDebugUI
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    public Type GetDebugUI(string pluginName)
    {
        var driverPlugin = PluginService.GetDriver(pluginName);
        driverPlugin?.SafeDispose();
        return driverPlugin?.DriverDebugUIType;
    }

    /// <summary>
    /// GetDriverUI
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    public Type GetDriverUI(string pluginName)
    {
        var driverPlugin = PluginService.GetDriver(pluginName);
        driverPlugin?.SafeDispose();
        return driverPlugin?.DriverUIType;
    }

    /// <summary>
    /// 获取设备方法
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    public List<DependencyProperty> GetDeviceMethodInfos(long deviceId)
    {
        var pluginName = (_serviceScope.ServiceProvider.GetService<IDeviceService>().GetDeviceById(deviceId))?.PluginName;
        if (!pluginName.IsNullOrEmpty())
        {
            var propertys = PluginService.GetDriverMethodInfos(pluginName).Values;
            return propertys.ToList().Adapt<List<DependencyProperty>>();
        }
        else
        {
            return new();
        }
    }

    /// <summary>
    /// 获取设备属性，传入设备Id，相同名称的属性值会被重写
    /// </summary>
    /// <param name="driverId"></param>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    public List<DependencyProperty> GetDevicePropertys(string pluginName, long deviceId = 0)
    {
        var propertys = PluginService.GetDriverPropertyTypes(pluginName).Values.ToList().Adapt<List<DependencyProperty>>();
        if (deviceId != 0)
        {
            var collectDevice = _serviceScope.ServiceProvider.GetService<IDeviceService>().GetDeviceById(deviceId);
            collectDevice?.DevicePropertys?.ForEach(it =>
            {
                var dependencyProperty = propertys.FirstOrDefault(a => a.Name == it.Name);
                if (dependencyProperty != null && !it.Value.IsNullOrEmpty())
                {
                    dependencyProperty.Value = it.Value;
                }
            });
        }
        return propertys;
    }

    #endregion 设备信息获取

    #region worker服务

    protected EasyLock _easyLock = new(false);

    /// <summary>
    /// 在软件关闭时取消
    /// </summary>
    protected CancellationToken _stoppingToken;

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(() => { _easyLock.Release(); _easyLock = null; });
        await base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// 线程检查时间，10分钟
    /// </summary>
    public const int CheckIntervalTime = 600;

    public event RestartEventHandler Stoping;

    public event RestartEventHandler Stoped;

    public event RestartEventHandler Started;

    public event RestartEventHandler Starting;

    private volatile bool otherstarted = false;

    protected async Task ProtectedStarted()
    {
        try
        {
            if (!otherstarted)
                if (ManagementWoker.IsStart || ManagementWoker.IsStartBusinessDevice)
                    if (Started != null)
                        await Started.Invoke();
        }
        finally
        {
            otherstarted = true;
            otherstoped = false;
        }
    }

    protected async Task ProtectedStarting()
    {
        if (Starting != null)
            await Starting.Invoke();
    }

    protected async Task ProtectedStoped()
    {
        try
        {
            if (!otherstoped)
                if (Stoped != null)
                    await Stoped.Invoke();
        }
        finally
        {
            otherstoped = true;
            otherstarted = false;
        }
    }

    protected async Task ProtectedStoping()
    {
        if (Stoping != null)
            await Stoping.Invoke();
    }

    protected abstract Task<IEnumerable<DeviceRunTime>> GetDeviceRunTimeAsync(long deviceId);

    protected virtual async Task WhileExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                //每5分钟检测一次
                await Task.Delay(300000, stoppingToken);

                //检测设备线程假死
                var data = DriverBases.ToList();
                var num = data.Count;
                for (int i = 0; i < num; i++)
                {
                    DriverBase driverBase = data[i];
                    try
                    {
                        if (driverBase.CurrentDevice != null)
                        {
                            //冗余切换
                            if (driverBase.CurrentDevice.DeviceStatus == DeviceStatusEnum.OffLine && driverBase.IsBeforStarted)
                            {
                                if (driverBase.CurrentDevice.IsRedundant && _serviceScope.ServiceProvider.GetService<DeviceService>().IsAny(a => a.Id == driverBase.CurrentDevice.RedundantDeviceId))
                                {
                                    await DeviceRedundantThreadAsync(driverBase.CurrentDevice.Id);
                                }
                            }

                            //线程卡死检测
                            if ((driverBase.CurrentDevice.ActiveTime != null && driverBase.CurrentDevice.ActiveTime.Value.AddMinutes(CheckIntervalTime) <= DateTimeUtil.Now)
                                || (driverBase.IsInitSuccess == false && driverBase.IsBeforStarted))
                            {
                                //如果线程处于暂停状态，跳过
                                if (driverBase.CurrentDevice.DeviceStatus == DeviceStatusEnum.Pause)
                                    continue;
                                //如果初始化失败
                                if (!driverBase.IsInitSuccess)
                                    _logger?.LogWarning($"{driverBase.CurrentDevice.Name}初始化失败，重启线程中");
                                else
                                    _logger?.LogWarning($"{driverBase.CurrentDevice.Name}采集线程假死，重启线程中");
                                //重启线程
                                await RestartChannelThreadAsync(driverBase.CurrentDevice.Id, false);
                                break;
                            }
                            else
                            {
                                _logger?.LogTrace($"{driverBase.CurrentDevice.Name}线程检测正常");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "设备线程检测出现错误");
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "设备线程检测错误");
            }
        }
    }

    #endregion worker服务

    #region 全部重启

    private EasyLock publicRestartLock = new();

    public async Task RestartAsync()
    {
        try
        {
            await publicRestartLock.WaitAsync();

            await StopAsync(true);
            await StartAsync();
        }
        finally
        {
            publicRestartLock.Release();
        }
    }

    protected volatile bool started;

    /// <summary>
    /// 启动全部设备，如果没有找到设备会创建
    /// </summary>
    public async Task StartAsync()
    {
        try
        {
            await restartLock.WaitAsync();
            await singleRestartLock.WaitAsync();
            if (stoped)
            {
                ChannelThreads.Clear();
                await CreatAllChannelThreadsAsync();
                await ProtectedStarting();
            }
            await StartAllChannelThreadsAsync();
            await ProtectedStarted();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动发生错误");
        }
        finally
        {
            started = true;
            stoped = false;
            otherstoped = false;
            singleRestartLock.Release();
            restartLock.Release();
        }
    }

    /// <summary>
    /// 初始化，如果没有找到设备会创建
    /// </summary>
    public async Task CreatThreadsAsync()
    {
        try
        {
            await restartLock.WaitAsync();
            await singleRestartLock.WaitAsync();
            if (stoped)
            {
                ChannelThreads.Clear();
                await CreatAllChannelThreadsAsync();
                await ProtectedStarting();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动发生错误");
        }
        finally
        {
            started = true;
            stoped = false;
            otherstoped = false;
            singleRestartLock.Release();
            restartLock.Release();
        }
    }

    /// <summary>
    /// 停止
    /// </summary>
    public async Task StopAsync(bool isRemoveDevice)
    {
        try
        {
            await restartLock.WaitAsync();
            await singleRestartLock.WaitAsync();
            await StopThreadAsync(isRemoveDevice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停止错误");
        }
        finally
        {
            singleRestartLock.Release();
            restartLock.Release();
        }
    }

    protected async Task StopThreadAsync(bool isRemoveDevice = true)
    {
        if (started)
        {
            //取消全部采集线程
            await BeforeRemoveAllChannelThreadAsync();
            if (isRemoveDevice)
                //取消其他后台服务
                await ProtectedStoping();
            //停止全部采集线程
            await RemoveAllChannelThreadAsync(isRemoveDevice);
            if (isRemoveDevice)
                //停止其他后台服务
                await ProtectedStoped();
            //清空内存列表
        }
        started = false;
        otherstarted = false;
        stoped = true;
    }

    private volatile bool otherstoped = true;
    protected volatile bool stoped = true;

    #endregion 全部重启

    #region 读取数据库

    /// <summary>
    /// 读取数据库，创建全部设备
    /// </summary>
    /// <returns></returns>
    protected abstract Task CreatAllChannelThreadsAsync();

    #endregion 读取数据库
}

public delegate Task RestartEventHandler();