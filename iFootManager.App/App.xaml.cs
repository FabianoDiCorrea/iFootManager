namespace iFootManager.App;

public partial class App : Application
{
	public App()
	{
		try
		{
			InitializeComponent();
			MainPage = new NavigationPage(new CoachHubPage());
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"APP CRASH: {ex}");
			Console.WriteLine($"APP CRASH: {ex}");
			throw;
		}
	}
}
