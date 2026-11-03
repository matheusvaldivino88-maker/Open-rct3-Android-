namespace RCT3AndroidTest;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_main);

        var sandboxButton = FindViewById<Button>(Resource.Id.button_sandbox);

        sandboxButton!.Click += (sender, e) =>
        {
            StartActivity(new Intent(this, typeof(SandboxActivity)));
        };
    }
}
