using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DocToHTML.Model;

using DocToHTML.Extention;
using System.Collections.ObjectModel;

namespace DocToHTML.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public string Title { get; set; } = "Конвертация MS Word в HTML";
        private bool _isWait;
        public bool IsWait
        {
            get { return _isWait; }
            set { _isWait = value; OnPropertyChanged("IsWait"); }
        }

       

        private DocHtml docHtml = new DocHtml();
        public DocHtml DocHtml
        {
            get { return docHtml; }
            set { docHtml = value; OnPropertyChanged("DocHtml"); }
        }


        private string _buzyText;
        public string BuzyText
        {
            get { return _buzyText; }
            set { _buzyText = value; OnPropertyChanged("BuzyText"); }
        }

        public string FileDoc
        {
            get { return Properties.Settings.Default.fileDoc; }
            set
            {
                Properties.Settings.Default.fileDoc = value;
                OnPropertyChanged("FileDoc");
                Properties.Settings.Default.Save();
            }
        }

        public string FileHtml
        {
            get { return Properties.Settings.Default.fileHtml; }
            set
            {
                Properties.Settings.Default.fileHtml = value;
                OnPropertyChanged("FileHtml");
                Properties.Settings.Default.Save();
            }
        }

        private ObservableCollection<string> _warnings = new ObservableCollection<string>();
        public ObservableCollection<string> Warnings
        {
            get { return _warnings; }
            set { _warnings = value; }
        }
        public string SelectedWarning { get; set; }


        public ICommand SelectFileDocCommand => new Command(
       _ =>
       {
           DialogService dlg = new DialogService();
           dlg.File = FileDoc;
           if (dlg.OpenFileDialog("MS Word(*.doc*)|*.doc*|All files (*.*)|*.*"))
           {
               FileDoc = dlg.File;
               FileHtml = FileDoc.Replace(".docx", ".html");
           }
       });

        public ICommand SelectFileHtmlCommand => new Command(
       _ =>
       {
           DialogService dlg = new DialogService();
           dlg.File = FileHtml;
           if (dlg.OpenFileDialog("HTML(*.htm*)|*.htm*|All files (*.*)|*.*"))
           {
               FileHtml = dlg.File;
           }
       });

        public ICommand ConvertDocToHtmlCommand => new Command(
       _ =>
       {
           Task task = ConvertDocToHtmlAsync();
       },
       _ => { return IsWait == false; });


        private async Task ConvertDocToHtmlAsync()
        {
            try
            {
                if (IsWait)
                {
                    return;
                }
                IsWait = true;
                BuzyText = "Конвертация MS Word в HTML...";

                string html = string.Empty;
                Warnings.Clear();
                await Task.Run(() =>
                {
                    if (DocHtml.ConvertDocToHtml(FileDoc) && DocHtml.ProcessingHTML())
                    {

                        DocHtml.WriteToFile(FileHtml);

                    }


                });
                if (DocHtml.Warnings.Count != 0)
                {
                    foreach (string item in DocHtml.Warnings)
                    {
                        Warnings.Add(item);
                    }
                    SelectedWarning = Warnings[0];
                }



            }
            catch (Exception ex)
            {
                Log.Instance.Write(ex);
            }
            IsWait = false;
            BuzyText = "Завершено.";
        }

        public MainViewModel()
        {
            string major=System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString();
            string minor=System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();

            Title += $" ({major}.{minor})";

        }


    }
}
