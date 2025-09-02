using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using sd.Client.Services;
using sd.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace sd.Client.Pages
{
    public class OtherPageManagerBase : ComponentBase
    {
        public OtherPageModel otherPageModel = new OtherPageModel();

        [Inject]
        public ILogger<OtherPageManagerBase> Log { get; set; }

        [Inject]
        protected ApiService ApiService { get; set; }

        [Inject]
        protected LangCodeService LangCodeService { get; set; }

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
                    HttpResponseMessage transObj = await ApiService.PostAsync<HttpResponseMessage>("api/OtherPage", otherPageModel);
                    Registered = true;
                    Info = $"your data for {otherPageModel.Host} saved successfully";
                }
                else
                {
                    HttpResponseMessage res = await ApiService.PutAsync<HttpResponseMessage>("api/OtherPage", otherPageModel);
                    Info = "Your data updated successfully";
                }
            }
            catch (Exception ex)
            {
                Info = "Check if your data saved successfully please! ";
                // Error
                Info += ex.Message;

                Log.LogError(ex.Message);
            }
            InfoShowClass = "";
        }
        public async Task RemoveOtherPage()
        {
            try
            {
                await ApiService.DeleteAsync($"api/OtherPage/?id={Id}");
                Info = $"{otherPageModel.Host} deleted successfully";
            }
            catch (Exception ex)
            {
                Info = "Check if your data deleted successfully please! ";
                // Error
                Info += ex.Message;
                Log.LogError(ex.Message);
            }
            InfoShowClass = "";
        }
        protected async override Task OnInitializedAsync()
        {
            otherPageModel = await ApiService.GetAsync<OtherPageModel>($"api/OtherPage/{Id}");

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
