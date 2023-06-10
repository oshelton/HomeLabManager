using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace HomeLabManager.Common.Data.Git.Server
{
    /// <summary>
    /// DTO representing a VM Based Server's information.
    /// </summary>
    /// <remarks>This is for a VM based server living on a physical machine which can have one or more services of their own.</remarks>
    public sealed record ServerVmDto : BaseServerDto 
    {
        /// <summary>
        /// Prefix to apply to the front of the unique id to identify VMs.
        /// </summary>
        public static readonly string UniqueIdPrefix = "vm-";

        /// <summary>
        /// Reference back to the host that owns this VM.
        /// </summary>
        [YamlIgnore]
        public ServerHostDto? Host { get; internal set; }

        /// <inheritdoc/>
        public override string? UniqueIdToDirectoryName() => $"{UniqueIdPrefix}{UniqueId?.ToString("D")}";
    }
}
