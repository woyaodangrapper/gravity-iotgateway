﻿// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components.Forms;

namespace ThingsGateway.Gateway.Application
{
    public interface IVariableRuntimeService
    {
        Task<bool> BatchEditAsync(IEnumerable<Variable> models, Variable oldModel, Variable model);
        Task<bool> DeleteVariableAsync(IEnumerable<long> ids);
        Task<Dictionary<string, object>> ExportVariableAsync(ExportFilter exportFilter);

        Task ImportVariableAsync(Dictionary<string, ImportPreviewOutputBase> input);
        Task InsertTestDataAsync(int testVariableCount, int testDeviceCount, string slaveUrl, bool restart = true);


        Task AddBatchAsync(List<Variable> input);

        Task<Dictionary<string, ImportPreviewOutputBase>> PreviewAsync(IBrowserFile browserFile);

        Task<bool> SaveVariableAsync(Variable input, ItemChangedType type);
        void PreheatCache();

        Task<MemoryStream> ExportMemoryStream(List<Variable> data, string devName);
        Task AddDynamicVariable(IEnumerable<VariableRuntime> newVariableRuntimes);
        Task DeleteDynamicVariable(IEnumerable<long> variableIds);
    }
}