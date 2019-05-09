using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QEthics
{
    /// <summary>
    /// Makes a Grower be able to be maintained.
    /// </summary>
    public interface IMaintainableGrower
    {
        float ScientistMaintenance { get; set; }
        float DoctorMaintenance { get; set; }
        float RoomCleanliness { get; }
    }
}
