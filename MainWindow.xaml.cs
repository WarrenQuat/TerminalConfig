using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;


namespace TerminalConfig
{
    //global properties
    public class Holder
    {
        public string schema { get; set; }

        public string defaultProfile { get; set; }

        public List<Profile> profiles { get; set; }

    }
    //profile class for json
    public class Profile
    {
        public Profile()
        {
            this.useAcrylic = true;
            this.fontFace = "Consolas";
            this.fontSize = 12;
            hidden = false;
        }

        public string guid { get; set; }
        public string name { get; set; }
        public string source { get; set; }
        public string commandline { get; set; }
        public bool? hidden { get; set; }
        public string backgroundImage { get; set; }
        public string colorScheme { get; set; }
        public string fontFace { get; set; }
        public int fontSize { get; set; }
        public string background { get; set;}
        public float acrylicOpacity { get; set; }
        public bool? useAcrylic { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Profile current;
        Holder profiles;
        string pathName;
        public string[] ColorSchemes = {"Campbell", "One Half Dark", "One Half Light",
                                        "Solarized Dark", "Solarized Light"}; 
        public MainWindow()
        {
            InitializeButtons();
            //get profiles files
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            pathName = path + "/Packages/Microsoft.WindowsTerminal_8wekyb3d8bbwe/LocalState/profiles.json";
            string tempPath = path + "/Packages/Microsoft.WindowsTerminal_8wekyb3d8bbwe/LocalState/temp.json";

            //remove all comments from file and $ from schema
            using (var sr = new StreamReader(pathName))
            using (var sw = new StreamWriter(tempPath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = Regex.Replace(line,@"\$", "");
                    //MessageBox.Show(line);
                    Regex re = new Regex("//w*");
                    if (!re.IsMatch(line))
                        sw.WriteLine(line);
                }
            }
            //write temp to path and delete temp
            File.Delete(pathName);
            File.Move(tempPath,pathName);
            
            using (StreamReader r = new StreamReader(pathName))
            {
                string json = r.ReadToEnd();
                profiles = JsonConvert.DeserializeObject<Holder>(json);
            }
            //add all retrieved profiles to the comboBox
            List<Profile> menuProf = profiles.profiles;
            foreach (Profile prof in menuProf)
                ProfileBox.Items.Add(prof.name);

        }

        private void ProfileBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            int chosen = ProfileBox.SelectedIndex;
            current = profiles.profiles[chosen];

            //set all fields to current values
            ColorSchemeBox.SelectedItem = current.colorScheme;
            Acrylicslider.Value = current.acrylicOpacity * 100;
            FontComboBox.SelectedItem = current.fontFace;
            FontCombo.SelectedItem = current.fontSize;
            if (current.backgroundImage != null)
                ImageCheckBox.IsChecked = true;
            else
            {
                ImageCheckBox.IsChecked = false;
                BrowseImage.IsEnabled = false;
            }
            HiddenCheck.IsChecked = (bool)current.hidden;

            //set checkbox
            if (current.guid == profiles.defaultProfile)
                SetDefault.IsChecked = true;
            else
                SetDefault.IsChecked = false;

            FontCombo.SelectedItem = current.fontSize;

            Color color;
            if (current.background != null)
                color = (Color)ColorConverter.ConvertFromString(current.background);
            else
                color = new Color();
            ColorBackground.SelectedColor = color;

            //enable buttons
            ColorSchemeBox.IsEnabled = true;
            Acrylicslider.IsEnabled = true;
            FontCombo.IsEnabled = true; ;
            ColorBackground.IsEnabled = true;
            SetDefault.IsEnabled = true;
            FontComboBox.IsEnabled = true;
            ImageCheckBox.IsEnabled = true;
            HiddenCheck.IsEnabled = true;
        }

        private void ColorSchemeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int chosen;
            if (ColorSchemeBox.SelectedIndex != -1)
                chosen = ColorSchemeBox.SelectedIndex;
            else
                chosen = 0;
            current.colorScheme = ColorSchemes[chosen];

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string json = JsonConvert.SerializeObject(profiles,Formatting.Indented,
                                                        new JsonSerializerSettings{
                                                        NullValueHandling = NullValueHandling.Ignore
                                                        });

            //write string to file
            json = Regex.Replace(json, "\"schema", "\"$schema");
            string tempPath = @"test.json";
            System.IO.File.WriteAllText(tempPath, json);
            File.Delete(pathName);
            File.Move(tempPath, pathName);
        }

        private void Acrylicslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            current.acrylicOpacity = (float)Acrylicslider.Value / 100;
        }

        private void FontCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            current.fontSize = (int)FontCombo.SelectedItem;
        }

        private void ColorBackground_SelectedColorchanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            string Text = ColorBackground.SelectedColor.ToString();
            Text = Text.Remove(1, 2);
            current.background = Text;
            ColorName.Text = Text;
        }

        private void SetDefault_Checked(object sender, RoutedEventArgs e)
        {
            HiddenCheck.IsChecked = false;
            current.hidden = false;
            profiles.defaultProfile = null;
            profiles.defaultProfile = current.guid;
        }

        private void ColorText_SelectedColorchanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            string Text = ColorBackground.SelectedColor.ToString();
            Text = Text.Remove(1, 2);
            current.background = Text;
            ColorName.Text = Text;
        }

        private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            current.fontFace = FontComboBox.SelectedItem.ToString();
        }


        private void BrowseImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Image Files(*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF|All files (*.*)|*.*";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                current.backgroundImage = filename;
            }
        }
        private void ImageCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)ImageCheckBox.IsChecked)
            {
                BrowseImage.IsEnabled = true;

            }
            else
            {
                current.backgroundImage = null;
                BrowseImage.IsEnabled = false;
            }
        }

        private void HiddenCheck_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)HiddenCheck.IsChecked)
            {
                current.hidden = true;
                profiles.defaultProfile = null;
                SetDefault.IsChecked = false;
            }
            else
                current.hidden = false;
        }

        public void InitializeButtons()
        {
            //initialize menu with profiles
            //remove comments from profiles.json
            InitializeComponent();
            ColorSchemeBox.IsEnabled = false;
            Acrylicslider.IsEnabled = false;
            FontCombo.IsEnabled = false;
            ColorBackground.IsEnabled = false;
            SetDefault.IsEnabled = false;
            FontComboBox.IsEnabled = false;
            BrowseImage.IsEnabled = false;
            ImageCheckBox.IsEnabled = false;
            HiddenCheck.IsEnabled = false;

            //add colorschemes to drop down
            foreach (string s in ColorSchemes)
                ColorSchemeBox.Items.Add(s);

            //add font sizes
            for (int i = 8; i < 50; i++)
                FontCombo.Items.Add(i);

            foreach (System.Drawing.FontFamily font in System.Drawing.FontFamily.Families)
            {
                FontComboBox.Items.Add(font.Name);
            }
        }

    }
}
