﻿namespace Vivi.SharedKernel.ApiService.Registrar;

public abstract partial class AbstractWebApiDependencyRegistrar
{
    /// <summary>
    /// 注册 MiniProfiler 组件
    /// </summary>
    protected virtual void AddMiniProfiler() =>
        Services
        .AddMiniProfiler(options => options.RouteBasePath = $"/{ServiceInfo.ShortName}/profiler")
        .AddEntityFramework();
}
