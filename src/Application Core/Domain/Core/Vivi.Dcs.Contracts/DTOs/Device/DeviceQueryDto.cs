﻿namespace Vivi.Dcs.Contracts.DTOs;

/// <summary>
/// 智能设备DTO
/// </summary>
public class DeviceQueryDto : SearchPagedDto
{
    /// <summary>
    /// 设备名称，如中央空调、风机盘管等
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 设备型号
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// 设备生产厂家
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// 设备安装位置
    /// </summary>
    public string? InstallationLocation { get; set; }

    /// <summary>
    /// 设备状态，如 active（启用）、inactive（停用）、maintenance（维护）
    /// </summary>
    public string? Status { get; set; }
}