using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainPage
{
   public partial class AndroidCallBlockerService : ICallBlockerService
   {
      public void InitializeService() { }
      public void AddToWhitelist(string phoneNumber) { }
      public void RemoveFromWhitelist(string phoneNumber) { }
      public ObservableCollection<string> GetWhitelist() => new ObservableCollection<string>();
   }
}
