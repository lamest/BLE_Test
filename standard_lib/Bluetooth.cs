using System;
using System.Threading;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Xamarin.Forms;

namespace standard_lib
{
    public static class Bluetooth
    {
        private static readonly Lazy<IBluetoothLE> Implementation = new Lazy<IBluetoothLE>(CreateImplementation, LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        ///     Current bluetooth LE implementation.
        /// </summary>
        public static IBluetoothLE Current
        {
            get
            {
                var ret = Implementation.Value;
                if (ret == null) throw NotImplementedInReferenceAssembly();
                return ret;
            }
        }

        private static IBluetoothLE CreateImplementation()
        {
#if PORTABLE
            return null;
#else
            var implementation = DependencyService.Get<BleImplementationBase>();
            implementation.Initialize();
            return implementation;
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException(
                "This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}