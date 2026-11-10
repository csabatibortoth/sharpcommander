namespace sharpcommander;

public partial class TestForm : Form
{
    public TestForm()
    {
        InitializeComponent();
        string path = "C:/";

        foreach (string elem in Directory.GetFileSystemEntries(path))
        {
            string name = Path.GetFileNameWithoutExtension(elem);
            string kiterjesztés = Path.GetExtension(elem);

            listView1.Items.Add(name).SubItems.Add(kiterjesztés == "" ? "DIR" : kiterjesztés);
        }
    }
}
