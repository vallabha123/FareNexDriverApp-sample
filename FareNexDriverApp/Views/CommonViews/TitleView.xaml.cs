namespace FareNexDriverApp.Views.CommonViews;

public partial class TitleView : ContentView
{

	public static readonly BindableProperty TitleProperty = BindableProperty
		.Create(nameof(Title),
		typeof(string),
		typeof(TitleView),
		string.Empty);
	public string Title
	{
		get { return (string)GetValue(TitleProperty);}
		set { SetValue(TitleProperty, value); }
	}
	public TitleView()
	{
		InitializeComponent();
	}
}