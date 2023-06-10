using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarSystem
{
    internal interface IAsset
    {
        public string AssetId { get; init; }

        public static string GetAssetBaseDir() { return ""; }
        public void Unload();
    }
}
