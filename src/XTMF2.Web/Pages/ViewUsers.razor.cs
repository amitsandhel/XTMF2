using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using XTMF2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using XTMF2.Web.Controllers;

namespace XTMF2.Web.Pages
{
    public partial class ViewUsers
    {
        public ReadOnlyObservableCollection<User> Users = Server.Runtime.UserController.Users;


        protected override void OnInitialized()
        {
            base.OnInitialized();
            ((INotifyCollectionChanged)Users).CollectionChanged += ViewUsers_CollectionChanged;
        }

        private void ViewUsers_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        public void NewUser()
        {
            Server.Runtime.UserController.CreateNew("FromASP", false, out var _, out var _);
        }
    }
}
