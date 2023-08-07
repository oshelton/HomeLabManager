using System.Runtime.CompilerServices;
using YamlDotNet.Serialization;

[assembly: InternalsVisibleTo("HomeLabManager.DataTests")]

namespace HomeLabManager.Common.Data.Git.Server;

/// <summary>
/// DTO representing a Server Host's information.
/// </summary>
/// <remarks>This is for a physical server which can have one or more VMs also hosting services.</remarks>
public sealed record ServerHostDto : BaseServerDto
{
    /// <summary>
    /// List of VMs associated with this physical host.
    /// </summary>
    [YamlIgnore]
    public IReadOnlyList<ServerVmDto> VMs 
    {
        get => _vms;
        set
        {
            if (_vms != value)
            {
                _vms = value;
                if (_vms is null)
                    return;

                foreach (var vm in _vms) 
                {
                    vm.Host = this;
                }
            }
        }
    }

    private IReadOnlyList<ServerVmDto> _vms = Array.Empty<ServerVmDto>();
}
