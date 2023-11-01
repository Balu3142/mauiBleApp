using MAUIAndroidFS.Core;
using Shiny.BluetoothLE.Hosting;
using Shiny.BluetoothLE;

namespace MAUIAndroidFS;

public partial class App : Application
{
    private readonly IBleManager _bleManager;
    
    public App(IBleManager bleManager)
    {
        // Store to the readonly variable (resolved classes/services)
        _bleManager = bleManager;
        
        InitializeComponent();

        // Load the dependency injection
        Resolver.Build();

        MainPage = new MainPage();
	}

    public IBleManager BluetoothLE => _bleManager;
}
