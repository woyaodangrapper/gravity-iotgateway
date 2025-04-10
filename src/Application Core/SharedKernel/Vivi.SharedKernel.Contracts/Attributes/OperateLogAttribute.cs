﻿namespace Vivi.SharedKernel.Application.Contracts.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class OperateLogAttribute : Attribute
{
    public string LogName { get; set; }
}