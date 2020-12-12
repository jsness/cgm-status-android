using CgmStatusBar.Models;
using System.Collections.Generic;

namespace CgmStatusBar.Interfaces
{
    public interface ICgmDataCallback
    {
        public void OnCgmDataChange(IEnumerable<CgmEntry> entries);
    }
}
