using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatioFIX.Common
{
    /// <summary>
    /// ExecInst (Tag = 18, Type: MultipleCharValue)
    /// Instructions for order handling on exchange trading floor.
    /// </summary>
    public enum ExecInstEnum : byte
    {
        /// <summary>
        /// 
        /// </summary>
        Default = 32,
        /// <summary>
        /// 'S' 	Suspend
        /// </summary>
        Suspend = 83/*S*/,
        /// <summary>
        /// 'q' 	Release from suspension (mutually exclusive with S)
        /// </summary>
        Unsuspend = 113/*q*/
    }
}
