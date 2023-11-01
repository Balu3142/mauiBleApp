using Shiny.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using MAUIAndroidFS.Core;
using MAUIAndroidFS.Interfaces;
using MAUIAndroidFS.Structures;
using SystemTimer = System.Timers.Timer;
using Shiny;

namespace MAUIAndroidFS.Services
{

    public class NoneObserver : IObserver<BleCharacteristicResult>
    {
        public void OnCompleted()
        {
        }
        public void OnError(Exception error)
        {
        }

        public void OnNext(BleCharacteristicResult value)
        {
        }
    }

    public class ConsolObserver : IObserver<BleCharacteristicResult>
    {
        public void PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            Console.WriteLine(sb.ToString());
        }

        public void OnCompleted()
        {
            Console.WriteLine("OnCompleted");
        }
        public void OnError(Exception error)
        {
            Console.WriteLine($"OnError: {error}");
        }

        public void OnNext(BleCharacteristicResult value)
        {
            Console.WriteLine($"OnNext:");
            PrintByteArray(value.Data);
        }
    }
    public class BluetoothClient : IBluetoothClient
    {
        private readonly IBleManager _bleManager;
        private readonly ObservableList<BluetoothDevice> _devices = new ObservableList<BluetoothDevice>();

        private BluetoothDevice _connectedDevice;

        public BluetoothClient(IMauiInterface mauiInterface)
        {
            _bleManager = mauiInterface.Resolve(typeof(IBleManager)) as IBleManager;
        }

        public bool Connect(BluetoothDevice device)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            try
            {
                IPeripheral peripheral = (IPeripheral)device.Device;

                peripheral.WhenStatusChanged()
                    .Subscribe(_state =>
                    {
                        if (_state == ConnectionState.Connected)
                        {
                            _connectedDevice = device;
                            _ = tcs.TrySetResult(true);
                        }
                    });

                peripheral.ConnectAsync(timeout: TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _ = tcs.TrySetResult(false);
            }

            return tcs.Task.GetAwaiter().GetResult();
        }

        public bool Disconnect()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            try
            {
                IPeripheral? device = null;

                if (_connectedDevice != null)
                    device = (IPeripheral)_connectedDevice.Device;

                if (!(device!.Status == ConnectionState.Connected))
                {
                    _ = tcs.TrySetResult(false);
                }
                else
                {
                    device.WhenStatusChanged()
                        .Subscribe(_state =>
                        {
                            if (_state == ConnectionState.Disconnected)
                            {
                                _connectedDevice = null;
                                _ = tcs.TrySetResult(true);
                            }
                        });

                    device.CancelConnection();
                }
            }
            catch
            {
                _ = tcs.TrySetResult(false);
            }

            return tcs.Task.GetAwaiter().GetResult();
        }

        public void StartScan()
        {
            Console.WriteLine("Start scan.");
            if (_bleManager.IsScanning)
            {
                return;
            }

            _devices.Clear();

            _bleManager.Scan()
                .Subscribe(a =>
                {
                    if (_devices.Any(b => b.Uuid.Equals(a.Peripheral.Uuid)))
                        _devices.Remove(_devices.First(b => b.Uuid.Equals(a.Peripheral.Uuid)));

                    if (a.Peripheral != null &&
                        a.Peripheral.Uuid != null &&
                        a.Peripheral.Uuid.Equals("00000000-0000-0000-0000-ebf5710f0cf1"))
                    {
                        Console.WriteLine($"Playbrush found.");

                        _devices.Add(new BluetoothDevice()
                        {
                            Uuid = a.Peripheral.Uuid,
                            Device = a.Peripheral,
                            LocalName = a.Peripheral.Name,
                            Rssi = a.Rssi
                        });
                    }
                });
        }

        public void StopScan()
        {
            if (_bleManager.IsScanning)
            {
                Console.WriteLine("Stop scan.");
                _bleManager.StopScan();
            }
        }

        public void ReadSession()
        {
            var request = new byte[] { 0x40, 0xC8, 0x00, 0x28, 0x00, 0x32, 0xA3, 0x49, 0xA4, 0x00, 0x00, 0x00};
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

            Console.WriteLine("Average() called.");

            Task.Run(() =>
            {
                try
                {
                    IPeripheral device = (IPeripheral)_connectedDevice.Device;

                    Console.WriteLine("Enable notifications.");

                    device.NotifyCharacteristic(BluetoothConstants.STATUS_SERVICE_UUID.ToString(),
                        BluetoothConstants.STATUS_READ_UUID.ToString()).Subscribe(new NoneObserver());

                    //device.NotifyCharacteristic(BluetoothConstants.COM_SERVICE_UUID.ToString(),
                      //  BluetoothConstants.COM_READ_UUID.ToString());//.Subscribe(new ConsolObserver());

                    Console.WriteLine("Notifications enabled.");

                    //Task.Delay(TimeSpan.FromSeconds(4));

                    device.WriteCharacteristic(BluetoothConstants.COM_SERVICE_UUID.ToString(), BluetoothConstants.COM_READ_UUID.ToString(), request, false).Subscribe(new ConsolObserver());

                }
                catch
                {
                    _ = tcs.TrySetResult(0);
                }
            });

            //return tcs.Task.GetAwaiter().GetResult();
        }

        public ObservableList<BluetoothDevice> ScanResults => _devices;

        public BluetoothDevice ConnectedDevice => _connectedDevice;
    }
}
