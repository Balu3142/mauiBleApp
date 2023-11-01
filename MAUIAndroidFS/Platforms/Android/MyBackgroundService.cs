using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using IntelliJ.Lang.Annotations;
using Microsoft.AspNetCore.SignalR.Client;
using MAUIAndroidFS.Core;
using MAUIAndroidFS.Interfaces;
using MAUIAndroidFS.Structures;

namespace MAUIAndroidFS.Platforms.Android;

[Service]
internal class MyBackgroundService : Service
{
    Timer timer = null;
    int myId = (new object()).GetHashCode();
    int BadgeNumber = 0;
    private bool IsBusy { get; set; } = false;

    public override IBinder OnBind(Intent intent)
    {
        return null;
    }

    public override StartCommandResult OnStartCommand(Intent intent,
        StartCommandFlags flags, int startId)
    {
        var input = intent.GetStringExtra("inputExtra");

        var notificationIntent = new Intent(this, typeof(MainActivity));
        var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent,
            PendingIntentFlags.Immutable);

        var notification = new NotificationCompat.Builder(this,
                MainApplication.ChannelId)
            .SetContentText(input)
            .SetSmallIcon(Resource.Drawable.AppIconPb)
            .SetContentIntent(pendingIntent);

        timer = new Timer(Timer_Elapsed, notification, 0, 10000);
        //AndroidServiceManager.IsRunning = true;
        //StartScan();

        // You can stop the service from inside the service by calling StopSelf();

        return StartCommandResult.Sticky;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    void Timer_Elapsed(object state)
    {
        AndroidServiceManager.IsRunning = true;

        //try scan here
        StartScan(state);

    }

    void StartScan(object state)
    {
        IBluetoothClient client = Resolver.Resolve<IBluetoothClient>();

        client.ScanResults.CollectionChanged += async (s, e) =>
        {
            if (client.ScanResults.Count > 0)
            {
                client.StopScan();

                await Task.Run(async () =>
                {
                    IsBusy = true;
                    
                    MAUIAndroidFS.Structures.BluetoothDevice device = client.ScanResults.OrderBy(a => a.Rssi).First();

                    bool result = client.Connect(device);
                    await Task.Delay(TimeSpan.FromSeconds(2));

                    client.ReadSession();
                    System.Diagnostics.Debug.WriteLine($"Heart Rate Average:");

                    await Task.Delay(TimeSpan.FromSeconds(2));

                    SendNotification(state);

                });
            }
        };

        Console.WriteLine("Start scan from service.");
        client.StartScan();

        /*Console.WriteLine("Start 5s sleep.");
        Thread.Sleep(5000);

        Console.WriteLine("Stop scan from service.");
        client.StopScan();*/
    }

    void SendNotification(object state)
    {
        var notification = (NotificationCompat.Builder)state;
        // set the number
        notification.SetNumber(5);
        // set the title (text) to Service Running
        notification.SetContentTitle("5 Brushing sessions synced");
        // build and notify
        StartForeground(myId, notification.Build());
    }
}