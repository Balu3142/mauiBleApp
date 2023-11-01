using MAUIAndroidFS.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MAUIAndroidFS.Extensions;

namespace MAUIAndroidFS.Core
{
    public static class BluetoothConstants
    {
        public static Guid STATUS_SERVICE_UUID = 0x2202.UuidFromPartial();
        public static Guid STATUS_READ_UUID = 0xE628.UuidFromPartial();

        public static Guid COM_SERVICE_UUID = 0x2806.UuidFromPartial();
        public static Guid COM_READ_UUID = 0xE629.UuidFromPartial();
    }
}
