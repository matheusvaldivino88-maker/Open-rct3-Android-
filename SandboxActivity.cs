using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace RCT3AndroidTest;

[Activity(Label = "Modo Sandbox")]
public class SandboxActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var tela = new LinearLayout(this);
        tela.Orientation = Orientation.Vertical;
        tela.Gravity = GravityFlags.Center;
        tela.SetPadding(30, 30, 30, 30);
        tela.SetBackgroundColor(Color.ParseColor("#10251C"));

        var titulo = new TextView(this);
        titulo.Text = "MODO SANDBOX";
        titulo.TextSize = 30;
        titulo.Gravity = GravityFlags.Center;
        titulo.SetTextColor(Color.ParseColor("#F4D27A"));

        var mensagem = new TextView(this);
        mensagem.Text = "Seu parque está esperando por você!\n\nA construção do parque será adicionada nas próximas etapas.";
        mensagem.TextSize = 17;
        mensagem.Gravity = GravityFlags.Center;
        mensagem.SetTextColor(Color.White);

        var voltar = new Button(this);
        voltar.Text = "Voltar ao menu";
        voltar.Click += (s, e) => Finish();

        tela.AddView(titulo);
        tela.AddView(mensagem);
        tela.AddView(voltar);

        SetContentView(tela);
    }
}
