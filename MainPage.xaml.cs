using System.Globalization;

namespace CalculatriceMaui;

public partial class MainPage : ContentPage
{
    // État de la calculatrice
    private double? _premierNombre = null;   // opérande mémorisé
    private string _operateur = "";           // opérateur en attente
    private bool _nouvelleSaisie = true;      // true = le prochain chiffre remplace l'affichage
    private readonly CultureInfo _ci = CultureInfo.InvariantCulture; // point comme séparateur décimal

    public MainPage()
    {
        InitializeComponent();
    }

    // ---------- Saisie ----------

    private void OnChiffre(object sender, EventArgs e)
    {
        var chiffre = ((Button)sender).Text;
        if (LblResultat.Text == "Erreur") Reinitialiser();

        if (_nouvelleSaisie || LblResultat.Text == "0")
        {
            LblResultat.Text = chiffre;
            _nouvelleSaisie = false;
        }
        else if (LblResultat.Text.Length < 15) // limite de longueur
        {
            LblResultat.Text += chiffre;
        }
    }

    private void OnVirgule(object sender, EventArgs e)
    {
        if (LblResultat.Text == "Erreur") Reinitialiser();

        if (_nouvelleSaisie)
        {
            LblResultat.Text = "0.";
            _nouvelleSaisie = false;
        }
        else if (!LblResultat.Text.Contains('.')) // une seule virgule par nombre
        {
            LblResultat.Text += ".";
        }
    }

    // ---------- Opérations ----------

    private void OnOperateur(object sender, EventArgs e)
    {
        if (LblResultat.Text == "Erreur") return;
        double valeur = LireAffichage();

        // Enchaînement : 2 + 3 + ... calcule le résultat intermédiaire
        if (_premierNombre.HasValue && !_nouvelleSaisie)
        {
            var res = Calculer(_premierNombre.Value, valeur, _operateur);
            if (res is null) { AfficherErreur(); return; }
            valeur = res.Value;
            LblResultat.Text = Formater(valeur);
        }

        _premierNombre = valeur;
        _operateur = ((Button)sender).Text;
        LblOperation.Text = $"{Formater(valeur)} {_operateur}";
        _nouvelleSaisie = true;
    }

    private void OnEgal(object sender, EventArgs e)
    {
        if (!_premierNombre.HasValue || LblResultat.Text == "Erreur") return;

        double second = LireAffichage();
        LblOperation.Text = $"{Formater(_premierNombre.Value)} {_operateur} {Formater(second)} =";

        var res = Calculer(_premierNombre.Value, second, _operateur);
        if (res is null) { AfficherErreur(); return; }

        LblResultat.Text = Formater(res.Value);
        _premierNombre = null;
        _operateur = "";
        _nouvelleSaisie = true;
    }

    // Renvoie null en cas de division par zéro (gérée sans plantage)
    private double? Calculer(double a, double b, string op)
    {
        switch (op)
        {
            case "+": return a + b;
            case "−": return a - b;
            case "×": return a * b;
            case "÷": return b == 0 ? (double?)null : a / b;
            default: return b;
        }
    }

    // ---------- Fonctions ----------

    private void OnAC(object sender, EventArgs e) => Reinitialiser();

    private void OnEffacer(object sender, EventArgs e)
    {
        if (_nouvelleSaisie || LblResultat.Text == "Erreur") return;

        var texte = LblResultat.Text;
        texte = texte.Length > 1 ? texte[..^1] : "0";
        if (texte == "-") texte = "0";
        LblResultat.Text = texte;
    }

    private void OnSigne(object sender, EventArgs e)
    {
        if (LblResultat.Text == "Erreur" || LblResultat.Text == "0") return;

        LblResultat.Text = LblResultat.Text.StartsWith("-")
            ? LblResultat.Text[1..]
            : "-" + LblResultat.Text;
    }

    private void OnPourcent(object sender, EventArgs e)
    {
        if (LblResultat.Text == "Erreur") return;

        double x = LireAffichage();
        // 200 + 10 % => 200 + 20 ; sinon 10 % => 0.1
        double r = (_premierNombre.HasValue && (_operateur == "+" || _operateur == "−"))
            ? _premierNombre.Value * x / 100
            : x / 100;

        LblResultat.Text = Formater(r);
        _nouvelleSaisie = false;
    }

    // ---------- Utilitaires ----------

    private void Reinitialiser()
    {
        _premierNombre = null;
        _operateur = "";
        _nouvelleSaisie = true;
        LblResultat.Text = "0";
        LblOperation.Text = "";
    }

    private void AfficherErreur()
    {
        LblResultat.Text = "Erreur";
        LblOperation.Text = "Division par zéro impossible";
        _premierNombre = null;
        _operateur = "";
        _nouvelleSaisie = true;
    }

    private double LireAffichage() => double.Parse(LblResultat.Text, _ci);

    private string Formater(double v)
    {
        if (double.IsInfinity(v) || double.IsNaN(v)) return "Erreur";
        return Math.Round(v, 10).ToString("G12", _ci);
    }
}
