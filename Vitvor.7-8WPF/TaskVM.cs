using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Serialization;

namespace Vitvor._7_8WPF
{
    class TaskVM : INotifyPropertyChanged
    {
        private MainWindow _mainWindow;
        private Stack<ObservableCollection<Task>> undos = new Stack<ObservableCollection<Task>>();
        private Stack<ObservableCollection<Task>> redos = new Stack<ObservableCollection<Task>>();
        public ObservableCollection<Task> TODO { get; set; } = new ObservableCollection<Task>();
        public ObservableCollection<Task> TODOSearch { get; set; } = new ObservableCollection<Task>();
        private Task _todo;
        public Task SelectedTask
        {
            get
            {
                return _todo;
            }
            set
            {
                _todo = value;
                OnPropertyChanged("SelectedTask");
            }
        }
        private RelayCommand _add;
        public RelayCommand Add
        {
            get
            {
                return _add ??
                    (_add = new RelayCommand(obj =>
                      {
                          TODO.Add(new Task());
                          SaveWindow();
                      }));
            }
        }
        private RelayCommand _delete;
        public RelayCommand Delete
        {
            get
            {
                return _delete ??
                    (_delete = new RelayCommand(obj =>
                     {
                         Task task = obj as Task;
                         if (task != null)
                         {
                             TODO.Remove(task);
                         }
                         SaveWindow();
                     }));
            }
        }
        private RelayCommand _search;
        public RelayCommand Search
        {
            get
            {
                return _search ??
                    (_search = new RelayCommand(obj =>
                      {
                          string part = obj as string;
                          if (part != null)
                          {
                              if (TODOSearch.Count < TODO.Count)
                              {
                                  foreach (var i in TODO)
                                  {
                                      TODOSearch.Add(i);
                                  }
                              }
                              if (part.Equals("Category"))
                              {
                                  TODO.Clear();
                                  var search = from i in TODOSearch where _mainWindow.Category.SelectedItem.ToString().Contains(i.Category) select i;
                                  foreach (var i in search)
                                  {
                                      TODO.Add(i);
                                  }
                                  
                              }
                              else if (part.Equals("Priority"))
                              {
                                  TODO.Clear();
                                  var search = from i in TODOSearch where _mainWindow.Priority.SelectedItem.ToString().Contains(i.Priority) select i;
                                  foreach (var i in search)
                                  {
                                      TODO.Add(i);
                                  }
                              }
                              else if (part.Equals("Date"))
                              {
                                  if (_mainWindow.Date.Text.Length == 10 && !_mainWindow.Date.Text.Contains('_'))
                                  {
                                      TODO.Clear();
                                      var search = from i in TODOSearch where i.Date == Convert.ToDateTime(_mainWindow.Date.Text) select i;
                                      foreach (var i in search)
                                      {
                                          TODO.Add(i);
                                      }
                                  }
                              }
                          }
                          SaveWindow();
                      }));
            }
        }
        private RelayCommand _reset;
        public RelayCommand Reset
        {
            get
            {
                return _reset ??
                    (_reset = new RelayCommand(obj =>
                      {
                          TODO.Clear();
                          foreach(var i in TODOSearch)
                          {
                              TODO.Add(i);
                          }
                          TODOSearch.Clear();
                          _mainWindow.Date.Clear();
                          _mainWindow.Priority.Text = "";
                          _mainWindow.Category.Text = "";
                          SaveWindow();
                      }));
            }
        }
        private RelayCommand _save;
        public RelayCommand Save
        {
            get
            {
                return _save ??
                    (_save = new RelayCommand(obj =>
                     {
                         if(TODO.Count<TODOSearch.Count)
                         {
                             TODO.Clear();
                             foreach (var i in TODOSearch)
                             {
                                 TODO.Add(i);
                             }
                         }
                         XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<Task>));
                         using(FileStream fs=new FileStream("save.xml", FileMode.OpenOrCreate))
                         {
                             xmlSerializer.Serialize(fs, TODO);
                         }
                         SaveWindow();
                     }));
            }
        }
        private RelayCommand _load;
        public RelayCommand Load
        {
            get
            {
                return _load ??
                    (_load = new RelayCommand(obj =>
                      {
                          XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<Task>));
                          using(FileStream fs=new FileStream("save.xml", FileMode.OpenOrCreate))
                          {
                              ObservableCollection<Task> tasks=(ObservableCollection<Task>)xmlSerializer.Deserialize(fs);
                              foreach(var i in tasks)
                              {
                                  if(!TODO.Any(u=>u.Category==i.Category && u.Date==i.Date && u.Duration==i.Duration && 
                                  u.FullDescription==i.FullDescription && u.Name==i.Name && u.Periodicity==i.Periodicity && u.Priority==i.Priority && u.State==i.State))
                                      TODO.Add(i);
                              }
                          }
                          SaveWindow();
                      }));
            }
        }
        private RelayCommand _changeTheme;
        public RelayCommand ChangeTheme
        {
            get
            {
                return _changeTheme ??
                    (_changeTheme = new RelayCommand(obj =>
                      {
                          string style = _mainWindow.Themes.SelectedItem.ToString().Remove(0, 38) + "Theme";
                          // определяем путь к файлу ресурсов
                          var uri = new Uri(style + ".xaml", UriKind.Relative);
                          // загружаем словарь ресурсов
                          ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                          ResourceDictionary oldDict = (from d in Application.Current.Resources.MergedDictionaries
                                                        where d.Source != null && d.Source.OriginalString.Contains("Theme")
                                                        select d).FirstOrDefault();
                          // очищаем коллекцию ресурсов приложения
                          if (oldDict != null)
                          {
                              int ind = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                              Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                              Application.Current.Resources.MergedDictionaries.Insert(ind, resourceDict);
                          }
                          else
                          {
                              Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                          }
                      }));
            }
        }
        private RelayCommand _undo;
        public RelayCommand Undo
        {
            get
            {
                return _undo ??
                    (_undo = new RelayCommand(obj =>
                      {
                          if (undos.Count > 0)
                          {
                              TODO.Clear();
                              foreach(var i in undos.Peek())
                              {
                                  TODO.Add(i);
                              }
                              redos.Push(undos.Pop());
                          }
                          else
                          {
                              MessageBox.Show("Нечего отменять!");
                          }
                      }));
            }
        }
        private RelayCommand _redo;
        public RelayCommand Redo
        {
            get
            {
                return _redo ??
                    (_redo = new RelayCommand(obj =>
                      {
                          if (redos.Count > 1)
                          {
                              undos.Push(redos.Pop());
                              TODO.Clear();
                              foreach (var i in redos.Peek())
                              {
                                  TODO.Add(i);
                              }
                          }
                          else
                          {
                              MessageBox.Show("Не к чему переходить!");
                          }
                      }));
            }
        }
        public RelayCommand command;
        private RelayCommand Command
        {
            get
            {
                return command ??
                    (command = new RelayCommand(obj =>
                      {
                          _mainWindow.FullInf.Text = "You just click custom control";
                      }));
            }
        }
        private void SaveWindow()
        {
            ObservableCollection<Task> tasks = new ObservableCollection<Task>();
            foreach(var i in TODO)
            {
                tasks.Add(i);
            }
            undos.Push(tasks);
        }
        public TaskVM(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
