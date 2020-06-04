using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MongoDB.Bson;
using SD.Client.Services;
using SD.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SD.Client.Pages
{
    public class OtherPageManagerBase : ComponentBase
    {
        public OtherPageModel otherPageModel = new OtherPageModel();

        [Inject]
        public OtherPageService OtherPageService { set; get; }

        [Inject]
        protected LangCodeService LangCodeService { get; set; }


        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Parameter]
        public string Id { get; set; }
        protected bool Registered { get; set; } = true;
        protected string Info { get; set; }
        protected string InfoShowClass { get; set; } = "d-none";
        public List<string> PrimLangs { get; set; }
        public List<string> SecLangs { get; set; }

        protected string TempPrimLang { get; set; }
        protected string TempSecLang { get; set; }

        protected void AddPrimLang(string lang)
        {
            if (!string.IsNullOrWhiteSpace(lang))
            {
                if (PrimLangs != null)
                {
                    if (!PrimLangs.Contains(lang))
                        PrimLangs.Add(lang);
                }
                else
                {
                    PrimLangs = new List<string>();
                    PrimLangs.Add(lang);
                }
            }
        }
        protected void AddSecLang(string lang)
        {
            if (!string.IsNullOrWhiteSpace(lang))
            {
                if (SecLangs != null)
                {
                    if (!SecLangs.Contains(lang))
                        SecLangs.Add(lang);
                }
                else
                {
                    SecLangs = new List<string>();
                    SecLangs.Add(lang);
                }
            }
        }

        protected void RemovePrimLang(string lang)
        {
            PrimLangs.Remove(lang);
        }
        protected void RemoveSecLang(string lang)
        {
            SecLangs.Remove(lang);
        }

        public async Task SaveData()
        {
            if (PrimLangs != null)
            {
                otherPageModel.PrimLangs = string.Join(",", PrimLangs.ToArray());
            }

            if (SecLangs != null)
            {
                otherPageModel.SecLangs = string.Join(",", SecLangs.ToArray());
            }

            try
            {
                if (!Registered)
                {
                    Id = Guid.NewGuid().ToString();
                    otherPageModel.OtherPageId = Id;
                    HttpResponseMessage transObj = await OtherPageService.RegisterOtherPage(otherPageModel);
                    Registered = true;
                    Info = $"your data for {otherPageModel.Host} saved successfully";
                }
                else
                {
                    HttpResponseMessage res = await OtherPageService.UpdateOtherPage(otherPageModel);
                   Info = "Your data updated successfully";
                }
            }
            catch (Exception ex)
            {
                Info = "Check if your data saved successfully please! ";
                // Error
                Info += ex.Message;
            }
            InfoShowClass = "";
        }
        public async Task RemoveOtherPage()
        {
            try
            {
                await OtherPageService.RemoveOtherPage(Id);
                Info = $"{otherPageModel.Host} deleted successfully";
            }
            catch (Exception ex)
            {
                Info = "Check if your data deleted successfully please! ";
                // Error
                Info += ex.Message;
            }
            InfoShowClass = "";
        }
        protected async override Task OnInitializedAsync()
        {
            try
            {
                var user = (await authenticationStateTask).User;
                // add Authorization
                otherPageModel =  await OtherPageService.GetOtherPageById(Id);
            }
            catch (Exception ex)
            {

            }

            if (otherPageModel.OtherPageId == null)
            {
                Registered = false;
            }
            else
            {
                if (otherPageModel.PrimLangs != null)
                {
                    PrimLangs = otherPageModel?.PrimLangs.Split(",").ToList();
                }
                if (otherPageModel.SecLangs != null)
                {
                    SecLangs = otherPageModel?.SecLangs.Split(",").ToList();
                }
            }
        }

    }
}
