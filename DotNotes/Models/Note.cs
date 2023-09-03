using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DotNotes.Models
{
    public class Note : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateOnly Date { get; set; }
        public string Body { get; set; }
        public FileInfo FileInfo { get; set; }
        public bool IsSelected { get; set; }

        public Note(int id, string name, DateOnly date, string body, FileInfo fileInfo)
        {
            Id = id;
            Name = name;
            Date = date;
            Body = body;
            FileInfo = fileInfo;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
